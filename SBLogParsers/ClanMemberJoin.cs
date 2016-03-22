using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogParsers
{
    public class ClanMemberJoin : JoinLeaveMessage
    {
        public const string ClanTagPrefix = ", in clan ";
        public const string ClanTagPostfix = ")";

        #region Constructors

        public ClanMemberJoin()
        {
            ClanTag = String.Empty;
        }

        private ClanMemberJoin(JoinLeaveMessage msg) : this()
        {
            msg.CopyTo(this);
        }

        #endregion

        #region Properties

        public string ClanTag
        {
            get;
            protected set;
        }

        #endregion

        public virtual void CopyTo(ClanMemberJoin dest)
        {
            base.CopyTo(dest);

            dest.ClanTag = ClanTag;
        }

        #region Static Methods

        public static new ClanMemberJoin Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static new ClanMemberJoin Parse(LogMessage message)
        {
            return Parse(JoinLeaveMessage.Parse(message));
        }

        public static ClanMemberJoin Parse(JoinLeaveMessage message)
        {
            ClanMemberJoin msg = new ClanMemberJoin(message);

            string content = msg.Content.ToLower();     // only used for comparison
            if (content.Contains(ClanTagPrefix))
            {
                int start = content.IndexOf(ClanTagPrefix) + ClanTagPrefix.Length;
                int end = content.IndexOf(ClanTagPostfix);

                msg.ClanTag = msg.Content.Substring(start, end - start);
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out ClanMemberJoin message)
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

        public static bool TryParse(LogMessage msg, out ClanMemberJoin message)
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

        public static bool TryParse(JoinLeaveMessage msg, out ClanMemberJoin message)
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

        public static bool QuickCheck(JoinLeaveMessage message)
        {
            string content = message.Content.Trim();
            if (content.Length == 0) return false;

            int prefix = content.IndexOf(ClanTagPrefix, sComp);
            return (prefix > 0 && content.IndexOf(ClanTagPostfix, sComp) > prefix) ;
        }

        #endregion
    }
}
