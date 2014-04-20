using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SunokoLibrary.Nicovideo;

namespace NicovideoLibrarySample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var cookie = new System.Net.CookieContainer();
            SunokoLibrary.Nicovideo.Net.PrimitiveLibrary.Login("userid", "pass", cookie);
            var cInfo = new ContentInfo("sm9", cookie);
            var comme = cInfo.MovieInfo.GetCommentContainer(-10);
        }
    }
}
