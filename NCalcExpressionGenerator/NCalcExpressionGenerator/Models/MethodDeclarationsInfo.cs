using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Models;

public record MethodDeclarationInfo(
    string ContainingType,
    string ContainingNamespace,
    MethodDeclarationSyntax Syntax
);