using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for second.xaml
    /// </summary>
    /// 
    
    public partial class second : Window, IDisposable
    {
        //public String videoName;

        MainWindow main;

        public second(MainWindow mainRef)
        {
            InitializeComponent();
            main = mainRef;

        }

        public void Dispose()
        {
            GC.Collect();
        }

        public string GetYouTubeVideoPlayerHTML(string videoCode)
        {
            var sb = new StringBuilder();

            const string YOUTUBE_URL = @"http://www.youtube.com/v/";


            sb.Append("<html>");
            sb.Append("    <head>");
            sb.Append("        <meta name=\"viewport\" content=\"width=device-width; height=device-height;\">");
            sb.Append("    </head>");
            sb.Append("    <body marginheight=\"0\" marginwidth=\"0\" leftmargin=\"0\" topmargin=\"0\" style=\"overflow-y: hidden\">");
            sb.Append("        <object width=\"100%\" height=\"100%\">");
            sb.Append("            <param name=\"movie\" value=\"" + YOUTUBE_URL + videoCode + "?version=3&amp;rel=0\" />");
            sb.Append("            <param name=\"allowFullScreen\" value=\"true\" />");
            sb.Append("            <param name=\"allowscriptaccess\" value=\"always\" />");
            sb.Append("            <embed src=\"" + YOUTUBE_URL + videoCode +"?autoplay=1&?version=3&amp;rel=0\" type=\"application/x-shockwave-flash\"");
            sb.Append("                   width=\"100%\" height=\"100%\" allowscriptaccess=\"always\" allowfullscreen=\"true\" />");
            sb.Append("        </object>");
            sb.Append("    </body>");
            sb.Append("</html>");


           // Console.WriteLine(sb);
            return sb.ToString();
        }

        public void ShowYouTubeVideo(string videoCode)
        {
            this.WebBrowser1.NavigateToString(GetYouTubeVideoPlayerHTML(videoCode));
        }

        private void Skip_F_Btn_Click(object sender, RoutedEventArgs e)
        {
            main.skipForward();
        }

        private void Skip_B_Btn_Click(object sender, RoutedEventArgs e)
        {
            main.skipBack();
        }
    }
}
