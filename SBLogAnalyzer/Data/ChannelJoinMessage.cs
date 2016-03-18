using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
{
    public class ChannelJoinMessage : LogMessage
    {
        public const string JoinedChannel = "-- Joined channel: ";
        public const string ClanPrefix = "clan";

        #region Constructors

        public ChannelJoinMessage()
        {
            IsClan = false;
        }

        private ChannelJoinMessage(LogMessage msg) : this()
        {
            msg.CopyTo(this);

            IsClan = Channel.StartsWith(ClanPrefix, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Properties

        public bool IsClan
        {
            get;
            protected set;
        }

        #endregion

        #region Static Methods

        public static new ChannelJoinMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static ChannelJoinMessage Parse(LogMessage message)
        {
            ChannelJoinMessage msg = new ChannelJoinMessage(message);
            if (msg.Content.StartsWith(JoinedChannel))
            {
                msg.Channel = msg.Content.Substring(JoinedChannel.Length);
                msg.Channel = msg.Channel.Substring(0, msg.Channel.Length - 3);

                msg.IsClan = msg.Channel.StartsWith(ClanPrefix, StringComparison.OrdinalIgnoreCase);
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

        #endregion
    }
}
