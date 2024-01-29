using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Extensions;

public static class MethodDeclarationSyntaxExtensions
{
    /// <summary>
    /// Create a <see cref="MethodDeclarationSyntax"/> from a given name and return type and parameters
    /// </summary>
    /// <param name="name"></param>
    /// <param name="returnTypeSyntax"></param>
    /// <param name="modifiers"></param>
    /// <param name="parametersSyntax"></param>
    /// <returns></returns>
    public static MethodDeclarationSyntax CreateMethodDeclaration(string name, TypeSyntax returnTypeSyntax, SyntaxKind[]? modifiers,
        params ParameterSyntax[]? parametersSyntax)
    {

        MethodDeclarationSyntax methodDeclarationSyntax = SyntaxFactory.MethodDeclaration(returnTypeSyntax, name);
        if (modifiers is not null)
            methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers(modifiers);

        if (parametersSyntax is not null)
            methodDeclarationSyntax = methodDeclarationSyntax.WithParameterList(SyntaxFactory.ParameterList(
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                SyntaxFactory.SeparatedList<ParameterSyntax>(parametersSyntax),
                SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                )
            );

        return methodDeclarationSyntax;

    }

    /// <summary>
    /// Adds a list of parameters to a method declaration
    /// </summary>
    /// <param name="methodSyntax">Target method declaration to add parameter.</param>
    /// <param name="parameters">Parameters to add on method declaration.</param>
    /// <returns></returns>
    public static MethodDeclarationSyntax AddParameters(this MethodDeclarationSyntax methodSyntax,
        params ParameterSyntax[] parameters)
        => methodSyntax.WithParameterList(
            SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList<ParameterSyntax>(parameters)
            )
        );

    /// <summary>
    /// Adds a single parameter to a method declaration
    /// </summary>
    /// <param name="methodSyntax">Target method declaration to add parameter.</param>
    /// <param name="parameter">Parameter to add on method declaration.</param>
    /// <returns></returns>
    public static MethodDeclarationSyntax AddParameter(this MethodDeclarationSyntax methodSyntax,
        ParameterSyntax parameter)
        => methodSyntax.WithParameterList(
            SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList(parameter)
            )
        );
    
    

    /// <summary>
    /// Add a list of <see cref="SyntaxKind"/> sequentially to passed method declaration
    /// </summary>
    /// <param name="methodSyntax">Method declaration to add modifier into.</param>
    /// <param name="modifiers">Modifiers to add on method.</param>
    /// <returns></returns>
    public static MethodDeclarationSyntax AddModifiers(this MethodDeclarationSyntax methodSyntax,
        params SyntaxKind[] modifiers)
    {
        List<SyntaxToken> tokenList = modifiers.AsEnumerable().Select(mod => SyntaxFactory.Token(mod)).ToList();
        tokenList.ForEach(token => methodSyntax = methodSyntax.AddModifiers(token));
        return methodSyntax;
    }

    /// <summary>
    /// Add a single <see cref="SyntaxKind"/> modifier to passed method declaration
    /// </summary>
    /// <param name="methodSyntax">Method declaration to add modifier into.</param>
    /// <param name="modifier">Modifier to add on method.</param>
    /// <returns></returns>
    public static MethodDeclarationSyntax AddModifier(this MethodDeclarationSyntax methodSyntax,
        SyntaxKind modifier)
        => methodSyntax.AddModifiers(SyntaxFactory.Token(modifier));

        
}