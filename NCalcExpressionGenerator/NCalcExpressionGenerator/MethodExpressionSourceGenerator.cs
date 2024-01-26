using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NCalcExpressionGenerator.Extensions;
using NCalcExpressionGenerator.Models;
using NCalcExpressionGenerator.Syntax.Receivers;

namespace NCalcExpressionGenerator;



/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator]
public class MethodExpressionSourceGenerator : ISourceGenerator
{
    /// <summary>
    /// Regular expression to find parameters on annotated method string expressions
    ///</summary>
    private static readonly Regex MethodParametersRegex = new(@"\[([^\]]+)\]");
    
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register an attribute syntax receiver in order to get the valid annotated method declarations
        context.RegisterForSyntaxNotifications(() => new MethodExpressionSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Retrieve the syntax receiver
        if (!(context.SyntaxReceiver is MethodExpressionSyntaxReceiver syntaxReceiver))
            return;
        
        // Group methods by containing class
        var methodsByClass = syntaxReceiver.Methods.GroupBy(methodSyntax =>
        {
            // Finding the annotated method containing class name to use as reference to generate the annotated expression implementation
            var classSyntax = methodSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            // Retrieving the parent namespace of the class that contains the annotated method to include on generated code
            var namespaceSyntax = methodSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            return (classSyntax?.Identifier.Text, namespaceSyntax?.Name.ToString());
        });
        
        // Generate code for each class declaration found
        foreach (var classGroup in methodsByClass)
        {
            var (className, namespaceName) = classGroup.Key;

            // Generate the implementation syntax tree
            var implementationSyntaxTree = GenerateImplementationSyntaxTree(className, namespaceName, classGroup);

            // Add the generated code to the compilation
            context.AddSource($"{className}.g.cs", implementationSyntaxTree);
        }
    }
    
    /// <summary>
    /// Returns the value contained on marker MethodExpressionAttribute given a method declaration 
    /// </summary>
    /// <param name="methodSyntax"></param>
    /// <returns></returns>
    private string GetMethodExpression(MethodDeclarationSyntax methodSyntax)
    {
        // Retrieve the MethodExpressionAttribute
        // Only the first attribute found is considered since only one expression 
        // is considered valid to generate the annotated method implementation
        var attribute = methodSyntax.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .FirstOrDefault(attribute => attribute.Name.ToString() == "MethodExpression");

        // Retrieve the expression string from the attribute found if any
        var expression = attribute?.ArgumentList?.Arguments.FirstOrDefault()?.ToString();

        // TODO: Emit a error diagnostic and interrupt source generator pipeline execution since none expression was found and consequentily, no implementation will be provided to annotated method if the execution continue
        return expression ?? "/* No valid expression found */";
    }
    
    /// <summary>
    /// Returns method declaration arguments information for syntax tree parameters validation before code generation
    /// </summary>
    /// <param name="methodSyntax"></param>
    /// <returns></returns>
    private List<ArgumentInfo> GetMethodParameterInfo(MethodDeclarationSyntax methodSyntax)
    {
        // Retrieve parameter information about its name and declared type to perform validation with the attribute expression provided
        var parameters = methodSyntax.ParameterList?.Parameters ?? Enumerable.Empty<ParameterSyntax>();

        return parameters.Select(param => new ArgumentInfo(param.Identifier.Text, param.Type.ToString())).ToList();
    }
    
    private string GenerateImplementationSyntaxTree(string className,string namespaceName, IEnumerable<MethodDeclarationSyntax> methods)
    {
        // Creating the compilation unit syntax factory to generate class code
        CompilationUnitSyntax syntaxFactory = SyntaxFactory.CompilationUnit();
        
        var declarations = methods.Select(methodSyntax =>
        {
            // Catching annotated method display string
            var methodName = methodSyntax.Identifier.Text;
            
            // Reading string expression representing math equation on marker attribute
            var expression = GetMethodExpression(methodSyntax);
            
            // Storing the annotated method parameters information to generate further validations
            var parameterInfo = GetMethodParameterInfo(methodSyntax);
            
            // Constructing method declarations declarations for each individual annotated method
            // Since all math operations will return a commom type of value, the double return type was choosen
            // to generate the contents
            var methodDeclaration = MethodDeclarationSyntaxExtensions.CreateMethodDeclaration(
                methodName,
                TypeDeclarationSyntaxExtensions.CreateTypeDeclaration("double"),
                [
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.PartialKeyword
                ],
                parameterInfo.Select(param => ParameterSyntaxExtensions.CreateParameter(param.Name, param.Type)).ToArray()
            );
            
            // To generate the parameters assignment, the NCalc expression approach is used
            // This approach consists in a existing dictionary for a given expression, where each element is a single parameter declared on the string expression content present on marker attribute
            // Since the objective is to inform the NCalc which value has to be assigned on each parameter, a regex is passed on expression to check for the parameters expecting all valid parameters to be enclosed by square brackets like [param_name]
            MatchCollection matches = MethodParametersRegex.Matches(expression);
            MatchCollection parametersContainedOnExpression = matches;
            const string expressionVariable = "exp";
            
            IEnumerable<ExpressionStatementSyntax> parameterAssignmentExpressions =
                parametersContainedOnExpression.Count > 0
                    ? matches
                        .Cast<Match>()
                        .Select(match => match.Value)
                        .Distinct()
                        .Select(val => CompilationUnitExtensions.CreateExpressionParameterAssignmentSyntax(
                            expressionVariable,
                            val.Replace("[",string.Empty).Replace("]",string.Empty),
                            val.Replace("[",string.Empty).Replace("]",string.Empty)))
                    : [];
            
            // TODO: Raise diagnostics in case the parameters count do not coincide with base annotated method parameters

            methodDeclaration = methodDeclaration
                .WithBody(
                    BlockSyntaxExtensions.CreateBlock(CompilationUnitExtensions.CreateExpressionDeclarationSyntax("exp", expression))
                        .AddStatements(parameterAssignmentExpressions.ToArray())
                        .AddStatements(SyntaxFactory.ExpressionStatement(CompilationUnitExtensions.CreateEvaluationExpressionSyntax("exp")))
                        .AddStatements(SyntaxFactory.ReturnStatement(
                            SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Convert"),
                                        SyntaxFactory.IdentifierName("ToDouble")))
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.IdentifierName("result"))))
                                        .WithCloseParenToken(
                                            SyntaxFactory.Token(
                                                SyntaxFactory.TriviaList(),
                                                SyntaxKind.CloseParenToken,
                                                SyntaxFactory.TriviaList(
                                                    SyntaxFactory.Trivia(
                                                        SyntaxFactory.SkippedTokensTrivia())))))
                                .NormalizeWhitespace()
                            ))
                );

            return (MemberDeclarationSyntax)methodDeclaration;
        }).ToList();

        ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration(className);

        classDeclaration = declarations.Aggregate(classDeclaration, (current, declaration) => current.AddMembers(declaration));
        classDeclaration = classDeclaration
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        
        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName))
            
        .AddMembers(classDeclaration);
        
        syntaxFactory = syntaxFactory
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("NCalc")))
            .AddMembers(namespaceDeclaration);

    return syntaxFactory.NormalizeWhitespace().ToFullString();
}

    
}
