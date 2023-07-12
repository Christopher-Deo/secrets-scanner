using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


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
            // Check if the line contains the word "username" (case-insensitive)
            if (line.IndexOf("username", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Add any additional logic to identify plain text usernames
            // Return true if a username is found, false otherwise
            return false;
        }

        static bool ContainsPassword(string line)
        {
            // Check if the line contains the word "password" (case-insensitive)
            if (line.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Add any additional logic to identify plain text passwords
            // Return true if a password is found, false otherwise
            return false;
        }

        static bool ContainsApiKey(string line)
        {
            // Use a regular expression to check for standard API key format
            string apiKeyPattern = @"^[A-Za-z0-9-_]{16,}$";
            return Regex.IsMatch(line, apiKeyPattern);
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
