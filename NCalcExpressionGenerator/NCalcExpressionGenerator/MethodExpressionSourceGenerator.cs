using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using NCalcExpressionGenerator.Abstractions;
using NCalcExpressionGenerator.Extensions;
using NCalcExpressionGenerator.Models;
using NCalcExpressionGenerator.Syntax.Receivers;

namespace NCalcExpressionGenerator;



/// <summary>
/// A sample source generator that creates C# classes based on the text file (in this case, Domain Driven Design ubiquitous language registry).
/// When using a simple text file as a baseline, we can create a non-incremental source generator.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class MethodExpressionSourceGenerator : 
    ExpressionGeneratorBase<MethodDeclarationInfo>
{
    #region Properties

    /// <summary>
    /// Default generated members expression variable prefix to add on method implementation body
    /// </summary>
    private const string ExpressionVariable = "exp";

    #endregion

    #region Constructors

    public MethodExpressionSourceGenerator()
    : base(
        "NCalcExpressionParserTestApp.MethodExpressionAttribute",
        new MethodExpressionSyntaxReceiver(),
        Structures.Enumerations.ParameterSeparatorOptions.SquareBrackets
    )
    {
    }

    #endregion

    #region ExpressionGeneratorBase

    private protected override IncrementalValuesProvider<MethodDeclarationInfo> GatherGenerationInfo(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                (syntaxNode, cancellationToken) => syntaxNode is MethodDeclarationSyntax methodDeclaration,
                (ctx, cancellationToken) => FilterDeclarations(ctx))
            .Where(t => t is not null)
            .Select((declarationInfo, _) => declarationInfo);
    }

    private protected override MethodDeclarationInfo FilterDeclarations(GeneratorSyntaxContext context)
    {
        MethodDeclarationSyntax methodSyntax = (MethodDeclarationSyntax)context.Node;
        
        // Finding the annotated method containing class name to use as reference to generate the annotated expression implementation
        ClassDeclarationSyntax classSyntax = methodSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        NamespaceDeclarationSyntax namespaceDeclarationSyntax = methodSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        
        // Since the generated code implementation will basically be a extension of annotated method
        // The syntax receiver is only interested in methods declared as partial methods
        // Also considering that methods must be created as public in order to be a valid declaration for code generation
        if(!(methodSyntax.Modifiers.AsEnumerable().Any(modf => modf.IsKind(SyntaxKind.PublicKeyword))
             || methodSyntax.Modifiers.AsEnumerable().Any(modf => modf.IsKind(SyntaxKind.VirtualKeyword))))
            return default;
        
        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in methodSyntax.AttributeLists)
        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue; // if we can't get the symbol, ignore it
            
            // Seeking only for classes implementing the MethodExpressionAttribute
            INamedTypeSymbol attributeSymbolContainingType = attributeSymbol.ContainingType;
            string attributeSymbolName = attributeSymbolContainingType.Name;
            
            if (!attributeSymbolName.Equals(AttributeName, StringComparison.OrdinalIgnoreCase))
                continue;

            string attributeDisplayName = attributeSymbolContainingType.ToDisplayString();

            // Check the full name of the [ProxyGenerated] attribute.
            string parentNamespace = attributeSymbolContainingType.ContainingNamespace.ToString();
            if (!(attributeSymbolName == $"{AttributeName}"
                  && parentNamespace == $"{Namespace}"))
                continue;
            
            return new MethodDeclarationInfo(classSyntax.Identifier.Text, namespaceDeclarationSyntax.Name.ToString(),methodSyntax);
        }

        return default;

    }

    private protected override void RegisterSource(SourceProductionContext context,
        (Compilation Left, ImmutableArray<MethodDeclarationInfo> Right) values)
        => GenerateSource(context, values.Left, values.Right);

    private protected override void GenerateSource(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<MethodDeclarationInfo> methodDeclarations)
    {
        IEnumerable<IGrouping<string, MethodDeclarationInfo>> methodSyntaxGroups = methodDeclarations
            .GroupBy(gp => gp.ContainingType);
        
        foreach (IGrouping<string, MethodDeclarationInfo>? groupedDeclarations in methodSyntaxGroups)
        {
            string? className = groupedDeclarations.Key;
            string? targetNamespace = groupedDeclarations.Select(declarationInfo => declarationInfo.ContainingNamespace).FirstOrDefault();
            
            IEnumerable<MethodDeclarationSyntax> syntaxList = groupedDeclarations
                .Select(methodDeclaration => methodDeclaration.Syntax);
            
            IEnumerable<MemberDeclarationSyntax> generatedMemberDeclarations = syntaxList.Select(GenerateMemberDeclaration);
            
            // Generate the implementation file name
            string fileName = $"{className}.g.cs";
            
            // Creating the compilation unit syntax factory to generate class code
            CompilationUnitSyntax syntaxFactory = SyntaxFactory.CompilationUnit();
        
            syntaxFactory = syntaxFactory
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("NCalc")))
                .AddMembers(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(targetNamespace))
                    .AddMembers(SyntaxFactory.ClassDeclaration(className)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                        .AddMembers([..generatedMemberDeclarations])));

            // Add the generated code to the compilation
            context.AddSource(fileName, syntaxFactory.NormalizeWhitespace().ToFullString());
        }
    }

    #endregion

    /// <summary>
    /// TODO: DOCUMENT
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private MemberDeclarationSyntax GenerateMemberDeclaration(MethodDeclarationSyntax method)
    {
        // Catching annotated method display string
        string methodName = method.Identifier.Text;
        
        // Reading string expression representing math equation on marker attribute
        string expression = GetMethodExpression(method);
        
        // Storing the annotated method parameters information to generate further validations
        List<ArgumentInfo> parameterInfo = GetMethodParameterInfo(method).ToList();
        
        // Constructing method declarations declarations for each individual annotated method
        // Since all math operations will return a commom type of value, the double return type was choosen
        // to generate the contents
        var memberDeclaration = MethodDeclarationSyntaxExtensions.CreateMethodDeclaration(
            methodName,
            TypeDeclarationSyntaxExtensions.CreateTypeDeclaration("double"),
            [
                SyntaxKind.PublicKeyword,
                SyntaxKind.PartialKeyword
            ],
            parameterInfo.Select(param => ParameterSyntaxExtensions.CreateParameter(param.Name, param.Type)).ToArray());
        
        // To generate the parameters assignment, the NCalc expression approach is used
        // This approach consists in a existing dictionary for a given expression, where each element
        // is a single parameter declared on the string expression content present on marker attribute
        // Since the objective is to inform the NCalc which value has to be assigned on each parameter,
        // a regex is passed on expression to check for the parameters expecting all valid parameters to be enclosed by square brackets like [param_name]
        MatchCollection matches = this.ParameterIdentifierRegx.Matches(expression);
        MatchCollection parametersContainedOnExpression = matches;
        
        IEnumerable<ExpressionStatementSyntax> parameterAssignmentExpressions =
            parametersContainedOnExpression.Count > 0
                ? matches
                    .Cast<Match>()
                    .Select(match => match.Value)
                    .Distinct()
                    .Select(val => CompilationUnitExtensions.CreateExpressionParameterAssignmentSyntax(
                        ExpressionVariable,
                        ReplaceParameterPlaceHolders(val),
                        ReplaceParameterPlaceHolders(val)))
                : [];
        
        // TODO: Raise diagnostics in case the parameters count do not coincide with base annotated method parameters

        memberDeclaration = memberDeclaration
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
        
        return (MemberDeclarationSyntax)memberDeclaration;
    }
}

