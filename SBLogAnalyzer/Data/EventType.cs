using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
{
    public enum EventType
    {
        None,
        UserJoin,
        UserLeft,
        JoinedChannel,
        UserTalk,
        UserEmote,
        Whisper
    }
}
