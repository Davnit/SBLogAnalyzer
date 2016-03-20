using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public enum MessageType
    {
        Generic,
        Chat,
        Whisper,
        Tagged,
        JoinLeave,
        KickBan,
        ChannelJoin
    }
}
