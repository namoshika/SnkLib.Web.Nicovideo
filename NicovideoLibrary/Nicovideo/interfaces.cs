using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace SunokoLibrary.Nicovideo
{
    public interface IComment
    {
        TimeSpan BeginTime { get; }
        TimeSpan Duration { get; }
    }
    public interface ICommentContainer : IEnumerable<IComment>
    {
        IComment this[int i] { get; }
        void Add(IComment item);
        bool Remove(IComment item);
        void Clear();
        event CommentEventHandler CommentAdded;
        event CommentEventHandler CommentRemoved;
    }
}
