using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
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
            Type = EventType.None;
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

        public EventType Type
        {
            get;
            protected set;
        }

        #endregion

        public override string ToString()
        {
            return base.ToString();
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

            string rest = msg.Content.Substring(msg.Username.Length);

            // function to evaluate properties based on message and indicator
            Action<string, string> p = (s, indicator) =>
            {
                msg.KickedBy = s.Substring(indicator.Length).Split(new string[1] { ReasonStart }, StringSplitOptions.None)[0];

                msg.Reason = s.Substring(indicator.Length + msg.KickedBy.Length + ReasonStart.Length);
                if (msg.Reason.EndsWith(ReasonEnd))
                    msg.Reason = msg.Reason.Substring(0, msg.Reason.Length - ReasonEnd.Length);
            };

            if (rest.StartsWith(KickIndicator))         // it's a kick
            {
                p(rest, KickIndicator);
                msg.Type = EventType.Kick;
            }
            else if (rest.StartsWith(BanIndicator))     // it's a ban
            {
                p(rest, BanIndicator);
                msg.Type = EventType.Ban;
            }
            else
                throw InvalidFormatException;

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
