using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class GetProperty
    {
        public static object GetColumnValue(object obj, string columnName)
        {
            // Get all properties and find the one that matches the column name, case-insensitive
            PropertyInfo propertyInfo = obj.GetType()
                .GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            // Check if property exists
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj); // Get the value of the property
            }

            return null; // Return null if the property doesn't exist
        }

        public static decimal EvaluateExpression(string expression)
        {
            // Use the Compute method to evaluate the expression and return the result as a double
            var dataTable = new System.Data.DataTable();
            var value = dataTable.Compute(expression, string.Empty);

            return Convert.ToDecimal(value);
        }
    }
}
