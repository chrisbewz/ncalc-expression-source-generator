using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Extensions;

public static class TypeDeclarationSyntaxExtensions
{
    /// <summary>
    /// Generates a TypeSyntax statement from a given fully declared type name
    /// </summary>
    /// <param name="fullyQualifiedTypeName"></param>
    /// <returns></returns>
    public static TypeSyntax CreateTypeDeclaration(string fullyQualifiedTypeName)
        => SyntaxFactory.ParseTypeName(fullyQualifiedTypeName);
}