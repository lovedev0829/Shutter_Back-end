using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                string serializedObjects = JsonConvert.SerializeObject(objects, Formatting.Indented);
                logMessage += $"\nData: {serializedObjects}";
            }

            try
            {
                using (var fileStream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        writer.WriteLine(logMessage + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            // Append the log message to the file
            //File.AppendAllText(logFilePath, );

            // Optional: Write to console as well
            // Console.WriteLine(logMessage);
        }
    }
}
