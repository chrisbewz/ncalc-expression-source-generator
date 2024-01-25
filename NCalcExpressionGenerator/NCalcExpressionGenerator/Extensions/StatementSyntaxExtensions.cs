using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Extensions;

public static class StatementSyntaxExtensions
{

    /// <summary>
    /// Create a return statement from a given expression syntax
    /// </summary>
    /// <param name="returnExpressionSyntax"></param>
    /// <returns></returns>
    public static ReturnStatementSyntax CreateReturnStatement(ExpressionSyntax? returnExpressionSyntax)
        => SyntaxFactory.ReturnStatement(returnExpressionSyntax);
}