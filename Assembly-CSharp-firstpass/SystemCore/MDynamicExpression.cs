using System;
using System.Linq.Expressions;
using System.Linq.Dynamic;

namespace Offworld.SystemCore
{
    public static class MDynamicExpression
    {
        //http://stackoverflow.com/questions/821365/how-to-convert-a-string-to-its-equivalent-expression-tree
        //example usage:
        //Func<int, int> function = DynamicExpression<int, int>("x*(x+1)", "x", out error);
        //int y = function(3); //y=12
        public static Func<T, TResult> ParseFunction<T, TResult>(string expression, string variableName, out string error)
        {
            error = null;
            try
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), variableName);
                Expression<Func<T, TResult>> parsedExpression = (Expression<Func<T, TResult>>)System.Linq.Dynamic.DynamicExpression.ParseLambda(new ParameterExpression [] {parameter}, typeof(TResult), expression);
                Func<T, TResult> dynamicFunction = parsedExpression.Compile();
                return dynamicFunction;
            }
            catch(System.Exception e)
            {
                error = e.Message;
                return null;
            }
        }

        public static Predicate<T> ParsePredicate<T>(string expression, string variableName, out string error)
        {
            Func<T, bool> function = ParseFunction<T, bool>(expression, variableName, out error);
            if(function == null)
                return null;

            Predicate<T> predicate = x => function(x);
            return predicate;
        }
    }
}