using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace BitbucketScanner
{
    class Program
    {
        private static string logFilePath = "scan_results.log";
        private static string excelFilePath = "C:/Users/deoc/Coding-Projects/PD-bitbucket-migrations/scan_results.xlsx";
        private static List<string> applicableExtensions = new List<string> { ".cs", ".csproj", ".sql", ".config". ".xml", ".project" };

        static void Main()
        {
            // Initialize logging
            using (var logger = new Logger(logFilePath))
            {
                try
                {
                    // Specify the directory to be scanned
                    string directoryPath = @"C:\Users\deoc\Coding-Projects\PD-bitbucket-migration\formfox";

                    // Scan the directory for exposed secrets
                    var scanResults = ScanDirectory(directoryPath, logger);

                    // Export scan results to an Excel file
                    ExportToExcel(scanResults, excelFilePath);

                    Console.WriteLine($"Scanning complete. Scan results exported to: {excelFilePath}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred: {ex.Message}");
                    Console.WriteLine("An error occurred. Check the log file for details.");
                }
            }
        }

        static List<ScanResult> ScanDirectory(string directoryPath, Logger logger)
        {
            List<ScanResult> scanResults = new List<ScanResult>();

            // Get all files recursively in the directory with applicable extensions
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => applicableExtensions.Contains(Path.GetExtension(file)));

            foreach (var file in files)
            {
                try
                {
                    // Read the contents of the file
                    var lines = File.ReadAllLines(file);

                    // Scan each line for exposed secrets
                    for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                    {
                        var line = lines[lineNumber];

                        // Check for exposed secrets
                        if (ContainsExposedSecret(line))
                        {
                            var username = ExtractUsername(line);
                            var password = ExtractPassword(line);

                            var scanResult = new ScanResult
                            {
                                FileName = file,
                                LineNumber = lineNumber + 1,
                                Username = username,
                                Password = password
                            };

                            scanResults.Add(scanResult);

                            logger.LogSensitiveInfo(file, "Exposed Secret", line, lineNumber + 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error processing file '{file}': {ex.Message}");
                }
            }

            return scanResults;
        }

        static bool ContainsExposedSecret(string line)
        {
            // Check for exposed secrets
            // Modify this method based on the patterns or criteria for exposed secrets you want to detect
            // Return true if an exposed secret is found, false otherwise
            return line.Contains("password") || line.Contains("api_key");
        }

        static string ExtractUsername(string line)
        {
            // Extract the username from the line
            // Modify this method based on the patterns or criteria for extracting usernames
            // Return the extracted username

            // Example: Extracting username between "Username:" and "Password:"
            int usernameStartIndex = line.IndexOf("Username:") + 9;
            int usernameEndIndex = line.IndexOf("Password:");
            if (usernameStartIndex >= 0 && usernameEndIndex >= 0)
            {
                return line.Substring(usernameStartIndex, usernameEndIndex - usernameStartIndex).Trim();
            }

            return string.Empty;
        }

        static string ExtractPassword(string line)
        {
            // Extract the password from the line
            // Modify this method based on the patterns or criteria for extracting passwords
            // Return the extracted password

            // Example: Extracting password between "Password:" and the end of the line
            int passwordStartIndex = line.IndexOf("Password:") + 9;
            if (passwordStartIndex >= 0 && passwordStartIndex < line.Length)
            {
                return line.Substring(passwordStartIndex).Trim();
            }

            return string.Empty;
        }

       static void ExportToExcel(List<ScanResult> scanResults, string filePath)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Scan Results");

            // Set headings
            worksheet.Cell(1, 1).Value = "Filename";
            worksheet.Cell(1, 2).Value = "Line number";
            worksheet.Cell(1, 3).Value = "Username";
            worksheet.Cell(1, 4).Value = "Password";

            // Write scan results
            for (int i = 0; i < scanResults.Count; i++)
            {
                var result = scanResults[i];
                worksheet.Cell(i + 2, 1).Value = result.FileName;
                worksheet.Cell(i + 2, 2).Value = result.LineNumber;
                worksheet.Cell(i + 2, 3).Value = result.Username;
                worksheet.Cell(i + 2, 4).Value = result.Password;
            }

            // Save Excel file
            workbook.SaveAs(filePath);
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

    class ScanResult
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
