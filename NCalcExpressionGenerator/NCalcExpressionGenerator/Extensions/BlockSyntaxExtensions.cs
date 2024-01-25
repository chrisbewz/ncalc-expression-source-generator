using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Extensions;

public class BlockSyntaxExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="statements"></param>
    /// <returns></returns>
    public static BlockSyntax CreateBlock(params StatementSyntax[] statements)
        => SyntaxFactory.Block(statements);
}