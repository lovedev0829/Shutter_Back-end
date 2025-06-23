using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class RoleNameHelper
    {
        private static readonly Dictionary<string, string> RoleReplacements = new()
        {
            { "Edit", "" },
            { "View", "" },
            { "Delete", "" },
            { "Add", "" },
            { "Address", "Address" }, // Fix for the typo issue
            { "SubTenant", "Partners" }
        };

        /// <summary>
        /// Cleans and standardizes role permission names by removing unnecessary suffixes
        /// and applying standard replacements
        /// </summary>
        /// <param name="permissionName">The raw permission name to clean</param>
        /// <returns>Cleaned role name</returns>
        public static string CleanRoleName(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                return string.Empty;

            var cleanedName = permissionName.Trim();

            // Apply all replacements in order
            foreach (var replacement in RoleReplacements)
            {
                cleanedName = cleanedName.Replace(replacement.Key, replacement.Value);
            }

            // Remove any double spaces and trim
            while (cleanedName.Contains("  "))
            {
                cleanedName = cleanedName.Replace("  ", " ");
            }

            return cleanedName.Trim();
        }

        /// <summary>
        /// Gets a list of unique cleaned role names from permission names
        /// </summary>
        /// <param name="permissionNames">Collection of permission names</param>
        /// <returns>Distinct cleaned role names</returns>
        public static IEnumerable<string> GetUniqueCleanedRoleNames(IEnumerable<string> permissionNames)
        {
            return permissionNames
                .Select(CleanRoleName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name);
        }
    }
} 