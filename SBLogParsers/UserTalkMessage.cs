using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public class UserTalkMessage : LogMessage
    {
        public const string UserStart = "<";
        public const string UserEnd = ">";
        public const string WhisperStart = "<From ";

        #region Constructors

        public UserTalkMessage()
        {
            Username = String.Empty;
            Type = EventType.None;
            Content = String.Empty;

            MessageType = MessageType.Chat;
        }

        private UserTalkMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);
        }

        #endregion

        #region Properties

        public string Username
        {
            get;
            protected set;
        }

        public EventType Type
        {
            get;
            protected set;
        }

        public override string Content
        {
            get;
            protected set;
        }

        public bool IsEmote
        {
            get { return Type == EventType.UserEmote; }
        }

        #endregion

        public virtual void CopyTo(UserTalkMessage dest)
        {
            base.CopyTo(dest);

            dest.Username = Username;
            dest.Type = Type;
            dest.Content = Content;
        }

        public override string ToString()
        {
            bool isEmote = (Type == EventType.UserEmote);
            return String.Concat(Timestamp, WordSeparator, UserStart, Username, 
                isEmote ? WordSeparator + Content : UserEnd, 
                isEmote ? UserEnd : WordSeparator + Content);
        }

        #region Static Methods

        public static new UserTalkMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static UserTalkMessage Parse(LogMessage message)
        {
            UserTalkMessage msg = new UserTalkMessage(message);
            string[] parts = msg.Content.Split(WordSeparator);

            if (parts[0].StartsWith(UserStart) && parts[0].EndsWith(UserEnd))
            {
                msg.Type = EventType.UserTalk;
                msg.Username = parts[0].Substring(UserStart.Length);
                msg.Username = msg.Username.Substring(0, parts[0].Length - (UserStart.Length + UserEnd.Length));
                msg.Content = msg.Content.Substring(parts[0].Length + 1);
            }
            else if (parts[0].StartsWith(UserStart) && msg.Content.EndsWith(UserEnd))
            {
                msg.Type = EventType.UserEmote;
                msg.Username = parts[0].Substring(1);
                msg.Content = msg.Content.Substring(parts[0].Length + 1);
                msg.Content = msg.Content.Substring(0, msg.Content.Length - UserEnd.Length);
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out UserTalkMessage message)
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

        public static bool TryParse(LogMessage msg, out UserTalkMessage message)
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

            string[] parts = content.Split(WordSeparator);
            bool hasUserStart = parts[0].StartsWith(UserStart);

            return (hasUserStart && parts[0].EndsWith(UserStart) ||
                hasUserStart && parts.Last().EndsWith(UserEnd));
        }

        #endregion
    }
}
