using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public class TaggedMessage : LogMessage
    {
        public const string TagStart = "[";
        public const string TagEnd = "]";

        #region Constructors

        public TaggedMessage()
        {
            Tag = String.Empty;

            Type = MessageType.Tagged;
        }

        private TaggedMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);

            Type = MessageType.Tagged;
        }

        #endregion

        #region Properties

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

        #endregion

        public virtual void CopyTo(TaggedMessage dest)
        {
            base.CopyTo(dest);

            dest.Tag = Tag;
            dest.Content = Content;
        }

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, TagStart, Tag, TagEnd, WordSeparator, Content);
        }

        #region Static Methods

        public static new TaggedMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static TaggedMessage Parse(LogMessage message)
        {
            TaggedMessage msg = new TaggedMessage(message);
            string[] parts = msg.Content.Split(WordSeparator);

            if (parts[0].StartsWith(TagStart, sComp) && parts[0].EndsWith(TagEnd, sComp))
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

        public static bool TryParse(LogMessage msg, out TaggedMessage message)
        {
            message = null;
            try
            {
                message = Parse(msg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool QuickCheck(LogMessage message)
        {
            string content = message.Content.Trim();
            if (content.Length == 0) return false;

            // Tags shouldn't have spaces in them. I've never seen one that did.
            return (content.StartsWith(TagStart, sComp) && content.ToLower().Contains(TagEnd) && 
                (content.IndexOf(TagEnd, sComp) < content.IndexOf(WordSeparator)));
        }

        #endregion
    }
}
