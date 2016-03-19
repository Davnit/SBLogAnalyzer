using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SBLogParsers;

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
                Console.WriteLine("Storage created at '{0}'", logDir.FullName);
                logDir.Create();
                Console.ReadKey();
                return;
            }

            // Strings to indicate that a file should not be processed.
            string[] doNotProcess = new string[4] { "WHISPERS", "PACKETLOG", "master", "commands" };

            List<LogMessage> messages = new List<LogMessage>();
            StringComparison sComp = StringComparison.OrdinalIgnoreCase;
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            #region File Processing

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
                                    {
                                        msg = jlm;

                                        if (ClanMemberJoin.QuickCheck(jlm))
                                        {
                                            ClanMemberJoin cmj;
                                            if (ClanMemberJoin.TryParse(jlm, out cmj))
                                                msg = cmj;
                                        }
                                    }
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
                

                Console.WriteLine("... {0} lines.", lineNumber);
            }

            #endregion

            // Put all of the resulting messages into a list ordered from earliest to latest.
            messages = messages.OrderBy(m => m.Time).ToList();
            
            Console.WriteLine();
            
            Console.WriteLine("        Files: {0}", fileCount);
            Console.WriteLine("  Total Lines: {0}", totalLines);
            Console.WriteLine("   Total Size: {0:n2} MB", ((double)(totalSize / 1000) / 1000));
            Console.WriteLine(" Largest file: {0} @ {1:n0} KB", largestFile, (largestSize / 1000));
            Console.WriteLine(" Average size: {0:n0} KB", ((totalSize / fileCount) / 1000));

            Console.WriteLine("    Messages: {0}", messages.Count);
            Console.WriteLine("   Time Span: {0:n0} days", (messages.Last().Time - messages.First().Time).TotalDays);

            Console.WriteLine();

            Console.Write("Processing complete. Press enter to begin analysis. ");
            Console.ReadLine();
            Console.WriteLine();

            // Now that all of the files have been read and parsed, look at them a little more closely.
            //   If we can figure out anything else about a message, it'll happen here.

            Console.WriteLine("Beginning analysis ...");

            // Go through all the messages and figure out what channels they are from, if any.
            #region Channel Linking

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

            #endregion

            // Get some sub-enums
            var chat = messages.Where(m => m is UserTalkMessage).Select(m => (UserTalkMessage)m); ;
            var joins = messages.Where(m => m is JoinLeaveMessage).Select(m => (JoinLeaveMessage)m); ;
            var channels = messages.Where(m => m is ChannelJoinMessage).Select(m => (ChannelJoinMessage)m);
            var removals = messages.Where(m => m is KickBanMessage).Select(m => (KickBanMessage)m);

            var clanMembers = messages.Where(m => m is ClanMemberJoin).Select(m => (ClanMemberJoin)m);

            string blizzPostfix = "@Blizzard";
            var blizzReps = chat.Where(m => m.Username.EndsWith(blizzPostfix)).Select(m => new Tuple<string, LogMessage>(m.Username, m));
            blizzReps = blizzReps.Concat(joins.Where(m => m.Username.EndsWith(blizzPostfix)).Select(m => new Tuple<string, LogMessage>(m.Username, m)));
            blizzReps = blizzReps.Concat(removals.Where(m => m.KickedBy.EndsWith(blizzPostfix)).Select(m => new Tuple<string, LogMessage>(m.KickedBy, m)));

            Console.WriteLine(" - {0} chat messages", chat.Count());
            Console.WriteLine(" - {0} channels", channels.Select(m => m.Channel).Distinct(comparer).Count());
            Console.WriteLine(" - {0} unique users", chat.Select(m => m.Username).Concat(joins.Select(m => m.Username)).Distinct(comparer).Count());
            Console.WriteLine(" - {0} operator actions", removals.Count());
            Console.WriteLine(" - {0} clan members from {1} clans", clanMembers.Select(m => m.Username).Distinct(comparer).Count(), clanMembers.Select(m => m.ClanTag).Distinct(comparer).Count());
            Console.WriteLine(" - {0} blizzard employees seen {1} times", blizzReps.Select(r => r.Item1).Distinct(comparer).Count(), blizzReps.Count());
            Console.WriteLine();

            // this function orders the elements by number of occurrences
            Func<IEnumerable<string>, IOrderedEnumerable<Tuple<string, int>>> OrderByCount = (e) => e.GroupBy(s => s, comparer).Select(g => new Tuple<string, int>(g.Key, g.Count())).OrderByDescending(r => r.Item2);

            // Get the leaderboards
            var mostJoined = OrderByCount(channels.Select(c => c.Channel));
            var activeChannels = OrderByCount(chat.Select(c => c.Channel));
            var activeUsers = OrderByCount(chat.Select(c => c.Username));
            var mostKicked = OrderByCount(removals.Where(r => r.Type == EventType.Kick).Select(c => c.Username));
            var mostBanned = OrderByCount(removals.Where(r => r.Type == EventType.Ban).Select(c => c.Username));
            var activeOps = OrderByCount(removals.Select(c => c.KickedBy));
            var mostCommonClans = OrderByCount(clanMembers.Select(c => c.ClanTag));

            #region Leaderboards

            // this function shows the first X elements
            #region ShowLeaders() function
            Action<string, IOrderedEnumerable<Tuple<string, int>>, int> ShowLeaders = (title, items, count) =>
            {
                int position = 0;
                Console.WriteLine(title + ": ");
                foreach (var item in items)
                {
                    position++;
                    Console.WriteLine(" - #{0} -> {1}: {2}", position, item.Item1, item.Item2);

                    if (position == 10)
                        break;
                }
                Console.WriteLine();
            };
            #endregion

            ShowLeaders("Most joined channels", mostJoined, 10);
            ShowLeaders("Most active channels", activeChannels, 10);
            ShowLeaders("Most active users", activeUsers, 10);
            ShowLeaders("Most kicked users", mostKicked, 10);
            ShowLeaders("Most banned users", mostBanned, 10);
            ShowLeaders("Most active operators", activeOps, 10);
            ShowLeaders("Most common clan tags", mostCommonClans, 10);

            #endregion

            Console.Write("Analysis complete. Press enter to begin writing output. ");
            Console.ReadLine();
            Console.WriteLine();

            Console.WriteLine("Writing data to output... this could take a while.");

            // Create writer for the output directory
            CollectionWriter writer = new CollectionWriter("Output");

            // Write the master output file (this will be big)
            Console.WriteLine("Writing master file...");
            writer.WriteFile("Master.txt", messages, m => m.ToString());

            // Write list of all clan tags found and when they were first seen
            writer.WriteFile("ClanTags.txt", clanMembers.GroupBy(m => m.ClanTag, comparer).Select(m => m.First()),
                m => String.Format("{0} -> {1} on {2} in '{3}'.", m.ClanTag, m.Username.Split('@')[0], m.Time.ToString("MMM d, yyyy"), m.Channel));

            // What has Blizzard been up to?
            writer.WriteFile("BlizzardEmployees.txt", blizzReps, r => String.Format("[{0}] {1} >> {2}", r.Item2.Time, r.Item1, r.Item2.Content));

            // Output leaderboards
            Console.WriteLine("Writing leaderboards...");

            #region WriteCounts() method
            Action<string, IOrderedEnumerable<Tuple<string, int>>> WriteCounts = (fileName, items) =>
            {
                writer.WriteFileWithPosition(fileName, items, item => String.Format("#%POSITION% -> {0}: {1}", item.Item1, item.Item2));
            };
            #endregion

            WriteCounts("JoinedChannels.txt", mostJoined);
            WriteCounts("ActiveChannels.txt", activeChannels);
            WriteCounts("ActiveUsers.txt", activeUsers);
            WriteCounts("ActiveOperators.txt", activeOps);
            WriteCounts("MostPopularClans.txt", mostCommonClans);

            // Output per-channel logs
            Console.WriteLine("Writing individual channel logs...");
            foreach (var channel in activeChannels)
            {
                string channelName = channel.Item1;

                writer.CurrentDirectory = "Output\\Channels";
                writer.CurrentDirectory = Path.Combine(writer.CurrentDirectory, channelName);
                Console.WriteLine(" - {0}", channelName);

                // Get a collection of messages from this channel
                var channelMessages = messages.Where(m => m.Channel.Equals(channelName, sComp));

                // Sub collections
                var channelJoins = joins.Where(m => m.Channel.Equals(channelName, sComp));
                var channelChat = chat.Where(m => m.Channel.Equals(channelName, sComp));

                // Write a list of users seen in this channel
                writer.WriteFile("Users.txt", channelJoins.Select(j => j.Username).Concat(channelChat.Select(c => c.Username)).Distinct(comparer), m => m);

                // Is it a clan? If so, who have we seen that's a member?
                if (ChannelJoinMessage.IsClanChannel(channel.Item1))
                {
                    string tag = channelName.Split(LogMessage.WordSeparator)[1];
                    var localMembers = clanMembers.Where(c => c.ClanTag.Equals(tag, sComp)).GroupBy(m => m.Username, comparer).Select(m => m.Last());

                    writer.WriteFile("Members.txt", localMembers, m => String.Format("{0} -> {1} in {2}", m.Username.Split('@')[0], m.Time, m.Channel));
                }

                // Write a separate log file for each date (same as source format)
                foreach (var date in channelMessages.Select(m => m.Time.Date).Distinct())
                {
                    string fileName = date.ToString("yyyy-MM-dd") + ".txt";
                    writer.WriteFile(fileName, channelMessages.Where(m => m.Time.Date.Equals(date)), m => m.ToString());
                }
            }

            Console.WriteLine("Work complete!");
            Console.ReadLine();
        }
    }
}
