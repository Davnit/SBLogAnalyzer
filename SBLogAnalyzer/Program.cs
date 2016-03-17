using SBLogAnalyzer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo logDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Logs"));
            if (!logDir.Exists)
            {
                Console.WriteLine("Log file directory doesn't exist.");
                logDir.Create();
            }
            else
            {
                string[] doNotProcess = new string[4] { "WHISPERS", "PACKETLOG", "master", "commands" };

                List<LogMessage> messages = new List<LogMessage>();

                int fileCount = 0, totalLines = 0;
                long totalSize = 0;
                foreach (FileInfo file in logDir.EnumerateFiles("*.txt").OrderBy(f => f.CreationTime))
                {
                    if (file.Name.ContainsAny(doNotProcess))
                        continue;

                    long fileSize = file.Length;
                    totalSize += fileSize;
                    Console.Write("Processing file: {0} [{1} KB] ...", file.Name, (fileSize / 1000));

                    int lineNumber = 0;

                    fileCount++;
                    StreamReader reader = file.OpenText();
                    while (!reader.EndOfStream)
                    {
                        lineNumber++;
                        totalLines++;

                        string line = reader.ReadLine();
                        if (line.Trim().Length > 0)
                        {
                            messages.Add(LogMessage.Parse(line));
                        }
                    }
                    Console.WriteLine("... {0} lines.", lineNumber);
                }

                Console.WriteLine("Processing complete.");
                Console.WriteLine();

                Console.WriteLine("  Messages: {0}", messages.Count);
                Console.WriteLine("     Files: {0}", fileCount);
                Console.WriteLine("     Lines: {0}", totalLines);
                Console.WriteLine("Total Size: {0:n2} MB", ((totalSize / 1000) / 1000));
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
