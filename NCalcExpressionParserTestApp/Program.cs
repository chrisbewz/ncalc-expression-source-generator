// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using NCalc;

namespace NCalcExpressionParserTestApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var aClass = new AClass();
            var res = aClass.ComplexExpressionNumbers(2, 4);
            Console.WriteLine(res);
        }
    }
}
