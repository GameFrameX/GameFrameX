using System.Linq.Expressions;

namespace Server.Extension
{
    /// <summary>
    /// 提供对 <see cref="Expression"/> 类型的扩展方法。
    /// </summary>
    public static class ExpressionExtension
    {
        /// <summary>
        /// 将两个表达式进行逻辑与运算。
        /// </summary>
        /// <typeparam name="T">表达式的参数类型。</typeparam>
        /// <param name="expr1">第一个表达式。</param>
        /// <param name="expr2">第二个表达式。</param>
        /// <returns>逻辑与运算后的表达式。</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null)
            {
                throw new ArgumentNullException(nameof(expr1) + " null不能处理");
            }

            if (expr2 == null)
            {
                throw new ArgumentNullException(nameof(expr2) + " null不能处理");
            }

            var newParameter = Expression.Parameter(typeof(T), "x");
            var visitor = new ExpressionVisitorCustom(newParameter);
            var left = visitor.Visit(expr1.Body);
            var right = visitor.Visit(expr2.Body);
            var body = Expression.And(left, right);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 将两个表达式进行逻辑或运算。
        /// </summary>
        /// <typeparam name="T">表达式的参数类型。</typeparam>
        /// <param name="expr1">第一个表达式。</param>
        /// <param name="expr2">第二个表达式。</param>
        /// <returns>逻辑或运算后的表达式。</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null)
            {
                throw new ArgumentNullException(nameof(expr1) + " null不能处理");
            }

            if (expr2 == null)
            {
                throw new ArgumentNullException(nameof(expr2) + " null不能处理");
            }

            var newParameter = Expression.Parameter(typeof(T), "x");
            var visitor = new ExpressionVisitorCustom(newParameter);
            var left = visitor.Visit(expr1.Body);
            var right = visitor.Visit(expr2.Body);
            var body = Expression.Or(left, right);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 对表达式进行逻辑非运算。
        /// </summary>
        /// <typeparam name="T">表达式的参数类型。</typeparam>
        /// <param name="expr">要进行逻辑非运算的表达式。</param>
        /// <returns>逻辑非运算后的表达式。</returns>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException(nameof(expr) + " null不能处理");
            }

            var newParameter = expr.Parameters[0];
            var body = Expression.Not(expr.Body);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }
    }
}