using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels;

namespace ShutterManagement.Core.Common
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Handles exceptions and returns a standardized error response
        /// </summary>
        /// <typeparam name="T">The response data type</typeparam>
        /// <param name="ex">The exception to handle</param>
        /// <param name="context">Context information about where the exception occurred</param>
        /// <param name="logAction">Optional custom logging action</param>
        /// <returns>ResponseVm with appropriate error message</returns>
        public static IResponseVm<T> HandleException<T>(
            Exception ex, 
            string context = "", 
            Action<string>? logAction = null)
        {
            var responseVm = new ResponseVm<T>
            {
                IsSuccess = false,
                Data = default(T)
            };

            var logMessage = $"Error in {context}: {ex.Message}";
            
            // Log the exception
            logAction?.Invoke(logMessage);
            FileLogger.Log(logMessage);

            // Set appropriate error message based on exception type
            responseVm.Message = ex switch
            {
                ArgumentNullException => "Required parameter is missing.",
                ArgumentException => "Invalid parameter provided.",
                UnauthorizedAccessException => AppConstant.AccessDenied,
                InvalidOperationException => "The requested operation is not valid in the current state.",
                TimeoutException => "The operation timed out. Please try again.",
                System.Net.Http.HttpRequestException => "Network error occurred. Please check your connection.",
                FormatException => "Invalid data format provided.",
                OverflowException => "Numeric value is too large or too small.",
                System.IO.FileNotFoundException => "Required file not found.",
                Microsoft.Data.SqlClient.SqlException sqlEx => HandleSqlException(sqlEx),
                _ => AppConstant.ExceptionError + " Please contact support if the issue persists."
            };

            return responseVm;
        }

        /// <summary>
        /// Handles SQL-specific exceptions
        /// </summary>
        /// <param name="sqlEx">SQL exception to handle</param>
        /// <returns>User-friendly error message</returns>
        private static string HandleSqlException(Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                2 => "Database connection timeout. Please try again.",
                18 => "Database connection failed. Please check your connection.",
                2146232060 => "Database constraint violation. Please check your data.",
                547 => "Cannot delete this record as it's referenced by other data.",
                2627 or 2601 => "Duplicate data detected. Please ensure unique values.",
                _ => "Database error occurred. Please contact support."
            };
        }

        /// <summary>
        /// Creates a success response
        /// </summary>
        /// <typeparam name="T">The response data type</typeparam>
        /// <param name="data">Response data</param>
        /// <param name="message">Success message</param>
        /// <param name="totalRecords">Total record count for pagination</param>
        /// <returns>Success ResponseVm</returns>
        public static IResponseVm<T> CreateSuccessResponse<T>(
            T data, 
            string message = "", 
            int totalRecords = 0)
        {
            return new ResponseVm<T>
            {
                IsSuccess = true,
                Data = data,
                Message = string.IsNullOrWhiteSpace(message) ? AppConstant.SuccessMsg : message,
                TotalRecords = totalRecords
            };
        }

        /// <summary>
        /// Creates a failure response
        /// </summary>
        /// <typeparam name="T">The response data type</typeparam>
        /// <param name="message">Error message</param>
        /// <param name="data">Optional response data</param>
        /// <returns>Failure ResponseVm</returns>
        public static IResponseVm<T> CreateFailureResponse<T>(string message, T data = default(T))
        {
            return new ResponseVm<T>
            {
                IsSuccess = false,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Validates that required parameters are not null or empty
        /// </summary>
        /// <param name="parameters">Dictionary of parameter names and values</param>
        /// <throws>ArgumentException if any parameter is null or empty</throws>
        public static void ValidateRequiredParameters(Dictionary<string, object?> parameters)
        {
            foreach (var param in parameters)
            {
                if (param.Value == null || 
                    (param.Value is string str && string.IsNullOrWhiteSpace(str)) ||
                    (param.Value is Guid guid && guid == Guid.Empty))
                {
                    throw new ArgumentException($"Parameter '{param.Key}' is required and cannot be null or empty.");
                }
            }
        }
    }
} 