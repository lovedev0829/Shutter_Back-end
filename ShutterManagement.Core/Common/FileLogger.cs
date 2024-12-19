using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public class FileLogger
    {
        private static readonly string logFilePath = "Files/logs.txt";

        public static void Log(string message, params object?[] objects)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} : {message}";
            if (objects != null && objects.Length > 0)
            {
                logMessage += objects.ToArray();
            }

            // Append the log message to the file
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);

            // Optional: Write to console as well
           // Console.WriteLine(logMessage);
        }
    }
}
