using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public class JoinLeaveMessage : LogMessage
    {
        public const string EventMessagePrefix = "--";
        public const string JoinMessageText = "has joined the channel";
        public const string LeaveMessageText = "has left the channel";

        #region Constructors

        public JoinLeaveMessage()
        {
            Username = String.Empty;
            EventType = EventType.None;

            Type = MessageType.JoinLeave;
        }

        private JoinLeaveMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);

            Type = MessageType.JoinLeave;
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

        #endregion

        public virtual void CopyTo(JoinLeaveMessage dest)
        {
            base.CopyTo(dest);

            dest.Username = Username;
            dest.EventType = EventType;
        }

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, EventMessagePrefix, WordSeparator, Content);
        }

        #region Static Methods

        public static new JoinLeaveMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static JoinLeaveMessage Parse(LogMessage message)
        {
            JoinLeaveMessage msg = new JoinLeaveMessage(message);

            string content = msg.Content;
            if (content.StartsWith(EventMessagePrefix, sComp))
            {
                string[] parts = content.Split(WordSeparator);

                if (msg.Content.ToLower().Contains(JoinMessageText))
                    msg.EventType = EventType.UserJoin;
                else if (msg.Content.ToLower().Contains(LeaveMessageText))
                    msg.EventType = EventType.UserLeft;

                if (msg.EventType == EventType.UserJoin || msg.EventType == EventType.UserLeft)
                {
                    msg.Username = msg.Content.Substring(parts[0].Length + 1, parts[1].Length); ;
                    msg.Content = msg.Content.Substring(EventMessagePrefix.Length + 1);
                }
                else
                    throw InvalidFormatException;
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out JoinLeaveMessage message)
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

        public static bool TryParse(LogMessage msg, out JoinLeaveMessage message)
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
            string content = message.Content.Trim().ToLower();
            if (content.Length == 0) return false;

            return (content.StartsWith(EventMessagePrefix, sComp) &&
                (content.Contains(JoinMessageText) || content.Contains(LeaveMessageText)));
        }

        #endregion
    }
}
