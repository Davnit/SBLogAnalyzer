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
        public const string WhisperInStart = "<From ";
        public const string WhisperOutStart = "<To ";

        #region Constructors

        public UserTalkMessage()
        {
            Username = String.Empty;
            EventType = EventType.None;
            Content = String.Empty;

            Type = MessageType.Chat;
        }

        private UserTalkMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);

            Type = MessageType.Chat;
        }

        #endregion

        #region Properties

        public string Username
        {
            get;
            protected set;
        }

        public EventType EventType
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
            get { return EventType == EventType.UserEmote; }
        }

        #endregion

        public virtual void CopyTo(UserTalkMessage dest)
        {
            base.CopyTo(dest);

            dest.Username = Username;
            dest.EventType = EventType;
            dest.Content = Content;
        }

        public override string ToString()
        {
            bool isEmote = (EventType == EventType.UserEmote);
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

            if (parts[0].StartsWith(UserStart, sComp) && parts[0].EndsWith(UserEnd, sComp))
            {
                // normal chat
                msg.Username = parts[0].Substring(UserStart.Length, parts[0].Length - (UserStart.Length + UserEnd.Length));

                msg.Content = msg.Content.Substring(parts[0].Length + 1);
                msg.EventType = EventType.UserTalk;
            }
            else if ((parts[0].StartsWith(WhisperInStart, sComp) || parts[0].StartsWith(WhisperOutStart, sComp)) && parts[1].EndsWith(UserEnd, sComp))
            {
                // whispers
                msg.Username = parts[1].Substring(0, parts[1].Length - UserEnd.Length);
                msg.Content = msg.Content.Substring(parts[0].Length + 1 + parts[1].Length + 1);
                msg.EventType = parts[0].StartsWith(WhisperInStart, sComp) ? EventType.WhisperIn : EventType.WhisperOut;
            }
            else if (parts[0].StartsWith(UserStart, sComp) && msg.Content.EndsWith(UserEnd, sComp))
            {
                // emotes
                msg.Username = msg.Content.Substring(UserStart.Length);

                msg.Content = msg.Content.Substring(parts[0].Length + 1);
                msg.Content = msg.Content.Substring(0, msg.Content.Length - UserEnd.Length);

                msg.EventType = EventType.UserEmote;
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
            bool hasUserStart = parts[0].StartsWith(UserStart, sComp);

            if (hasUserStart && parts[0].EndsWith(UserEnd, sComp))
                return true;    // normal message

            if (parts.Length > 1)
            {
                if ((parts[0].StartsWith(WhisperInStart, sComp) || parts[0].StartsWith(WhisperOutStart, sComp)) && parts[1].EndsWith(UserEnd, sComp))
                    return true;    // whisper
            }

            if (hasUserStart && parts.Last().EndsWith(UserEnd, sComp))
                return true;    // emote

            return false;
        }

        #endregion
    }
}
