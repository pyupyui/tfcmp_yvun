﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tfcmp_yvun //youtube video (and community) upload notification
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            if (Environment.Is64BitOperatingSystem) // 운영체제 종류 확인 (64비트)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION", Application.ProductName + ".exe", 11001);
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION", Application.ProductName + ".vshost.exe", 11001);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", Application.ProductName + ".exe", 11001);
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", Application.ProductName + ".vshost.exe", 11001);
            }
            */
            webBrowser1.Navigate("https://www.naver.com");
            //webBrowser1.Navigate("https://cafe.naver.com/otyutest1");

        }

        void main()
        {
            yvun_main();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 1000 * 10)
            {
                Application.DoEvents();
            }
            sw.Stop();

            ycun_main();

        }

        void yvun_main()
        {
            webBrowser1.Navigate("https://www.youtube.com/channel/UCTSaxXnhUcrhv984bVpDr6Q/videos");

            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            string video_data_raw = Regex.Match(webBrowser1.DocumentText, "(?<=<h3 class=\"yt-lockup-title \">)(.*?)(?=</span></h3>)", RegexOptions.Singleline).Value;
            string link = Regex.Match(video_data_raw, "(?<=href=\"/watch\\?v\\=)(.*?)(?=\")").Value;
            string title = Regex.Match(video_data_raw, "(?<=rel=\"nofollow\">)(.*?)(?=</a>)").Value;

            Video_Data web_video_data = new Video_Data(link, title);
            string file_video_data_link = File.ReadAllText("video_link.txt");
            List<string> file_video_data_link_list = new List<string>(Regex.Split(file_video_data_link, "\r\n"));

            if (file_video_data_link_list.Contains(web_video_data.link) == false)
            {
                upload_cafe_article(web_video_data);
                File.AppendAllText("video_link.txt", "\r\n" + web_video_data.link);
            }
            else
            {

            }
        }

        void ycun_main()
        {
            webBrowser1.Navigate("https://www.youtube.com/channel/UCTSaxXnhUcrhv984bVpDr6Q/community");

            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            MatchCollection mc = Regex.Matches(webBrowser1.DocumentText, "(?<=tabindex=\"0\"><a href=\"\\/post\\/)(.*?)(?=\")", RegexOptions.Singleline);
            Console.WriteLine(mc[6]);

            string web_community_link = Regex.Match(webBrowser1.DocumentText, "(?<=tabindex=\"0\"><a href=\"\\/post\\/)(.*?)(?=\")", RegexOptions.Singleline).Value;
            string file_community_link = File.ReadAllText("community_link.txt");
            List<string> file_community_link_list = new List<string>(Regex.Split(file_community_link, "\r\n"));

            if (file_community_link_list.Contains(web_community_link) == false)
            {
                upload_cafe_article_community(web_community_link);
                File.AppendAllText("community_link.txt", "\r\n" + web_community_link);
            }
            else
            {

            }
        }

        void upload_cafe_article(Video_Data video_data)
        {
            //mobile naver cafe write page
            webBrowser1.Navigate("https://m.cafe.naver.com/ArticleWrite.nhn?m=write&clubid=29846417&menuid=");

            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            //article category select //selectedIndex = 4
            webBrowser1.Document.GetElementsByTagName("select")[0].SetAttribute("selectedIndex", "4");
            webBrowser1.Document.GetElementById("subject").SetAttribute("value", "[유튜브 영상] " + video_data.title);

            webBrowser1.Document.Window.Frames["frame"].Document.Body.InnerHtml =
                "<iframe width=\"560\" height=\"315\" src=\"" +
                "https://www.youtube.com/embed/" + video_data.link +
                "\" frameborder=\"0\" allow=\"accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture\" allowfullscreen></iframe>";

            HtmlElementCollection hec = webBrowser1.Document.GetElementsByTagName("a");

            foreach (HtmlElement a in hec)
            {
                if (a.InnerText == "등록")
                {
                    a.InvokeMember("click");
                    break;
                }
            }
        }
        void upload_cafe_article_community(string web_community_link)
        {
            //desktop naver cafe main page
            webBrowser1.Navigate("https://cafe.naver.com/yellowticket");

            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            //move to article write page
            HtmlElementCollection hec = webBrowser1.Document.GetElementsByTagName("a");
            foreach (HtmlElement he in hec)
            {
                if (he.InnerText == "카페 글쓰기")
                {
                    he.InvokeMember("click");
                    break;
                }
            }

            //wait article write page load
            while (true)
            {
                try
                {
                    string buffer = webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementById("frmWrite").InnerText;
                    break;
                }
                catch
                {

                }
                Application.DoEvents();
            }

            //write article //selectedIndex = 4
            webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementsByTagName("select")[0].SetAttribute("selectedIndex", "4");
            webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementById("subject").SetAttribute("value", "[유튜브 커뮤니티] " +
                DateTime.Now.Year + "년 " +
                DateTime.Now.Month + "월 " +
                DateTime.Now.Day + "일 " +
                DateTime.Now.Hour + "시 " +
                DateTime.Now.Minute + "분 " +
                "알림");

            HtmlElementCollection hec2 = webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementsByTagName("a");
            foreach (HtmlElement he in hec2)
            {
                if (he.InnerText == "링크")
                {
                    he.InvokeMember("click");
                    break;
                }
            }

            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            while (sw2.ElapsedMilliseconds < 3000)
            {
                Application.DoEvents();
            }
            sw2.Stop();

            while (true)
            {
                try
                {
                    webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementById("attachLinkUrl").Focus();
                    break;
                }
                catch
                {

                }
                Application.DoEvents();
            }

            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            while (sw3.ElapsedMilliseconds < 2000)
            {
                Application.DoEvents();
            }
            sw3.Stop();

            webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementById("attachLinkUrl").SetAttribute("value", "https://www.youtube.com/channel/UCTSaxXnhUcrhv984bVpDr6Q/community?lb=" + web_community_link);

            HtmlElementCollection hec3 = webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementsByTagName("button");
            foreach (HtmlElement he in hec3)
            {
                if (he.InnerText == "미리보기")
                {
                    he.InvokeMember("click");
                    break;
                }
            }

            while (Regex.Match(webBrowser1.Document.Window.Frames["cafe_main"].Document.Body.InnerHtml, "(?<=<div class=\"se2_og_content\" id=\"attachLinkPreview\" style=\"display: )(.*?)(?=;\">)", RegexOptions.Singleline).Value != "block")
            {
                Application.DoEvents();
            }

            HtmlElementCollection hec4 = webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementsByTagName("button");
            foreach (HtmlElement he in hec4)
            {
                if (he.InnerText == "적용")
                {
                    he.InvokeMember("click");
                    break;
                }
            }

            webBrowser1.Document.Window.Frames["cafe_main"].Document.GetElementById("cafewritebtn").InvokeMember("click");
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                timer1.Start();
                label1.Text = "RUN";
                label1.BackColor = Color.FromArgb(128, 192, 255);
                main();
            }
            else
            {
                timer1.Stop();
                label1.Text = "STOP";
                label1.BackColor = Color.FromArgb(255, 128, 128);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            main();
        }
    }

    public class Video_Data
    {
        public string link;
        public string title;

        public Video_Data(string link, string title)
        {
            this.link = link;
            this.title = title;
        }
    }
}