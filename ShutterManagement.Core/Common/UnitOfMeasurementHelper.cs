using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class UnitOfMeasurementHelper
    {
        /// <summary>
        /// Enumeration of supported unit types for better type safety
        /// </summary>
        public enum UnitType
        {
            Each,
            Weight,
            Area,
            Linear,
            Unknown
        }

        private static readonly Dictionary<string, UnitType> UnitTypeMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Each (Ea)", UnitType.Each },
            { "Lbs", UnitType.Weight },
            { "Sq Feet", UnitType.Area },
            { "Sq Inches", UnitType.Area },
            { "Linear Inches", UnitType.Linear },
            { "Linear Feet", UnitType.Linear }
        };

        /// <summary>
        /// Calculates length and width dimensions based on unit of measurement
        /// </summary>
        /// <param name="unitOfMeasurement">The unit of measurement string</param>
        /// <param name="inputLength">Input length value</param>
        /// <param name="inputWidth">Input width value</param>
        /// <returns>Tuple containing calculated length and width</returns>
        public static (decimal? length, decimal? width) CalculateDimensions(
            string unitOfMeasurement, 
            decimal? inputLength, 
            decimal? inputWidth)
        {
            if (string.IsNullOrWhiteSpace(unitOfMeasurement))
                return (null, null);

            var unitType = GetUnitType(unitOfMeasurement);

            return unitType switch
            {
                UnitType.Each or UnitType.Weight => (null, null),
                UnitType.Area => (inputLength, inputWidth),
                UnitType.Linear => (inputLength, null),
                UnitType.Unknown => (null, null),
                _ => (null, null)
            };
        }

        /// <summary>
        /// Gets the unit type for a given unit of measurement string
        /// </summary>
        /// <param name="unitOfMeasurement">The unit of measurement string</param>
        /// <returns>The corresponding UnitType</returns>
        public static UnitType GetUnitType(string unitOfMeasurement)
        {
            if (string.IsNullOrWhiteSpace(unitOfMeasurement))
                return UnitType.Unknown;

            return UnitTypeMapping.TryGetValue(unitOfMeasurement.Trim(), out var unitType) 
                ? unitType 
                : UnitType.Unknown;
        }

        /// <summary>
        /// Checks if the unit of measurement requires length dimension
        /// </summary>
        /// <param name="unitOfMeasurement">The unit of measurement string</param>
        /// <returns>True if length is required, false otherwise</returns>
        public static bool RequiresLength(string unitOfMeasurement)
        {
            var unitType = GetUnitType(unitOfMeasurement);
            return unitType == UnitType.Area || unitType == UnitType.Linear;
        }

        /// <summary>
        /// Checks if the unit of measurement requires width dimension
        /// </summary>
        /// <param name="unitOfMeasurement">The unit of measurement string</param>
        /// <returns>True if width is required, false otherwise</returns>
        public static bool RequiresWidth(string unitOfMeasurement)
        {
            var unitType = GetUnitType(unitOfMeasurement);
            return unitType == UnitType.Area;
        }

        /// <summary>
        /// Gets all supported unit of measurement strings
        /// </summary>
        /// <returns>Collection of supported unit strings</returns>
        public static IEnumerable<string> GetSupportedUnits()
        {
            return UnitTypeMapping.Keys;
        }

        /// <summary>
        /// Validates if a unit of measurement is supported
        /// </summary>
        /// <param name="unitOfMeasurement">The unit of measurement to validate</param>
        /// <returns>True if supported, false otherwise</returns>
        public static bool IsValidUnit(string unitOfMeasurement)
        {
            return GetUnitType(unitOfMeasurement) != UnitType.Unknown;
        }
    }
} 