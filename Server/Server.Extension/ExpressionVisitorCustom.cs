using System.Linq.Expressions;

namespace Server.Extension
{
    /// <summary>
    /// 表达式访问器的自定义实现。
    /// </summary>
    public class ExpressionVisitorCustom : ExpressionVisitor
    {
        /// <summary>
        /// 获取或设置访问器中的参数表达式。
        /// </summary>
        public ParameterExpression Parameter { get; }

        /// <summary>
        /// 初始化 <see cref="ExpressionVisitorCustom"/> 类的新实例。
        /// </summary>
        /// <param name="param">访问器中的参数表达式。</param>
        public ExpressionVisitorCustom(ParameterExpression param)
        {
            this.Parameter = param;
        }

        /// <summary>
        /// 访问参数表达式。
        /// </summary>
        /// <param name="node">要访问的参数表达式。</param>
        /// <returns>返回访问后的表达式。</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return this.Parameter;
        }
    }
}