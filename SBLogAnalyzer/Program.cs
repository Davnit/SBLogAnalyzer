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
            Console.WriteLine("StealthBot Log Analyzer");
            Console.WriteLine("by Pyro");
            Console.WriteLine();

            // Make sure the log storage directory exists.
            DirectoryInfo logDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Logs"));
            if (!logDir.Exists)
            {
                Console.WriteLine("Log file directory doesn't exist.");
                logDir.Create();
                Console.ReadKey();
                return;
            }

            // Read all of the files and parse each line into a message object.
            //   Dates from the file names are combined with the inline timestamp to establish context.
            //   Invalid lines and other junk are not parsed.

            // Strings to indicate that a file should not be processed.
            string[] doNotProcess = new string[4] { "WHISPERS", "PACKETLOG", "master", "commands" };

            List<LogMessage> messages = new List<LogMessage>();
            StringComparison sComp = StringComparison.OrdinalIgnoreCase;
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            // Number of files, lines, and bytes.
            int fileCount = 0, totalLines = 0;
            long totalSize = 0;

            // Largest file and its size.
            string largestFile = String.Empty;
            long largestSize = 0;

            foreach (FileInfo file in logDir.EnumerateFiles("*.txt").OrderBy(f => f.CreationTime))
            {
                if (file.Name.ContainsAny(doNotProcess))
                    continue;

                // How big is this file?
                long fileSize = file.Length;
                if (fileSize > largestSize)
                {
                    largestFile = file.Name;
                    largestSize = fileSize;
                }

                totalSize += fileSize;
                Console.Write("Processing file: {0} [{1} KB] ...", file.Name, (fileSize / 1000));

                #region File Processing

                int lineNumber = 0;
                fileCount++;

                // Get the date from the file name.
                DateTime fileDate = DateTime.Parse(Path.GetFileNameWithoutExtension(file.Name)).Date;

                // Read the file
                using (StreamReader reader = file.OpenText())
                {
                    while (!reader.EndOfStream)
                    {
                        lineNumber++;
                        totalLines++;

                        string line = reader.ReadLine();

                        // Make sure the line is at least somewhat valid
                        //   Some messages are shown without timestamps, such as plugin system /updates
                        //   We don't really care about these, so they shouldn't be parsed.
                        if (LogMessage.QuickCheck(line))
                        {
                            try
                            {
                                LogMessage msg = LogMessage.Parse(line);
                                msg.SetDate(fileDate);

                                // is there... more?
                                #region Upgrade Checks

                                if (TaggedMessage.QuickCheck(msg))
                                {
                                    TaggedMessage tagged;
                                    if (TaggedMessage.TryParse(msg, out tagged))
                                        msg = tagged;
                                }
                                else if (ChannelJoinMessage.QuickCheck(msg))
                                {
                                    ChannelJoinMessage join;
                                    if (ChannelJoinMessage.TryParse(msg, out join))
                                        msg = join;
                                }
                                else if (JoinLeaveMessage.QuickCheck(msg))
                                {
                                    JoinLeaveMessage jlm;
                                    if (JoinLeaveMessage.TryParse(msg, out jlm))
                                        msg = jlm;
                                }
                                else if (UserTalkMessage.QuickCheck(msg))
                                {
                                    UserTalkMessage utm;
                                    if (UserTalkMessage.TryParse(msg, out utm))
                                        msg = utm;
                                }
                                else if (KickBanMessage.QuickCheck(msg))
                                {
                                    KickBanMessage kbm;
                                    if (KickBanMessage.TryParse(msg, out kbm))
                                        msg = kbm;
                                }

                                #endregion

                                messages.Add(msg);
                            }
                            catch
                            {
                                Console.WriteLine();
                                Console.WriteLine("Exception raised while parsing message.");
                                Console.WriteLine(" - Current file: {0}", file.Name);
                                Console.WriteLine(" -  Line number: {0}", lineNumber);
                                throw;
                            }
                        }
                    }
                }
                #endregion

                Console.WriteLine("... {0} lines.", lineNumber);
            }

            // Put all of the resulting messages into a list ordered from earliest to latest.
            messages = messages.OrderBy(m => m.Time).ToList();

            Console.WriteLine("Processing complete.");
            Console.WriteLine();

            
            Console.WriteLine("        Files: {0}", fileCount);
            Console.WriteLine("  Total Lines: {0}", totalLines);
            Console.WriteLine("   Total Size: {0:n2} MB", ((double)(totalSize / 1000) / 1000));
            Console.WriteLine(" Largest file: {0} @ {1:n0} KB", largestFile, (largestSize / 1000));
            Console.WriteLine(" Average size: {0:n0} KB", ((totalSize / fileCount) / 1000));

            Console.WriteLine("    Messages: {0}", messages.Count);
            Console.WriteLine("   Time Span: {0:n0} days", (messages.Last().Time - messages.First().Time).TotalDays);

            Console.WriteLine();

            // Now that all of the files have been read and parsed, look at them a little more closely.
            //   If we can figure out anything else about a message, it'll happen here.

            Console.WriteLine("Beginning analysis ...");

            // Go through all the messages and figure out what channels they are from, if any.
            string lastChannel = String.Empty;
            string unknownChannel = "## Unknown Channel ##";
            foreach (LogMessage message in messages)
            {
                // This indicates a new channel was joined.
                if (message is ChannelJoinMessage)
                    lastChannel = message.Channel;

                if (message is TaggedMessage)
                {
                    // This indicates the bot was disconnected from the server, and by extension the channel.
                    if (((TaggedMessage)message).Tag.Equals("BNCS", sComp) && message.Content.Equals("Disconnected.", sComp))
                        lastChannel = String.Empty;
                }

                // This indicates the user disconnected the bot.
                if (message.Content.Equals("All connections closed.", sComp))
                    lastChannel = String.Empty;

                // Assign the channel
                message.Channel = lastChannel;

                if (message is UserTalkMessage || message is JoinLeaveMessage)
                {
                    if (message.Channel.Length == 0)
                        message.Channel = unknownChannel;
                }
            }

            // Get some sub-enums
            var chat = messages.Where(m => m is UserTalkMessage).Select(m => (UserTalkMessage)m); ;
            var joins = messages.Where(m => m is JoinLeaveMessage).Select(m => (JoinLeaveMessage)m); ;
            var channels = messages.Where(m => m is ChannelJoinMessage).Select(m => (ChannelJoinMessage)m);
            var removals = messages.Where(m => m is KickBanMessage).Select(m => (KickBanMessage)m);

            Console.WriteLine(" - {0} chat messages", chat.Count());
            Console.WriteLine(" - {0} channels", channels.Select(m => m.Channel).Distinct(comparer).Count());
            Console.WriteLine(" - {0} unique users", chat.Select(m => m.Username).Concat(joins.Select(m => m.Username)).Distinct(comparer).Count());
            Console.WriteLine(" - {0} operator actions", removals.Count());
            Console.WriteLine();

            int position = 0;
            Console.WriteLine("Most joined channels: ");
            foreach (var res in channels.Select(c => c.Channel).GroupBy(s => s, comparer).Select(g => new { Name = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count))
            {
                position++;
                Console.WriteLine(" - #{0} -> {1}: {2}", position, res.Name, res.Count);

                if (position == 10)
                    break;
            }
            Console.WriteLine();

            position = 0;
            Console.WriteLine("Most active channels: ");
            foreach (var res in chat.Select(c => c.Channel).GroupBy(s => s, comparer).Select(g => new { Name = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count))
            {
                position++;
                Console.WriteLine(" - #{0} -> {1}: {2}", position, res.Name, res.Count);

                if (position == 10)
                    break;
            }
            Console.WriteLine();

            position = 0;
            Console.WriteLine("Most active users: ");
            foreach (var res in chat.Select(c => c.Username).GroupBy(s => s, comparer).Select(g => new { User = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count))
            {
                position++;
                Console.WriteLine(" - #{0} -> {1}: {2}", position, res.User, res.Count);

                if (position == 10)
                    break;
            }
            Console.WriteLine();

            position = 0;
            Console.WriteLine("Most kicked users: ");
            foreach (var res in removals.Where(r => r.Type == EventType.Kick).Select(c => c.Username).GroupBy(s => s, comparer).Select(g => new { User = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count))
            {
                position++;
                Console.WriteLine(" - #{0} -> {1}: {2}", position, res.User, res.Count);

                if (position == 10)
                    break;
            }
            Console.WriteLine();

            position = 0;
            Console.WriteLine("Most banned users: ");
            foreach (var res in removals.Where(r => r.Type == EventType.Ban).Select(c => c.Username).GroupBy(s => s, comparer).Select(g => new { User = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count))
            {
                position++;
                Console.WriteLine(" - #{0} -> {1}: {2}", position, res.User, res.Count);

                if (position == 10)
                    break;
            }
            Console.WriteLine();

            position = 0;
            Console.WriteLine("Most active operators: ");
            foreach (var res in removals.Select(c => c.KickedBy).GroupBy(s => s, comparer).Select(g => new { User = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count))
            {
                position++;
                Console.WriteLine(" - #{0} -> {1}: {2}", position, res.User, res.Count);

                if (position == 10)
                    break;
            }

            Console.WriteLine("Work complete!");
            Console.ReadKey();
        }
    }
}
