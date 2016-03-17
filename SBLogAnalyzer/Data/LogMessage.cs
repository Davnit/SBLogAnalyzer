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

        #region Constructors

        protected LogMessage()
        {
            Timestamp = String.Empty;
            Content = String.Empty;
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

        #endregion

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, Content);
        }

        #region Static Methods

        protected static readonly FormatException InvalidFormatException = new FormatException("Invalid log message format.");

        public static LogMessage Parse(string line)
        {
            string[] parts = line.Split(WordSeparator);

            LogMessage msg = new LogMessage();
            msg.Timestamp = parts[0];
            msg.Content = parts[1];

            if (!msg.Timestamp.StartsWith("[") || !msg.Timestamp.EndsWith("]"))
                throw InvalidFormatException;

            char[] allowed = new char[5] { ':', '.', 'A', 'P', 'M' };
            for (int i = 0; i < msg.Timestamp.Length; i++)
            {
                char x = msg.Timestamp[i];
                if (!char.IsNumber(x) && !allowed.Contains(x))
                    throw InvalidFormatException;
            }
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
