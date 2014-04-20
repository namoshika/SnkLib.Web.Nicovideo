using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Nicovideo
{
    public class NicoTextComment : NicoComment
    {
        public NicoTextComment(string text, TimeSpan begin, TimeSpan duration, bool isAnonymity, string userId, NicoCommand[] cmds)
            : base(begin, duration, userId, cmds) { Text = text; }
        public string Text { get; set; }
    }
}
