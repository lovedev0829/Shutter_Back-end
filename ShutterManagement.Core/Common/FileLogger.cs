using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ShutterManagement.Core.Common
{
    public class FileLogger
    {
        private static readonly string logFilePath = "Files/logs.txt";

        public static void Log(string message, params object?[] objects)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Prepare log message
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} : {message}";
                if (objects != null && objects.Length > 0)
                {
                    string serializedObjects = JsonConvert.SerializeObject(objects, Formatting.Indented);
                    logMessage += $"\nData: {serializedObjects}";
                }

                // Append log entry safely
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Handle logging failure (could log to console or another file)
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}
