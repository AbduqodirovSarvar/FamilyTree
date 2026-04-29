using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Extentions
{
    /// <summary>
    /// Builds EF-translatable filter / search predicates from request DTO state.
    ///
    /// Supports two filter key shapes so the API stays backward compatible with
    /// the original "Property.operation" form while also accepting the bare
    /// "Property" form the admin SPA actually sends (e.g. <c>Filters[FamilyId]</c>):
    /// <list type="bullet">
    ///   <item><description><c>"Status.equals"</c> → explicit operator</description></item>
    ///   <item><description><c>"FamilyId"</c> → defaults to <c>equals</c> (with type
    ///     conversion for Guid/Enum so frontend strings bind cleanly)</description></item>
    /// </list>
    /// </summary>
    public static class FilterExpressionBuilder
    {
        public static Expression<Func<T, bool>>? BuildPredicate<T>(this Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0) return null;

            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            foreach (var filter in filters)
            {
                if (string.IsNullOrEmpty(filter.Value)) continue;

                string propertyName;
                string operation;

                int dotIdx = filter.Key.IndexOf('.');
                if (dotIdx >= 0)
                {
                    propertyName = filter.Key[..dotIdx];
                    operation = filter.Key[(dotIdx + 1)..].ToLowerInvariant();
                }
                else
                {
                    propertyName = filter.Key;
                    operation = "equals";
                }

                PropertyInfo? property = typeof(T).GetProperty(
                    propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null) continue;

                Expression left = Expression.Property(param, property);
                Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                object? typedValue;
                try { typedValue = ConvertValue(filter.Value, targetType); }
                catch { continue; }

                // Constant must be created at the underlying type, then converted to
                // the property type — so a non-nullable Guid binds cleanly into a
                // Guid? property without "argument types do not match" errors.
                Expression right = property.PropertyType == targetType
                    ? Expression.Constant(typedValue, property.PropertyType)
                    : Expression.Convert(Expression.Constant(typedValue, targetType), property.PropertyType);

                Expression? condition = operation switch
                {
                    "equals" => Expression.Equal(left, right),
                    "notequals" => Expression.NotEqual(left, right),
                    "greaterthan" or "morethan" => Expression.GreaterThan(left, right),
                    "lessthan" => Expression.LessThan(left, right),
                    "contains" when property.PropertyType == typeof(string)
                        => BuildStringContains(left, filter.Value),
                    "startswith" when property.PropertyType == typeof(string)
                        => Expression.Call(left, _stringStartsWith, Expression.Constant(filter.Value)),
                    "endswith" when property.PropertyType == typeof(string)
                        => Expression.Call(left, _stringEndsWith, Expression.Constant(filter.Value)),
                    _ => null
                };

                if (condition != null)
                    combined = combined == null ? condition : Expression.AndAlso(combined, condition);
            }

            if (combined == null) return null;
            return Expression.Lambda<Func<T, bool>>(combined, param);
        }

        /// <summary>
        /// Build a case-insensitive OR-search predicate across the given string
        /// fields. Returns null when there is nothing to search for.
        /// </summary>
        public static Expression<Func<T, bool>>? BuildSearchPredicate<T>(string? searchText, params string[] fields)
        {
            if (string.IsNullOrWhiteSpace(searchText) || fields.Length == 0) return null;

            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            string lowered = searchText.Trim().ToLowerInvariant();
            ConstantExpression loweredConst = Expression.Constant(lowered);

            foreach (var fieldName in fields)
            {
                PropertyInfo? property = typeof(T).GetProperty(
                    fieldName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null || property.PropertyType != typeof(string)) continue;

                Expression prop = Expression.Property(param, property);

                // x.Field != null && x.Field.ToLower().Contains(lowered)
                // EF translates ToLower → SQL LOWER(...) which copes with NULL safely.
                Expression notNull = Expression.NotEqual(prop, Expression.Constant(null, typeof(string)));
                Expression containsCall = Expression.Call(
                    Expression.Call(prop, _stringToLower),
                    _stringContains,
                    loweredConst);
                Expression condition = Expression.AndAlso(notNull, containsCall);

                combined = combined == null ? condition : Expression.OrElse(combined, condition);
            }

            if (combined == null) return null;
            return Expression.Lambda<Func<T, bool>>(combined, param);
        }

        /// <summary>
        /// Combine two predicates with AND. Either side may be null — the other
        /// is returned as-is. When both are null, returns null.
        /// </summary>
        public static Expression<Func<T, bool>>? AndAlso<T>(
            this Expression<Func<T, bool>>? left,
            Expression<Func<T, bool>>? right)
        {
            if (left == null) return right;
            if (right == null) return left;

            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression leftBody = new ParameterReplacer(left.Parameters[0], param).Visit(left.Body)!;
            Expression rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body)!;
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody, rightBody), param);
        }

        private static object? ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string)) return value;
            if (targetType == typeof(Guid)) return Guid.Parse(value);
            if (targetType.IsEnum) return Enum.Parse(targetType, value, ignoreCase: true);
            if (targetType == typeof(DateTime)) return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType == typeof(DateOnly)) return DateOnly.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType == typeof(bool)) return bool.Parse(value);
            return Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static Expression BuildStringContains(Expression left, string value)
        {
            // Case-insensitive contains via LOWER(field).Contains(lowered).
            return Expression.AndAlso(
                Expression.NotEqual(left, Expression.Constant(null, typeof(string))),
                Expression.Call(
                    Expression.Call(left, _stringToLower),
                    _stringContains,
                    Expression.Constant(value.ToLowerInvariant())));
        }

        private static readonly MethodInfo _stringContains =
            typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        private static readonly MethodInfo _stringStartsWith =
            typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
        private static readonly MethodInfo _stringEndsWith =
            typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;
        private static readonly MethodInfo _stringToLower =
            typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

        private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) =>
                node == from ? to : base.VisitParameter(node);
        }
    }
}
