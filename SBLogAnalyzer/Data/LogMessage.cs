using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
{
    public class LogMessage
    {
        public const char WordSeparator = ' ';
        public const string StampStart = "[";
        public const string StampEnd = "]";

        public static readonly char[] TimestampCharacters = new char[7] { '[', ']', ':', '.', 'A', 'P', 'M' };

        #region Constructors

        protected LogMessage()
        {
            Timestamp = String.Empty;
            Content = String.Empty;
            Time = DateTime.Now;

            Channel = String.Empty;
        }

        #endregion

        #region Properties

        public string Timestamp
        {
            get;
            protected set;
        }

        public virtual string Content
        {
            get;
            protected set;
        }

        public DateTime Time
        {
            get;
            protected set;
        }

        public string Channel
        {
            get;
            set;
        }

        #endregion

        public void SetDate(DateTime date)
        {
            Time = new DateTime(date.Year, date.Month, date.Day, Time.Hour, Time.Minute, Time.Second, Time.Millisecond, DateTimeKind.Local);
        }

        public void SetTime(DateTime time)
        {
            Time = new DateTime(Time.Year, Time.Month, Time.Day, time.Hour, time.Minute, time.Second, time.Millisecond, DateTimeKind.Local);
        }

        public virtual void CopyTo(LogMessage dest)
        {
            dest.Timestamp = Timestamp;
            dest.Content = Content;
            dest.Time = Time;

            dest.Channel = Channel;
        }

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, Content);
        }

        #region Static Methods

        protected static readonly FormatException InvalidFormatException = new FormatException("Invalid log message format.");

        public static LogMessage Parse(string line)
        {
            line = line.TrimStart('\0', '\t', ' ');
            string[] parts = line.Split(WordSeparator);

            LogMessage msg = new LogMessage();
            msg.Timestamp = parts[0];
            msg.Content = line.Substring(parts[0].Length + 1);

            if (!msg.Timestamp.StartsWith(StampStart))
                throw InvalidFormatException;

            if (!msg.Timestamp.EndsWith(StampEnd))
            {
                if (parts[1].EndsWith(StampEnd))
                {
                    msg.Timestamp += parts[1];
                    msg.Content = msg.Content.Substring(parts[1].Length + 1);
                }
                else
                    throw InvalidFormatException;
            }

            for (int i = 0; i < msg.Timestamp.Length; i++)
            {
                char x = msg.Timestamp[i];
                if (!char.IsNumber(x) && !TimestampCharacters.Contains(x))
                    throw InvalidFormatException;
            }

            msg.SetTime(DateTime.Parse(msg.Timestamp.Substring(1, msg.Timestamp.Length - 2)));
            return msg;
        }

        public static bool TryParse(string line, out LogMessage message)
        {
            message = null;
            try
            {
                message = Parse(line);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
