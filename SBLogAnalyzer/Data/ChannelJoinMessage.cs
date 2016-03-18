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
            ChannelName = String.Empty;
            IsClanChannel = false;
        }

        private ChannelJoinMessage(LogMessage msg) : this()
        {
            Timestamp = msg.Timestamp;
            Content = msg.Content;
            Time = msg.Time;
        }

        #endregion

        #region Properties

        public string ChannelName
        {
            get;
            protected set;
        }

        public bool IsClanChannel
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
                msg.ChannelName = msg.Content.Substring(JoinedChannel.Length);
                msg.ChannelName = msg.ChannelName.Substring(0, msg.ChannelName.Length - 3);

                msg.IsClanChannel = msg.ChannelName.StartsWith(ClanPrefix, StringComparison.OrdinalIgnoreCase);
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
