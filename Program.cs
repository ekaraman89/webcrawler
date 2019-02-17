using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace webcrawler
{
    class Program
    {
        static void Main(string[] args)
        {

            string path = AppDomain.CurrentDomain.BaseDirectory+"links.txt";
            if (!File.Exists(path))
            {
                 File.WriteAllText(path, String.Empty);
            }
            string[] articles = File.ReadAllLines(path);
            File.WriteAllText(path, String.Empty);
            DateTime LastWriteTime = File.GetLastWriteTime(path);
            Console.WriteLine("Linkler okundu işlem başlatılıyor...");
            bool flag = false;
            while (true)
            {
                foreach (var item in articles)
                {
                    flag = true;
                    string text = null;
                    try
                    {
                        text = GetResponse(item.Trim());
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        string category = GetCategory(text);

                        text = GetPlainTextFromHtml(text);
                        text = Crop(text, "Tüm Yazıları", "Yazarın Diğer Yazıları");
                        WriteToFile(text, category);
                    }
                }
                if (flag)
                {
                    Console.WriteLine("Bütün linkler başarıyla işletildi.");
                    flag=false;
                }
                else
                {
                    GC.Collect();
                }
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

        private static string GetCategory(string Text)
        {
            string catRegex = "<div class=\"dtyTop\"><div class=\"dTTabs\"><div class=\"kat\"><a href=\"/.*";
            string Category = "Diğer";
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
            StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());

            return streamReader.ReadToEnd();
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

            CreateFile("Original.txt", Text);
            CreateArffFile(description, path, "Original", Text, Category);
            CreateArffFile(description, path, "WithoutStopWordOriginal", StopwordTool.RemoveStopwords(Text), Category);
        }

        private static void CreateArffFile(string Description, string Path, string FileName, string TextData, string Category)
        {
            Arff arff = new Arff(Description, Path, FileName);
            arff.AddAttribute("Text", Arff.ArffType.String);
            arff.AddAttribute(new string[] { "SİYASET", "GÜNDEM", "EKONOMİ", "DÜNYA" });
            arff.AddData(new string[] { TextData, Category });
        }

        private static void CreateFile(string FileName, string Text)
        {
            using (StreamWriter sw = new StreamWriter(FileName, true))
            {
                sw.WriteLine(Text);
            }
        }


    }
}
