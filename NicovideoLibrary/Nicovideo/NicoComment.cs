using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Nicovideo
{
    public class NicoComment : IComment
    {
        public NicoComment(TimeSpan begin, TimeSpan duration, string userId, NicoCommand[] cmds)
        {
            if (begin == null || duration == null || cmds == null)
                throw new ArgumentNullException("引数にnullを指定することはできません。");

            Commands = cmds;
            BeginTime = begin;
            Duration = duration;
            IsAnonymity = true;
            UserId = userId;
        }

        public TimeSpan BeginTime { get; protected set; }
        public TimeSpan Duration { get; protected set; }
        public bool IsAnonymity { get; protected set; }
        public string UserId { get; protected set; }
        public NicoCommand[] Commands { get; protected set; }
    }
}
