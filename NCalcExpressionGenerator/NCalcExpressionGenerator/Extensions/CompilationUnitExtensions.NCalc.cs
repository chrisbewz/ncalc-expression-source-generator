using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCalcExpressionGenerator.Extensions;

public static class CompilationUnitExtensions
{
    /// <summary>
    /// Generated a custom named expression statement syntax like :
    /// <para></para> Expression [EXPRESSION_VARIABLE] = new Expression([EXPRESSION]);
    /// </summary>
    /// <param name="expressionVariable"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static LocalDeclarationStatementSyntax CreateExpressionDeclarationSyntax(string expressionVariable,
        string expression)
        => SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("Expression"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(expressionVariable))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName("Expression"))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.ParseExpression(expression))))))))))
            .NormalizeWhitespace();

    /// <summary>
    /// Generates a <see cref="NCalc.Expression"/> assigment value given a target parameter like:
    /// <para></para> [EXPRESSION].Parameters[PARAM_NAME] = [VALUE];
    /// </summary>
    /// <param name="parameterName"></param>
    /// <param name="value"></param>
    /// <param name="expressionVariable"></param>
    /// <returns></returns>
    public static ExpressionStatementSyntax CreateExpressionParameterAssignmentSyntax(string expressionVariable,
        string parameterName, string value)
        => SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.ElementAccessExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName($"{expressionVariable}"),
                            SyntaxFactory.IdentifierName("Parameters")))
                    .WithArgumentList(
                        SyntaxFactory.BracketedArgumentList(
                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal($"{parameterName}")))))),
                SyntaxFactory.IdentifierName($"{value}")))
            .NormalizeWhitespace();

    /// <summary>
    /// Generates a <see cref="NCalc.Expression"/> assigment value given a target parameter like:
    /// <para></para> [EXPRESSION].Evaluate();
    /// </summary>
    /// <param name="expressionVariable"></param>
    /// <returns></returns>
    public static ExpressionStatementSyntax CreateExpressionInvocationSyntax(string expressionVariable)
        => SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(expressionVariable),
                        SyntaxFactory.IdentifierName("Evaluate"))))
            .WithSemicolonToken(
                SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken))
            .NormalizeWhitespace();

    /// <summary>
    /// Creates a <see cref="NCalc.Expression"/> evaluation invocation expression syntax like:
    /// <para></para> [EXPRESSION].Evaluate();
    /// </summary>
    /// <param name="expressionVariable"></param>
    /// <returns></returns>
    public static ExpressionSyntax CreateEvaluationExpressionSyntax(string expressionVariable, string evaluationVariable = "result")
        => SyntaxFactory.ConditionalExpression(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(evaluationVariable),
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(expressionVariable),
                            SyntaxFactory.IdentifierName("Evaluate")))),
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.MissingToken(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.IdentifierToken,
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.Trivia(
                                SyntaxFactory.SkippedTokensTrivia())))))
            .WithColonToken(
                SyntaxFactory.MissingToken(SyntaxKind.ColonToken))
            .NormalizeWhitespace();
}