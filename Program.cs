using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace webcrawler
{
    class Program
    {
        static readonly string[] categories = { "SİYASET", "GÜNDEM", "EKONOMİ", "DÜNYA" };
        static void Main(string[] args)
        {
            //GetHurriyetNews(); 
            string path = AppDomain.CurrentDomain.BaseDirectory + "links.txt";
            if (!File.Exists(path))
                File.WriteAllText(path, String.Empty);

            string[] articles = File.ReadAllLines(path);
            File.WriteAllText(path, String.Empty);
            DateTime LastWriteTime = File.GetLastWriteTime(path);
            Console.WriteLine("Linkler okundu işlem başlatılıyor...");

            while (true)
            {
                toWork(articles);

                Thread.Sleep(2000);
                articles = new string[] { };
                if (LastWriteTime != File.GetLastWriteTime(path))
                {
                    Console.WriteLine("Yeni linkler algılandı...");
                    articles = File.ReadAllLines(path);
                    File.WriteAllText(path, String.Empty);
                    LastWriteTime = File.GetLastWriteTime(path);
                }
            }
        }

        private static void toWork(string[] articles)
        {
            int count = articles.Length;
            if (count > 0)
            {
                using (var progress = new ProgressBar())
                {
                    for (int i = 0; i < count; i++)
                    {
                        string text = null;
                        try
                        {
                            text = GetResponse(articles[i].Trim());
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            //string category = GetCategory(text);
                            text = GetPlainTextFromHtml(text);

                            if (articles[i].IndexOf("milliyet.com.tr") != -1)
                            {
                                text = Crop(text, "Tüm Yazıları", "Yazarın Diğer Yazıları");
                            }
                            else if (articles[i].IndexOf("hurriyet.com.tr") != -1)
                            {
                                text = Crop(text, "Yorum yaz", "PAYLAŞ");
                            }
                            else if (articles[i].IndexOf("sozcu.com.tr") != -1)
                            {
                                text = Crop(text, "more", "social-facebook");
                            }
                            else if (articles[i].IndexOf("haberturk.com") != -1)
                            {
                                text = Crop(text, "    \r\n", "Değerli Haberturk.com okurları");
                            }
                            else if (articles[i].IndexOf("sabah.com.tr") != -1)
                            {
                                text = Crop(text, "Yükleniyor...", "\r\n            Yasal Uyarı:");
                            }
                            WriteToFile(text, "?");
                        }

                        progress.Report((double)i / 100);
                        Thread.Sleep(20);
                    }
                }
                Console.WriteLine("Bütün linkler başarıyla işletildi.\n\n\n\n");
            }
        }

        private static string GetCategory(string Text)
        {
            string catRegex = "<div class=\"dtyTop\"><div class=\"dTTabs\"><div class=\"kat\"><a href=\"/.*";
            string Category = "?";
            foreach (Match item in Regex.Matches(Text, catRegex))
            {
                Category = Crop(item.Value, "/\">", "</a></div></div></div>");
            }
            return Category;
        }

        private static string Crop(string text, string topCrop, string botomCrop)
        {
            int index = text.IndexOf(topCrop);
            if (index > 0)
                text = text.Remove(0, index + topCrop.Length);

            index = text.IndexOf(botomCrop);
            if (index > 0)
                text = text.Remove(index).Trim();

            return text;
        }

        private static string GetResponse(string url)
        {
            WebRequest webRequest = HttpWebRequest.Create(url);
            WebResponse webResponse = webRequest.GetResponse();

            using (Stream stream = webResponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }

        }

        private static string GetPlainTextFromHtml(string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace(htmlString, string.Empty);
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace("&nbsp;", string.Empty);

            return htmlString;
        }

        private static void WriteToFile(string Text, string Category)
        {

            string path = "Files";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string description = "Makale kategorisi bulma";
            Text = Text.Replace("\r", "").Replace("\n","");
            CreateFile("Original.txt", Text);
            CreateArffFile(description, path, "Original", Text, Category);
            CreateArffFile(description, path, "WithoutStopWordOriginal", StopwordTool.RemoveStopwords(Text), Category);
        }

        private static void CreateArffFile(string Description, string Path, string FileName, string TextData, string Category)
        {
            Arff arff = new Arff(Description, Path, FileName);
            arff.AddAttribute("Text", Arff.ArffType.String);
            arff.AddAttribute(categories);
            arff.AddData(new string[] { TextData, Category });
        }

        private static void CreateFile(string FileName, string Text)
        {
            using (StreamWriter sw = new StreamWriter(FileName, true, System.Text.Encoding.UTF8))
            {
                sw.WriteLine(Text);
            }
        }
    }
}
