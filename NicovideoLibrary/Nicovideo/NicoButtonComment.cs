using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Nicovideo
{
    public class NicoButtonComment : NicoTextComment
    {
        public NicoButtonComment(string text, TimeSpan begin, TimeSpan duration, bool isAnonymity, string userId, NicoCommand[] cmds)
            : base(text, begin, duration, isAnonymity, userId, cmds) { }

        public int ClickCount { get; set; }
    }
}
