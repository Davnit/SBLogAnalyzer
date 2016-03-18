using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer
{
    public class ChannelStats
    {
        public string ChannelName
        {
            get;
            set;
        }

        public int TimesJoined
        {
            get;
            set;
        }

        public int TimesRemoved
        {
            get;
            set;
        }

        public int TotalUserJoins
        {
            get;
            set;
        }

        public int UniqueUsers
        {
            get;
            set;
        }

        public int TotalChatMessages
        {
            get;
            set;
        }

        public int WordCount
        {
            get;
            set;
        }

        public int TimeInChannel
        {
            get;
            set;
        }
    }
}
