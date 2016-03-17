using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
{
    public class TaggedMessage : LogMessage
    {
        public const string TagStart = "[";
        public const string TagEnd = "]";

        public TaggedMessage()
        {
            Tag = String.Empty;
        }

        private TaggedMessage(LogMessage msg) : this()
        {
            Timestamp = msg.Timestamp;
            Content = msg.Content;
        }

        public string Tag
        {
            get;
            protected set;
        }

        public override string Content
        {
            get;
            protected set;
        }

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, TagStart, Tag, TagEnd, WordSeparator, Content);
        }

        public static new TaggedMessage Parse(string line)
        {
            TaggedMessage msg = new TaggedMessage(LogMessage.Parse(line));
            string[] parts = msg.Content.Split(WordSeparator);

            if (parts[0].StartsWith(TagStart) && parts[0].EndsWith(TagEnd))
            {
                msg.Tag = parts[0].Substring(1, parts[0].Length - 2);
                if (parts.Length > 1)
                    msg.Content = msg.Content.Substring(msg.Tag.Length + 3);
                else
                    msg.Content = String.Empty;
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out TaggedMessage message)
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
    }
}
