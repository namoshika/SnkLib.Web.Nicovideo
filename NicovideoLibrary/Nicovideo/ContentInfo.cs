using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SunokoLibrary.Nicovideo.Net;

namespace SunokoLibrary.Nicovideo
{
    public class ContentInfo
    {
        public ContentInfo(string movieId, CookieContainer cookie)
        {
            _cookie = cookie;

            var src = PrimitiveLibrary.GetThumbInfo(movieId);
            var infos = (System.Xml.Linq.XElement)src.Nodes().FirstOrDefault();
            if (infos.Name == "thumb")
            {
                try
                {
                    _thumbUrl = new Uri(infos.Element("thumbnail_url").Value);

                    Status = StatusType.OK;
                    PostingDate = DateTime.Parse(infos.Element("first_retrieve").Value);
                    WatchUrl = new Uri(infos.Element("watch_url").Value);
                    MovieId = infos.Element("video_id").Value;
                    Title = infos.Element("title").Value;
                    Description = infos.Element("description").Value;
                    ViewCount = int.Parse(infos.Element("view_counter").Value);
                    CommentCount = int.Parse(infos.Element("comment_num").Value);
                    MyListCount = int.Parse(infos.Element("mylist_counter").Value);
                    LatestComment = infos.Element("last_res_body").Value;
                    Embeddable = infos.Element("embeddable").Value == "1";
                    //再生時間を算出し、Lengthを求める
                    {
                        var splits = infos.Element("length").Value.Split(':');
                        var tmpLen = TimeSpan.FromMinutes(double.Parse(splits[0]));
                        Length = tmpLen.Add(TimeSpan.FromSeconds(int.Parse(splits[1])));
                    }

                    //動画形式を取得し、ContentTypeを初期化
                    switch (infos.Element("movie_type").Value)
                    {
                        case "flv":
                            ContentType = MovieType.Flv;
                            break;
                        case "mp4":
                            ContentType = MovieType.MP4;
                            break;
                        case "swf":
                            ContentType = MovieType.SWF;
                            break;
                        default:
                            throw new InvalidOperationException("異常発生");
                    }

                    //タグを処理し、Tagsを初期化
                    var tagList = new List<TagInfo>();
                    foreach (var tagContainer in infos.Elements("tags"))
                        foreach (var tagEle in tagContainer.Elements("tag"))
                        {
                            var locked = tagEle.Attribute("lock");
                            tagList.Add(new TagInfo(tagEle.Value, locked == null ? false : locked.Value == "1", tagContainer.Attribute("domain").Value));
                        }
                    Tags = tagList.ToArray();

                    var thumbType = infos.Element("thumb_type").Value;
                    var noLvePlay = infos.Element("no_live_play").Value;
                    var mviSize = int.Parse(infos.Element("size_high").Value);
                    var mviEcoSize = int.Parse(infos.Element("size_low").Value);
                }
                catch (NullReferenceException e)
                {
                    throw new Exception("ニコニコ動画の\"ThumbInfo\"APIの返す情報の書式が変です。", e);
                }
            }
            else if (infos.Name == "error")
            {
                Status = StatusType.Deleted;
                MovieId = movieId;
                //var code = infos.Element("code").Value;
                //var desc = infos.Element("description").Value;
            }
            else
                throw new Exception("ニコニコ動画のAPIからのレスポンスに異常があります。");
        }
        public ContentInfo(
            StatusType status, string movieId, string title, TimeSpan length, MovieType type, CookieContainer cookie,
            string desc = null, string latestComme = null, bool embed = false,
            int viewCnt = -1, int cmmeCnt = -1, int mylistCnt = -1, Uri watchUrl = null,
            System.Windows.Media.Imaging.BitmapSource thumb = null, TagInfo[] tags = null)
        {
            if (tags == null)
                tags = new TagInfo[0];

            Status = status;
            WatchUrl = watchUrl;
            MovieId = movieId;
            Title = title;
            Description = desc;
            ContentType = type;
            Length = length;
            ViewCount = viewCnt;
            CommentCount = cmmeCnt;
            MyListCount = mylistCnt;
            LatestComment = latestComme;
            Embeddable = embed;
            Thumbnail = thumb;
            Tags = tags;
            _cookie = cookie;
        }

        Uri _thumbUrl;
        MovieInfo _mviInfo;
        CookieContainer _cookie;
        System.Windows.Media.Imaging.BitmapSource _thumb;
        public StatusType Status { get; protected set; }
        public Uri WatchUrl { get; protected set; }
        public string MovieId { get; protected set; }
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public MovieType ContentType { get; protected set; }
        public DateTime PostingDate { get; protected set; }
        public TimeSpan Length { get; protected set; }
        public int ViewCount { get; protected set; }
        public int CommentCount { get; protected set; }
        public int MyListCount { get; protected set; }
        public bool Embeddable { get; protected set; }
        public string LatestComment { get; protected set; }
        public TagInfo[] Tags { get; protected set; }
        public MovieInfo MovieInfo
        {
            get
            {
                if (_mviInfo == null)
                    _mviInfo = new MovieInfo(MovieId, _cookie);
                return _mviInfo;
            }
        }
        public System.Windows.Media.Imaging.BitmapSource Thumbnail
        {
            get
            {
                if (_thumb == null)
                    _thumb = new System.Windows.Media.Imaging.BitmapImage(_thumbUrl);
                return _thumb;
            }
            protected set { _thumb = value; }
        }

        public RelationContainer GetRelation(SortKeys key, bool ascendingSort)
        {
            var src = new RelationContainer(MovieId, key, ascendingSort, _cookie);
            return src;
        }
        public void RefreshMovieInfo()
        {
            _mviInfo = new MovieInfo(MovieId, _cookie);
        }
    }

    public class MovieInfo
    {
        public MovieInfo(string movieId, CookieContainer cookie)
        {
            #region getflvで取得できる情報
            //thread_id: 1173108780
            //l: 320
            //url: http://smile-com00.nicovideo.jp/smile?v=9.0468
            //link: http://www.smilevideo.jp/view/9/51910
            //ms: http://msg.nicovideo.jp/10/api/
            //user_id: 51910
            //is_premium: 0
            //nickname: kasi
            //time: 1273838438
            //done: true
            //hms: hiroba05.nicovideo.jp
            //hmsp: 2530
            //hmst: 1000000086
            //hmstk: 1273838498.Wo6aMuOlAOXBMucVE5vSZxRh2VE
            #endregion

            _cookie = cookie;
            var res = PrimitiveLibrary.GetFlv(movieId, cookie);
            if (res.ContainsKey("error"))
            {
                IsFailed = false;
                var regex = System.Text.RegularExpressions.Regex.Match(res["error"], "^invalid_v([1-3])$");
                if (regex.Success)
                    MovieStatus = (FailType)int.Parse(regex.Groups[1].Value);
                else
                    MovieStatus = (res["error"] == "access_locked") ? FailType.AccessLocked : FailType.Unknown;
            }
            else
            {
                MovieId = movieId;
                try
                {
                    IsFailed = true;
                    ThreadId = int.Parse(res["thread_id"]);
                    CommentUrl = new Uri(res["ms"]);
                    ContentUrl = new Uri(res["url"]);
                    ContentControlPageUrl = new Uri(res["link"]);
                    UserId = int.Parse(res["user_id"]);
                    MovieStatus = res.ContainsKey("deleted") ? (FailType)int.Parse(res["deleted"]) : FailType.OK;
                }
                catch (KeyNotFoundException e)
                {
                    throw new Exception("ニコニコ動画の\"getflv\"APIから返された情報の書式がおかしいです。", e);
                }
            }
        }

        CookieContainer _cookie;
        public bool IsFailed { get; protected set; }
        public string MovieId { get; protected set; }
        public int ThreadId { get; protected set; }
        public int UserId { get; protected set; }
        public Uri CommentUrl { get; protected set; }
        public Uri ContentUrl { get; protected set; }
        public Uri ContentControlPageUrl { get; protected set; }
        public FailType MovieStatus { get; protected set; }

        /// <exception cref="System.Net.WebException" />
        /// <exception cref="System.InvalidOperationException" />
        public System.IO.Stream GetContentStream(int offset = 0, int length = int.MaxValue)
        {
            //動画の状態を調べ、落とせないなら例外発生
            if (MovieStatus != FailType.OK)
                switch (MovieStatus)
                {
                    case FailType.InvalidV1:
                        throw new InvalidOperationException("動画は存在しません。",
                            new NotExistContentException("動画は投稿者によって削除されています。", null, FailType.InvalidV1));
                    case FailType.InvalidV2:
                        throw new InvalidOperationException("動画は存在しません。",
                            new NotExistContentException("動画は投稿者によって非表示にされています。", null, FailType.InvalidV2));
                    case FailType.InvalidV3:
                        throw new InvalidOperationException("動画は存在しません。",
                            new NotExistContentException("動画は運営者によって削除されています。", null, FailType.InvalidV3));
                    default:
                        throw new InvalidOperationException("動画は存在しません。",
                            new NotExistContentException("動画情報を取得できません。", null, FailType.Unknown));
                }
            //動画が存在するならば落とす
            try
            {
                //再生ページへアクセスしないと動画を落とせない。
                using (var strmPg = PrimitiveLibrary.HttpConect(new Uri(String.Format("http://nicovideo.jp/watch/{0}", MovieId)), null, _cookie))
                {
                    //動画URLへ接続
                    var strmMvi = PrimitiveLibrary.HttpConect(ContentUrl, null, _cookie, offset, length);
                    var buffer = new System.IO.BufferedStream(strmMvi, 51200);
                    return buffer;
                }
            }
            catch (System.Net.WebException e)
            {
                throw new System.Net.WebException("動画サーバへの接続に失敗しました。", e);
            }
        }
        /// <exception cref="System.Net.WebException" />
        /// <exception cref="System.InvalidOperationException" />
        public NicoCommentContainer GetCommentContainer(int commeAmnt)
        {
            if (!IsFailed)
                throw new InvalidOperationException("コメントは削除されています。");
            else
            {
                var cmeXml = PrimitiveLibrary.GetMsg(ThreadId, commeAmnt, UserId, CommentUrl, _cookie);
                var container = NicoCommentContainer.Load(cmeXml);
                return container;
            }
        }
    }
    public class RelationContainer : IEnumerable<ContentInfo>
    {
        public RelationContainer(string movieId, SortKeys key, bool ascendingSort, CookieContainer cookie)
        {
            var src = PrimitiveLibrary.GetRelation(movieId, 0, key, ascendingSort);
            _movieId = movieId;
            _pageCnt = int.Parse(src.Element("page_count").Value);
            _cInfoCntOfPage = int.Parse(src.Element("data_count").Value);
            _infos = new ContentInfo[int.Parse(src.Element("total_count").Value)];
            _cookie = cookie;

            IsAscendingSort = ascendingSort;
            SortKey = key;

            SetInfo(0);
        }

        string _movieId;
        int _pageCnt;
        int _cInfoCntOfPage;
        CookieContainer _cookie;
        ContentInfo[] _infos;

        public bool IsAscendingSort { get; protected set; }
        public SortKeys SortKey { get; protected set; }
        public ContentInfo this[int i]
        {
            get
            {
                if (_infos[i] == null)
                    SetInfo(i / _cInfoCntOfPage);
                return _infos[i];
            }
        }

        void SetInfo(int pageNum)
        {
            var src = PrimitiveLibrary.GetRelation(_movieId, pageNum, SortKey, IsAscendingSort);
            var videos = src.Elements("video");
            var i = _cInfoCntOfPage * pageNum;
            foreach (var videoEle in videos.Select(e => e.Element("url").Value))
            {
                var match = System.Text.RegularExpressions.Regex.Match(videoEle, "^.*/([^/]+)$");
                var id = match.Groups[1].Value;
                _infos[i] = new ContentInfo(id, _cookie);
                i++;
            }
        }
        public IEnumerator<ContentInfo> GetEnumerator()
        {
            foreach (var info in _infos)
                yield return info;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _infos.GetEnumerator();
        }
    }
    public class TagInfo
    {
        public TagInfo(string text, bool isLock = false, string lang = "jp")
        {
            Text = text;
            IsLock = isLock;
            Lang = lang;
        }

        public string Text { get; protected set; }
        public bool IsLock { get; protected set; }
        public string Lang { get; protected set; }

        public override string ToString()
        {
            return Text;
        }
        public static implicit operator TagInfo(string s)
        {
            var tag = new TagInfo(s);
            return tag;
        }
        public static explicit operator string(TagInfo nt)
        {
            var str = nt.Text;
            return str;
        }
    }

    public enum StatusType { OK, Deleted }
    public enum FailType { OK, InvalidV1 = 1, InvalidV2 = 2, InvalidV3 = 3, AccessLocked, Unknown }
    public enum MovieType { Flv, MP4, SWF, }
    public enum SortKeys { Osusume, Comment, ViewCount, PostTime, MyListCount, }

    public class NotExistContentException : Exception
    {
        public NotExistContentException(string message = "", Exception innerException = null, FailType type = FailType.Unknown)
            : base(message, innerException) { Error = type; }
        public FailType Error { get; protected set; }
    }
}
