using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public enum EventType
    {
        None,
        UserJoin,
        UserLeft,
        JoinedChannel,
        UserTalk,
        UserEmote,
        WhisperIn,
        WhisperOut,
        Kick,
        Ban
    }
}
