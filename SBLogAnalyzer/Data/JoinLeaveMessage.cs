﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer.Data
{
    public class JoinLeaveMessage : LogMessage
    {
        public const string EventMessagePrefix = "--";
        public const string JoinMessageText = "has joined the channel";
        public const string LeaveMessageText = "has left the channel";

        #region Constructors

        public JoinLeaveMessage()
        {
            Username = String.Empty;
            Type = EventType.None;
        }

        private JoinLeaveMessage(LogMessage msg) : this()
        {
            Timestamp = msg.Timestamp;
            Content = msg.Content;
        }

        #endregion

        #region Properties

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

        #endregion

        public override string ToString()
        {
            return String.Concat(Timestamp, WordSeparator, EventMessagePrefix, WordSeparator, Content);
        }

        #region Static Methods

        public static new JoinLeaveMessage Parse(string line)
        {
            return Parse(LogMessage.Parse(line));
        }

        public static JoinLeaveMessage Parse(LogMessage message)
        {
            JoinLeaveMessage msg = new JoinLeaveMessage(message);
            if (msg.Content.StartsWith(EventMessagePrefix))
            {
                string[] parts = msg.Content.Split(WordSeparator);

                if (msg.Content.Contains(JoinMessageText))
                    msg.Type = EventType.UserJoin;
                else if (msg.Content.Contains(LeaveMessageText))
                    msg.Type = EventType.UserLeft;

                if (msg.Type == EventType.UserJoin || msg.Type == EventType.UserLeft)
                {
                    msg.Username = parts[1];
                    msg.Content = msg.Content.Substring(EventMessagePrefix.Length + 1);
                }
                else
                    throw InvalidFormatException;
            }
            else
                throw InvalidFormatException;

            return msg;
        }

        public static bool TryParse(string line, out JoinLeaveMessage message)
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

        public static bool TryParse(LogMessage msg, out JoinLeaveMessage message)
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
