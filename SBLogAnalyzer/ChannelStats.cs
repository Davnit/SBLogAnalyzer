using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer
{
    public class ChannelStats
    {
        public ChannelStats(string channelName)
        {
            ChannelName = channelName;

            TimesJoined = 0;
            TotalUserJoins = 0;
            TotalUserLeaves = 0;
            TotalChatMessages = 0;
            WordCount = 0;
            TimeInChannel = 0;
        }

        public string ChannelName
        {
            get;
            private set;
        }

        // Number of times you joined the channel
        public int TimesJoined
        {
            get;
            set;
        }

        // Number of other users who joined the channel
        public int TotalUserJoins
        {
            get;
            set;
        }

        // Number of other users who left the channel
        public int TotalUserLeaves
        {
            get;
            set;
        }

        // Number of times another user was removed from the channel
        public int TotalRemoved
        {
            get;
            set;
        }

        // Number of chat messages seen in the channel
        public int TotalChatMessages
        {
            get;
            set;
        }

        // Number of words seen in the channel
        public int WordCount
        {
            get;
            set;
        }

        // Duration of time spent in the channel (in seconds)
        public double TimeInChannel
        {
            get;
            set;
        }

        // The date and time you last joined the channel.
        public DateTime LastJoined
        {
            get;
            set;
        }
    }
}
