using System;
using System.IO;
using System.Text;

namespace IHECLibrary.Helpers
{
    public static class DebugHelper
    {
        private static readonly object LogLock = new object();
        private static string LogFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");

        public static void LogMessage(string message)
        {
            try
            {
                Console.WriteLine(message);
                
                lock (LogLock)
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logEntry = $"[{timestamp}] {message}\n";
                    
                    File.AppendAllText(LogFilePath, logEntry);
                }
            }
            catch
            {
                // Ignore any errors in logging
            }
        }
        
        public static void LogError(string context, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ERROR in {context}: {ex.Message}");
            sb.AppendLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner exception: {ex.InnerException.Message}");
                sb.AppendLine($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
            
            LogMessage(sb.ToString());
        }
        
        public static void ClearLog()
        {
            try
            {
                lock (LogLock)
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                }
            }
            catch
            {
                // Ignore any errors in clearing log
            }
        }
        
        // Additional helper methods used in other parts of the code
        public static void LogDebugInfo(string message)
        {
            LogMessage($"DEBUG: {message}");
        }
        
        public static void LogException(Exception ex, string context)
        {
            LogError(context, ex);
        }
        
        public static void LogViewCreationError(Exception ex, string viewName)
        {
            LogError($"View creation error in {viewName}", ex);
        }
    }
} 