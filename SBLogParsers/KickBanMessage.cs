using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public class KickBanMessage : LogMessage
    {
        public const string BanIndicator = " was banned by ";
        public const string KickIndicator = " was kicked out of the channel by ";
        public const string ReasonStart = " (";
        public const string ReasonEnd = ").";

        #region Constructors

        public KickBanMessage()
        {
            KickedBy = String.Empty;
            Username = String.Empty;
            Reason = String.Empty;
            EventType = EventType.None;

            Type = MessageType.KickBan;
        }
        
        private KickBanMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);
        }

        #endregion

        #region Properties

        public string KickedBy
        {
            get;
            protected set;
        }

        public string Username
        {
            get;
            protected set;
        }

        public string Reason
        {
            get;
            protected set;
        }

        public EventType EventType
        {
            get;
            protected set;
        }

        public bool IsKick
        {
            get { return EventType == EventType.Kick; }
        }

        public bool IsBan
        {
            get { return EventType == EventType.Ban; }
        }

        public bool ReasonGiven
        {
            get { return Reason.Length > 0; }
        }

        #endregion

        public virtual void CopyTo(KickBanMessage dest)
        {
            base.CopyTo(dest);

            dest.KickedBy = KickedBy;
            dest.Username = Username;
            dest.Reason = Reason;
            dest.EventType = EventType;
        }

        public override string ToString()
        {
            bool isBan = (EventType == EventType.Ban);
            return String.Concat(Timestamp, WordSeparator, Username,
                isBan ? BanIndicator : KickIndicator,
                KickedBy, ReasonStart, Reason, ReasonEnd);
        }

        #region Static Methods

        public static new KickBanMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static KickBanMessage Parse(LogMessage message)
        {
            // These messages are very similar. If it's a valid chat message, it's not a kick or ban.
            if (UserTalkMessage.QuickCheck(message))
                throw InvalidFormatException;

            KickBanMessage msg = new KickBanMessage(message);
            msg.Username = msg.Content.Substring(0, msg.Content.IndexOf(WordSeparator));

            // What type of action, kick or ban?
            string rest = msg.Content.Substring(msg.Username.Length);
            if (rest.StartsWith(KickIndicator))
            {
                rest = rest.Substring(KickIndicator.Length);
                msg.EventType = EventType.Kick;
            }
            else if (rest.StartsWith(BanIndicator))
            {
                rest = rest.Substring(BanIndicator.Length);
                msg.EventType = EventType.Ban;
            }

            // who is responsible?
            msg.KickedBy = rest.Split(WordSeparator)[0];
            if (rest.Length == msg.KickedBy.Length)
                msg.KickedBy = msg.KickedBy.Substring(0, msg.KickedBy.Length - 1);
            else
            {
                // why did they do it?
                rest = rest.Substring(msg.KickedBy.Length);
                if (rest.StartsWith(ReasonStart))
                {
                    msg.Reason = rest.Substring(ReasonStart.Length);

                    // Remove the trailing bits if they are there.
                    foreach (char c in ReasonEnd.Reverse())
                    {
                        if (msg.Reason.EndsWith(c.ToString()))
                            msg.Reason = msg.Reason.Substring(0, msg.Reason.Length - 1);
                    }
                }
            }

            return msg;
        }

        public static bool TryParse(string line, out KickBanMessage message)
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

        public static bool TryParse(LogMessage msg, out KickBanMessage message)
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

            if (UserTalkMessage.QuickCheck(message)) return false;

            int space = content.IndexOf(WordSeparator);
            if (space == -1) return false;
            content = content.Substring(space);

            // This will return true if a user says one of the key phrases as the first part of their message.
            //   Checking the UserTalkMessage check above should eliminate that.
            return (content.StartsWith(KickIndicator) || content.StartsWith(BanIndicator));
        }

        #endregion
    }
}
