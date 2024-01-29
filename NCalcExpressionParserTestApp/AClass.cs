namespace NCalcExpressionParserTestApp
{
    public partial class AClass
    {
        [MethodExpression("2* ([a]+[b])")]
        public partial double SumNumbers(int a, int b);
        //
        [MethodExpression("(([a]+[b])+((2*[a])+[b]))/(2*[b])")]
        public partial double ComplexExpressionNumbers(int a, int b);
        //
        [MethodExpression("[a]*[b]")]
        public partial double MultiplyNumbers(int a, int b);

        [MethodExpression("[a]*[b]+1")]
        public partial double MultiplyNumbers2(int a, int b);
        
        [MethodExpression("[a]*[b]+2")]
        public partial double MultiplyNumbers3(int a, int b);
    }
}