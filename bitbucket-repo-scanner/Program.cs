using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BitbucketScanner
{
    class Program
    {
        private static string logFilePath = "scan_results.log";

        static void Main()
        {
            // Initialize logging
            using (var logger = new Logger(logFilePath))
            {
                try
                {
                    // Specify the Bitbucket repository path
                    string repositoryPath = "C:\\Path\\To\\BitbucketRepository";

                    // Scan the repository
                    ScanRepository(repositoryPath, logger);

                    Console.WriteLine("Scanning complete. Check the log file for results.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred: {ex.Message}");
                    Console.WriteLine("An error occurred. Check the log file for details.");
                }
            }
        }

        static void ScanRepository(string repositoryPath, Logger logger)
        {
            // Get all files recursively in the repository
            var files = Directory.GetFiles(repositoryPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    // Read the contents of the file
                    var lines = File.ReadAllLines(file);

                    // Scan each line for sensitive information
                    for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                    {
                        var line = lines[lineNumber];

                        // Check for usernames
                        if (ContainsUsername(line))
                            logger.LogSensitiveInfo(file, "Username", line, lineNumber + 1);

                        // Check for passwords
                        if (ContainsPassword(line))
                            logger.LogSensitiveInfo(file, "Password", line, lineNumber + 1);

                        // Check for API keys
                        if (ContainsApiKey(line))
                            logger.LogSensitiveInfo(file, "API Key", line, lineNumber + 1);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error processing file '{file}': {ex.Message}");
                }
            }
        }

        static bool ContainsUsername(string line)
        {
            // Add your logic to check for usernames
            // For example, you could use regular expressions or custom validation rules
            return line.Contains("username");
        }

        static bool ContainsPassword(string line)
        {
            // Add your logic to check for passwords
            // For example, you could use regular expressions or custom validation rules
            return line.Contains("password");
        }

        static bool ContainsApiKey(string line)
        {
            // Add your logic to check for API keys
            // For example, you could use regular expressions or custom validation rules
            return line.Contains("api_key");
        }
    }

    class Logger : IDisposable
    {
        private StreamWriter writer;

        public Logger(string logFilePath)
        {
            writer = new StreamWriter(logFilePath, append: true);
        }

        public void LogSensitiveInfo(string fileName, string sensitiveType, string line, int lineNumber)
        {
            string logMessage = $"Sensitive information found in file '{fileName}', {sensitiveType}: '{line}', Line: {lineNumber}";
            writer.WriteLine(logMessage);
            Console.WriteLine(logMessage);
        }

        public void LogError(string errorMessage)
        {
            string logMessage = $"Error: {errorMessage}";
            writer.WriteLine(logMessage);
            Console.WriteLine(logMessage);
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
