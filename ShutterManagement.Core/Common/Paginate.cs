using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace ShutterManagement.Core.Common
{
    public static class Paginate
    {
        public static List<T> Pagination<T>(IEnumerable<T> filteredRecords, Pagination filter)
        {
            return filteredRecords
                .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                .Take(filter.RecordPerPage)
                .ToList();
        }


        // Apply filters dynamically by field and value
        public static IQueryable<T> ApplyColumnFilters<T>(IQueryable<T> source, List<ColumnFilter> columnFilters)
        {
            if (columnFilters == null || !columnFilters.Any())
                return source;

            foreach (var col in columnFilters)
            {
                if (string.IsNullOrWhiteSpace(col.Field) || string.IsNullOrWhiteSpace(col.Value))
                    continue;

                var entityType = typeof(T);
                var property = entityType.GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, col.Field, StringComparison.OrdinalIgnoreCase));
                if (property == null) continue;

                var val = col.Value.ToLower().Trim();
                var parameter = Expression.Parameter(typeof(T), "r");
                var propertyExpression = Expression.Property(parameter, property.Name);
                var propertyType = property.PropertyType;

                Expression predicateBody = null;

                if (propertyType == typeof(string))
                {
                    var toLower = Expression.Call(propertyExpression, "ToLower", null);
                    predicateBody = Expression.Equal(toLower, Expression.Constant(val));
                }
                else if (propertyType == typeof(Guid))
                {
                    if (Guid.TryParse(val, out var guidValue))
                        predicateBody = Expression.Equal(propertyExpression, Expression.Constant(guidValue));
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (DateTime.TryParse(val, out var dateTimeValue))
                        predicateBody = Expression.Equal(propertyExpression, Expression.Constant(dateTimeValue));
                }
                else if (propertyType == typeof(bool))
                {
                    if (bool.TryParse(val, out var boolValue))
                        predicateBody = Expression.Equal(propertyExpression, Expression.Constant(boolValue));
                }

                if (predicateBody != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(predicateBody, parameter);
                    source = source.Where(lambda);
                }
            }

            return source;
        }

        // Apply dynamic sorting
        public static IQueryable<T> ApplySorting<T>(IQueryable<T> source, string sortColumn, string sortDirection)
        {
            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                var direction = string.IsNullOrEmpty(sortDirection) || sortDirection.ToLower() == "asc" ? "asc" : "desc";
                var sortExpression = $"{sortColumn} {direction}";
                return source.OrderBy(sortExpression);
            }

            return source;
        }

    }
}

