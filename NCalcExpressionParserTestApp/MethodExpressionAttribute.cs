namespace NCalcExpressionParserTestApp
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = false)]
    public class MethodExpressionAttribute : Attribute
    {
        public string Expression { get;  }
    
        public MethodExpressionAttribute(string expression)
        {
            this.Expression = expression;
        }
    }
}