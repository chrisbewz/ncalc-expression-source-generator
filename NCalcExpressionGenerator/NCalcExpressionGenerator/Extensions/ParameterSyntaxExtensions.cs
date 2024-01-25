using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Extensions;

public static class ParameterSyntaxExtensions
{
    /// <summary>
    /// Create a <see cref="ParameterSyntax"/> from a given name and type syntax 
    /// </summary>
    /// <param name="parameterName">Name to be assigned on parameter created.</param>
    /// <param name="parameterType">Type syntax to be assigned on created parameter.</param>
    /// <returns></returns>
    public static ParameterSyntax CreateParameter(string parameterName, TypeSyntax parameterType)
        => SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
            .WithType(parameterType);
    
    /// <summary>
    /// Create a <see cref="ParameterSyntax"/> from a given name and fully qualified type name string
    /// </summary>
    /// <param name="parameterName">Name to be assigned on parameter created.</param>
    /// <param name="parameterTypeName">Fully qualified type name to generate type syntax from.</param>
    /// <returns></returns>
    public static ParameterSyntax CreateParameter(string parameterName, string parameterTypeName)
        => SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
            .WithType(TypeDeclarationSyntaxExtensions.CreateTypeDeclaration(parameterTypeName));
}