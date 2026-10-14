using System.Linq.Expressions;

namespace TestService.Application.Common;

/// <summary>
/// 用于在运行时拼接多个 Lambda 谓词。
/// 解决 EF Core 多条件动态拼装时无法直接 &amp;&amp; 的问题。
/// </summary>
public static class PredicateExtensions
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
        var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody!, rightBody!), parameter);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        public ParameterReplacer(ParameterExpression parameter) => _parameter = parameter;
        protected override Expression VisitParameter(ParameterExpression node) => _parameter;
    }
}
