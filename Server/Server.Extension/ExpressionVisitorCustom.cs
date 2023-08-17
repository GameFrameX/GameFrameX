using System.Linq.Expressions;

namespace Server.Extension;

public class ExpressionVisitorCustom : ExpressionVisitor
{
    public ParameterExpression Parameter { get; }

    public ExpressionVisitorCustom(ParameterExpression param)
    {
        this.Parameter = param;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return this.Parameter;
    }
}