using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public class ChannelJoinMessage : LogMessage
    {
        public const string JoinedChannel = "-- Joined channel: ";
        public const string ChannelPostfix = " --";
        public const string ClanPrefix = "clan ";

        #region Constructors

        public ChannelJoinMessage()
        {
            Type = MessageType.ChannelJoin;
        }

        private ChannelJoinMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);

            Type = MessageType.ChannelJoin;
        }

        #endregion

        #region Properties

        public bool IsClan
        {
            get { return IsClanChannel(Channel); }
        }

        #endregion

        public virtual void CopyTo(ChannelJoinMessage dest)
        {
            base.CopyTo(dest);
        }

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, JoinedChannel, Channel, ChannelPostfix);
        }

        #region Static Methods

        public static bool IsClanChannel(string channelName)
        {
            return channelName.StartsWith(ClanPrefix, sComp) && !channelName.EndsWith("recruitment", sComp);
        }

        public static new ChannelJoinMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static ChannelJoinMessage Parse(LogMessage message)
        {
            ChannelJoinMessage msg = new ChannelJoinMessage(message);

            if (msg.Content.StartsWith(JoinedChannel, sComp))
            {
                msg.Channel = msg.Content.Substring(JoinedChannel.Length);
                msg.Channel = msg.Channel.Substring(0, msg.Channel.Length - ChannelPostfix.Length);
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out ChannelJoinMessage message)
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

        public static bool TryParse(LogMessage msg, out ChannelJoinMessage message)
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

            return (content.StartsWith(JoinedChannel, sComp) && content.EndsWith(ChannelPostfix, sComp));
        }

        #endregion
    }
}
