using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
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
            Type = EventType.None;
        }

        private JoinLeaveMessage(LogMessage msg) : this()
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

        #endregion

        public virtual void CopyTo(JoinLeaveMessage dest)
        {
            base.CopyTo(dest);

            dest.Username = Username;
            dest.Type = Type;
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
            if (msg.Content.StartsWith(EventMessagePrefix))
            {
                string[] parts = msg.Content.Split(WordSeparator);

                if (msg.Content.Contains(JoinMessageText))
                    msg.Type = EventType.UserJoin;
                else if (msg.Content.Contains(LeaveMessageText))
                    msg.Type = EventType.UserLeft;

                if (msg.Type == EventType.UserJoin || msg.Type == EventType.UserLeft)
                {
                    msg.Username = parts[1];
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
            string content = message.Content.Trim();
            if (content.Length == 0) return false;

            return (content.StartsWith(EventMessagePrefix) &&
                (content.Contains(JoinMessageText) || content.Contains(LeaveMessageText)));
        }

        #endregion
    }
}
