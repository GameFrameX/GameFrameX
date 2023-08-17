using System.Linq.Expressions;

namespace Server.Extension;

/// <summary>
/// 合并表达式 And Or Not扩展方法
/// </summary>
public static class ExpressionExtension
{
    /// <summary>
    /// 合并表达式 expr1 AND expr2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expr1"></param>
    /// <param name="expr2"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
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
    /// 合并表达式 expr1 or expr2
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expr1"></param>
    /// <param name="expr2"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
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
    /// 表达式取非
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expr"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
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