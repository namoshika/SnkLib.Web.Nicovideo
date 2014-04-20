using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SunokoLibrary.Nicovideo.Net
{
    public class PrimitiveLibrary
    {
        static string _nicoUrl = "http://nicovideo.jp";
        static string _loginUrl = "https://secure.nicovideo.jp/secure/login?site=niconico";
        static string _getFlvUrl = "http://www.nicovideo.jp/api/getflv/{0}";
        static string _getThumbInfoUrl = "http://www.nicovideo.jp/api/getthumbinfo/{0}";
        static string _getRelationUrl = "http://www.nicovideo.jp/api/getrelation?page={0}&sort={1}&order={2}&video={3}";

        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.Net.WebException" />
        public static bool Login(string mailAddress, string password, CookieContainer cookie)
        {
            if (mailAddress == null)
                throw new ArgumentNullException("mailAddress", "nullにする事は出来ません。");
            if(password == null)
                throw new ArgumentNullException("password", "nullにする事は出来ません。");

            try
            {
                var postMsg = String.Format("mail={0}&password={1}", mailAddress, password);
                using (var strm = HttpConect(new Uri(_loginUrl), postMsg, cookie))
                {
                    //何もしない
                }
            }
            catch (WebException e)
            {
                throw new System.Net.WebException("ログインサーバへアクセス出来ませんでした。", e);
            }

            var cc = cookie.GetCookies(new Uri(_nicoUrl));
            var succ = cc["user_session"] != null;
            return succ;
        }
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.Net.WebException" />
        public static System.Xml.Linq.XDocument GetMsg(int threadId, int commeAmount, int userId, Uri commeServer, System.Net.CookieContainer cookie)
        {
            if(commeServer == null)
                throw new ArgumentNullException("commeServer", "nullにする事は出来ません。");

            var reqXml = new System.Xml.Linq.XElement(
                "thread",
                //new System.Xml.Linq.XAttribute("no_compress", 0),
                new System.Xml.Linq.XAttribute("fork", "0"),//投票者コメント:1
                new System.Xml.Linq.XAttribute("user_id", userId),
                //new System.Xml.Linq.XAttribute("when", 0),
                new System.Xml.Linq.XAttribute("res_from", commeAmount),
                new System.Xml.Linq.XAttribute("version", "20061206"),
                new System.Xml.Linq.XAttribute("thread", threadId));
            var reqMsg = reqXml.ToString();

            try
            {
                using (var strm = HttpConect(commeServer, reqMsg, cookie))
                using (var reader = new System.IO.StreamReader(strm))
                    return System.Xml.Linq.XDocument.Load(reader);
            }
            catch (System.Net.WebException e)
            {
                throw new System.Net.WebException("コメントサーバへの接続に失敗しました。", e);
            }
        }
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentOutOfRangeException" />
        /// <exception cref="System.Net.WebException" />
        public static System.Xml.Linq.XElement GetRelation(string movieId, int pageNum, SortKeys key, bool ascendingSort)
        {
            if (movieId == null)
                throw new ArgumentNullException("movieId", "nullにする事は出来ません。");
            if (pageNum < 0)
                throw new ArgumentOutOfRangeException("pageNum", "0未満にする事は出来ません。");

            string typeChar;
            switch (key)
            {
                case SortKeys.Comment: typeChar = "r"; break;
                case SortKeys.ViewCount: typeChar = "v"; break;
                case SortKeys.PostTime: typeChar = "f"; break;
                case SortKeys.MyListCount: typeChar = "m"; break;
                default: typeChar = "p"; break;
            }
            var url = new Uri(String.Format(_getRelationUrl, pageNum, typeChar, ascendingSort ? "a" : "d", movieId));
            try
            {
                using (var strm = HttpConect(url, null, null))
                using (var reader = new System.IO.StreamReader(strm))
                {
                    var doc = System.Xml.Linq.XElement.Load(reader);
                    return doc;
                }
            }
            catch (WebException e)
            {
                throw new WebException("GetRelation_APIへアクセス出来ませんでした。", e);
            }
        }
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.Net.WebException" />
        public static System.Xml.Linq.XElement GetThumbInfo(string movieId)
        {
            if (movieId == null)
                throw new ArgumentNullException("movieId", "nullにする事は出来ません。");

            try
            {
                var url = new Uri(String.Format(_getThumbInfoUrl, movieId));
                using(var strm = HttpConect(url, null, null))
                using (var reader = new System.IO.StreamReader(strm))
                {
                    var doc = System.Xml.Linq.XElement.Load(reader);
                    return doc;
                }
            }
            catch (WebException e)
            {
                throw new WebException("GetThumbInfo_APIへアクセス出来ませんでした。", e);
            }

        }
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.Net.WebException" />
        public static Dictionary<string, string> GetFlv(string movieId, CookieContainer cookie)
        {
            if (movieId == null)
                throw new ArgumentNullException("movieId", "この引数はnullを入れることはできません。");

            String[] resultPair;
            try
            {
                var url = new Uri(String.Format(_getFlvUrl, movieId));
                using (var strm = HttpConect(url, null, cookie))
                {
                    var reader = new System.IO.StreamReader(strm);
                    var resStr = reader.ReadToEnd();
                    resultPair = resStr.Split('&');
                }
            }
            catch (System.IO.IOException e)
            {
                throw new WebException("getFlvApiのレスポンス受信時にエラーが発生しました。", e);
            }

            var keyVals = new Dictionary<string, string>();
            foreach (var pair in resultPair)
            {
                var keyVal = pair.Split('=');
                if (keyVal.Length == 2)
                {
                    var key = System.Web.HttpUtility.UrlDecode(keyVal[0]);
                    var val = System.Web.HttpUtility.UrlDecode(keyVal[1]);
                    keyVals[key] = val;
                }
            }
            return keyVals;
        }
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Net.WebException" />
        /// <returns></returns>
        public static System.IO.Stream HttpConect(Uri url, string postMsg, CookieContainer cookie,
            int offset = 0, int length = int.MaxValue)
        {
            if (url == null)
                throw new ArgumentNullException("引数urlはnullでは使えません");
            
            var req = HttpWebRequest.Create(url) as HttpWebRequest;
            req.CookieContainer = cookie;
            if (offset > 0)
            {
                try
                {
                    if (length == int.MaxValue)
                        req.AddRange(offset);
                    else if (length > -1)
                        req.AddRange(offset, length);
                    else
                        throw new ArgumentOutOfRangeException("length", "0以上にしてください");
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("引数offset、lengthの値に異常があります。", e);
                }
            }
            if (postMsg != null)
            {
                var bytes = Encoding.ASCII.GetBytes(postMsg);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bytes.Length;

                using (var reqStrm = req.GetRequestStream())
                    reqStrm.Write(bytes, 0, bytes.Length);
            }

            var res = req.GetResponse();
            return res.GetResponseStream();
        }
    }
}
