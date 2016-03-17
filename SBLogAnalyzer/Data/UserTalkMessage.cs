﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
{
    public class UserTalkMessage : LogMessage
    {
        public const string UserStart = "<";
        public const string UserEnd = ">";
        public const string WhisperStart = "<From ";

        public UserTalkMessage()
        {
            Username = String.Empty;
            Type = EventType.None;
            Content = String.Empty;
        }

        public UserTalkMessage(LogMessage msg) : this()
        {
            Timestamp = msg.Timestamp;
            Content = msg.Content;
        }

        public string Username
        {
            get;
            protected set;
        }

        public EventType Type
        {
            get;
            protected set;
        }

        public override string Content
        {
            get;
            protected set;
        }

        public override string ToString()
        {
            bool isEmote = (Type == EventType.UserEmote);
            return String.Concat(Timestamp, WordSeparator, UserStart, Username, isEmote ? Content : UserEnd, isEmote ? UserEnd : Content);
        }

        public static new UserTalkMessage Parse(string line)
        {
            UserTalkMessage msg = new UserTalkMessage(LogMessage.Parse(line));
            string[] parts = msg.Content.Split(WordSeparator);

            if (parts[0].StartsWith(UserStart) && parts[0].EndsWith(UserEnd))
            {
                msg.Type = EventType.UserTalk;
                msg.Username = parts[0].Substring(UserStart.Length);
                msg.Username = msg.Username.Substring(0, parts[0].Length - (UserStart.Length + UserEnd.Length));
                msg.Content = msg.Content.Substring(parts[0].Length + 1);
            }
            else if (parts[0].StartsWith(UserStart) && msg.Content.EndsWith(UserEnd))
            {
                msg.Type = EventType.UserEmote;
                msg.Username = parts[0].Substring(1);
                msg.Content = msg.Content.Substring(parts[0].Length + 1);
                msg.Content = msg.Content.Substring(0, msg.Content.Length - UserEnd.Length);
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out UserTalkMessage message)
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
    }
}