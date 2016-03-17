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
            Console.WriteLine(" -- StealthBot Log Analyzer");
            Console.WriteLine(" -- by Pyro");
            Console.WriteLine();

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

                    #region File Processing

                    int lineNumber = 0;

                    fileCount++;
                    DateTime fileDate = DateTime.Parse(Path.GetFileNameWithoutExtension(file.Name)).Date;

                    StreamReader reader = file.OpenText();
                    while (!reader.EndOfStream)
                    {
                        lineNumber++;
                        totalLines++;

                        string line = reader.ReadLine();
                        if (line.Trim().Length > 0)
                        {
                            string check = line.Trim();
                            if (check.StartsWith(LogMessage.StampStart) && check.Contains(LogMessage.StampEnd) && !check.EndsWith(LogMessage.StampEnd))
                            {
                                try
                                {
                                    LogMessage msg = LogMessage.Parse(line);
                                    msg.SetDate(fileDate);
                                    messages.Add(msg);
                                }
                                catch
                                {
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

                Console.WriteLine("Processing complete.");
                Console.WriteLine();

                Console.WriteLine("  Messages: {0}", messages.Count);
                Console.WriteLine("     Files: {0}", fileCount);
                Console.WriteLine("     Lines: {0}", totalLines);
                Console.WriteLine("Total Size: {0:n2} MB", ((double)(totalSize / 1000) / 1000));
                Console.WriteLine();

                Console.WriteLine("Beginning analysis ...");

                Dictionary<string, int> tagTracker = new Dictionary<string, int>();
                Dictionary<string, int> joinTracker = new Dictionary<string, int>();
                Dictionary<string, int> talkTracker = new Dictionary<string, int>();

                string inClanDelim = ", in clan ";
                Dictionary<string, JoinLeaveMessage> foundClanTags = new Dictionary<string, JoinLeaveMessage>();

                int tagCount = 0, joinCount = 0, talkCount = 0;
                int wordCount = 0;
                for (int i = 0; i < messages.Count; i++)
                {
                    LogMessage message = messages[i];

                    #region Message Upgrading

                    if (message.Content.StartsWith(TaggedMessage.TagStart))
                    {
                        TaggedMessage newMsg;
                        if (TaggedMessage.TryParse(message, out newMsg))
                        {
                            message = newMsg;
                            tagCount++;

                            if (!tagTracker.ContainsKey(newMsg.Tag))
                                tagTracker.Add(newMsg.Tag, 1);
                            else
                                tagTracker[newMsg.Tag]++;
                        }
                    }
                    else if (message.Content.StartsWith(JoinLeaveMessage.EventMessagePrefix) &&
                        (message.Content.Contains(JoinLeaveMessage.JoinMessageText) || message.Content.Contains(JoinLeaveMessage.LeaveMessageText)))
                    {
                        JoinLeaveMessage newMsg;
                        if (JoinLeaveMessage.TryParse(message, out newMsg))
                        {
                            message = newMsg;
                            joinCount++;

                            if (newMsg.Type == EventType.UserJoin)
                            {
                                if (!joinTracker.ContainsKey(newMsg.Username))
                                    joinTracker.Add(newMsg.Username, 1);
                                else
                                    joinTracker[newMsg.Username]++;

                                
                                if (newMsg.Content.Contains(inClanDelim))
                                {
                                    int tagIndex = newMsg.Content.IndexOf(inClanDelim) + inClanDelim.Length;
                                    string tag = newMsg.Content.Substring(tagIndex, 4).Trim();
                                    if (tag.Contains(")"))
                                        tag = tag.Substring(0, tag.IndexOf(")"));

                                    if (!foundClanTags.ContainsKey(tag))
                                        foundClanTags.Add(tag, newMsg);
                                }
                            }
                        }
                    }
                    else if (message.Content.StartsWith(UserTalkMessage.UserStart))
                    {
                        UserTalkMessage newMsg;
                        if (UserTalkMessage.TryParse(message, out newMsg))
                        {
                            message = newMsg;
                            talkCount++;

                            if (!talkTracker.ContainsKey(newMsg.Username))
                                talkTracker.Add(newMsg.Username, 1);
                            else
                                talkTracker[newMsg.Username]++;

                            wordCount += newMsg.Content.Split(LogMessage.WordSeparator).Count(w => w.Length > 1);
                        }
                    }

                    #endregion
                }

                int totalUpgrade = (tagCount + joinCount + talkCount);
                Console.WriteLine("Upgraded {0}\\{1} messages:", totalUpgrade, messages.Count);
                Console.WriteLine("\t - {0} tagged", tagCount);
                Console.WriteLine("\t - {0} join/leaves", joinCount);
                Console.WriteLine("\t - {0} user talks", talkCount);
                Console.WriteLine();

                Console.WriteLine("{0} unique users were seen.", joinTracker.Keys.Concat(talkTracker.Keys).Distinct(StringComparer.OrdinalIgnoreCase).Count());
                Console.WriteLine("~{0} words were received in chat", wordCount);
                Console.WriteLine();

                #region Print Record Holders

                int position = 0;
                Console.WriteLine("Most joined:");
                foreach (KeyValuePair<string, int> kvp in joinTracker.OrderByDescending(o => o.Value))
                {
                    position++;
                    Console.Write(" - #{0} -> ", position);
                    Console.Write(kvp.Key);
                    Console.Write(": ");
                    Console.WriteLine(kvp.Value);

                    if (position == 20)
                        break;
                }

                position = 0;
                Console.WriteLine("Most talked:");
                foreach (KeyValuePair<string, int> kvp in talkTracker.Where(o => o.Value > 10).OrderByDescending(o => o.Value))
                {
                    position++;
                    Console.Write(" - #{0} -> ", position);
                    Console.Write(kvp.Key);
                    Console.Write(": ");
                    Console.WriteLine(kvp.Value);

                    if (position == 20)
                        break;
                }

                #endregion

                Console.WriteLine("Found {0} clan tags: ", foundClanTags.Count);
                foreach (KeyValuePair<string, JoinLeaveMessage> kvp in foundClanTags.OrderByDescending(p => p.Value.Time))
                {
                    Console.WriteLine(" - {0} seen on {1} -> {2}", kvp.Key, kvp.Value.Username.Split('@')[0], kvp.Value.Time.ToString("MMM d, yyyy @ h:mm tt"));
                }
            }

            Console.ReadKey();
        }
    }
}
