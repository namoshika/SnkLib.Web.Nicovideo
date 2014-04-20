using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Nicovideo
{
    public class NicoCommentContainer : ICommentContainer
    {
        public NicoCommentContainer()
        {
            _comment = new List<IComment>();
        }

        List<IComment> _comment;
        public IComment this[int i]
        {
            get { return _comment[i]; }
        }
        public int Count { get { return _comment.Count; } }
        
        public void Add(IComment item)
        {
            _comment.Add(item);
            OnCommentAdded(new CommentEventArgs(item));
        }
        public bool Remove(IComment item)
        {
            var suc = _comment.Remove(item);
            OnCommentRemoved(new CommentEventArgs(item));
            return suc;
        }
        public void Clear()
        {
            foreach (var item in _comment)
                Remove(item);
            _comment.Clear();
        }
        public static NicoCommentContainer Load(System.Xml.Linq.XDocument source)
        {
            var comments = GetComments(source);
            var container = new NicoCommentContainer();
            container._comment.AddRange(comments);
            return container;
        }
        static NicoComment[] GetComments(System.Xml.Linq.XDocument source)
        {
            var root = source.Element("packet");
            var chats = root.Elements("chat");
            var commes = new List<NicoComment>();

            foreach (var element in chats)
            {
                var cmds = new List<NicoCommand>();
                NicoCommand layout = null;
                NicoCommand size = null;
                NicoCommand color = null;
                var mail = element.Attribute("mail");
                if (mail != null)
                {

                    var args = mail.Value
                        .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                        .ToArray();
                    foreach (var cmd in args.Select(syntax => GetCommand(syntax)))
                        if (cmd != null)
                            switch (cmd.Type)
                            {
                                case CommandType.Layout:
                                    layout = cmd;
                                    break;
                                case CommandType.Color:
                                    color = cmd;
                                    break;
                                case CommandType.Size:
                                    size = cmd;
                                    break;
                            }
                }
                if (layout == null)
                    layout = NicoCommands.Flow;
                if (color == null)
                    color = NicoCommands.White;
                if (size == null)
                    size = NicoCommands.Standard;
                cmds.AddRange(new NicoCommand[] { layout, color, size });

                var rnd = new Random((int)DateTime.Now.Ticks).Next(2);
                if (true)//rnd == 0)
                {
                    var c = new NicoTextComment(
                        element.Value.Replace("\n", "\r\n"),
                        TimeSpan.FromMilliseconds(double.Parse(element.Attribute("vpos").Value) * 10),
                        TimeSpan.FromSeconds(4),
                        true,
                        "test",
                        cmds.ToArray()
                        );
                    commes.Add(c);
                }
                //else if (rnd == 1)
                //{
                //    var c = new NicoButtonComment(
                //        element.Value.Replace("\n", "\r\n"),
                //        TimeSpan.FromMilliseconds(double.Parse(element.Attribute("vpos").Value) * 10),
                //        this,
                //        cmds.ToArray()
                //        );
                //    commes.Add(c);
                //}
                //else
                //    throw new Exception("わけわからん");
            }
            return commes.ToArray();
        }
        static NicoCommand GetCommand(string syntax)
        {
            NicoCommand cmd = null;
            switch (syntax)
            {
                case "ue":
                    cmd = NicoCommands.Top;
                    break;
                case "shita":
                    cmd = NicoCommands.Bottom;
                    break;
                case "red":
                    cmd = NicoCommands.Red;
                    break;
                case "pink":
                    cmd = NicoCommands.Pink;
                    break;
                case "orange":
                    cmd = NicoCommands.Orange;
                    break;
                case "yellow":
                    cmd = NicoCommands.Yellow;
                    break;
                case "green":
                    cmd = NicoCommands.Green;
                    break;
                case "cyan":
                    cmd = NicoCommands.Cyan;
                    break;
                case "blue":
                    cmd = NicoCommands.Blue;
                    break;
                case "purple":
                    cmd = NicoCommands.Purple;
                    break;
                case "black":
                    cmd = NicoCommands.Black;
                    break;
                case "big":
                    cmd = NicoCommands.Big;
                    break;
                case "small":
                    cmd = NicoCommands.Small;
                    break;
            }
            return cmd;
        }

        public event CommentEventHandler CommentAdded;
        protected void OnCommentAdded(CommentEventArgs e)
        {
            if (CommentAdded != null)
                CommentAdded(this, e);
        }
        public event CommentEventHandler CommentRemoved;
        protected void OnCommentRemoved(CommentEventArgs e)
        {
            if (CommentRemoved != null)
                CommentRemoved(this, e);
        }

        public IEnumerator<IComment> GetEnumerator()
        { return _comment.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        { return _comment.GetEnumerator(); }
    }
    public delegate void CommentEventHandler(ICommentContainer sender, CommentEventArgs comment); 
    public class CommentEventArgs : EventArgs
    {
        public CommentEventArgs(IComment comment)
        {
            Comment = comment;
        }
        public IComment Comment{get;private set;}
    }
}
