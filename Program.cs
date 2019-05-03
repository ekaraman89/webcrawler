using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace webcrawler
{
    class Program
    {
        static readonly string[] categories = { "SIYASET", "GUNDEM", "EKONOMI", "DUNYA" };
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
                            text = ConvertToTurkish(GetPlainTextFromHtml(text));

                            if (articles[i].IndexOf("milliyet.com.tr") != -1)
                            {
                                text = Crop(text, "Tüm Yazıları", "Yazarın Diğer Yazıları");
                            }
                            else if (articles[i].IndexOf("hurriyet.com.tr") != -1)
                            {
                                //spor
                                if (articles[i].IndexOf("http://www.hurriyet.com.tr/sporarena/") != -1)
                                {
                                    string mail = GetMailAdress(text);
                                    if (!string.IsNullOrWhiteSpace(mail))
                                    {
                                        text = Crop(text, mail, "PAYLAŞ");
                                        text = Crop(text, mail, mail);
                                    }
                                    else
                                    {
                                        string auther = string.Empty;
                                        int index = text.IndexOf("MENÜHÜRRİYET.COM.TR");
                                        while (text[--index] != '-')
                                        {
                                            auther = auther.Insert(0, text[index].ToString());
                                        }
                                        text = Crop(text, $"Tipi{auther.Trim()} ", $"{auther.Trim()} YazdırAYazı");
                                    }
                                }
                                else
                                {
                                    text = Crop(text, "Yorum yazA", "PAYLAŞ");
                                }
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
            Text = Regex.Replace(Text, @"\t|\n|\r", "");
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

        private static string GetMailAdress(string text)
        {
            const string MatchEmailPattern =
           @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
           + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
           + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})";
            Regex rx = new Regex(MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(text);
            int noOfMatches = matches.Count;
            String mailadress = string.Empty;
            foreach (Match match in matches)
            {
                mailadress = match.Value.ToString();
                return (mailadress[mailadress.Length - 1] == 'Y') ? mailadress.Remove(mailadress.Length - 1) : mailadress.Remove(mailadress.Length - 2);
            }
            return mailadress;
        }

        private static String ConvertToTurkish(string text)
        {
            text = text.Replace("&#252;", "ü")
                        .Replace("&#220;", "Ü")
                        .Replace("&#199;", "Ç")
                        .Replace("&#231;", "ç")
                        .Replace("&#246;", "ö")
                        .Replace("&#214;", "Ö")
                        .Replace("&#286;", "Ğ")
                        .Replace("&#287;", "ğ")
                        .Replace("&#304;", "İ")
                        .Replace("&#305;", "ı")
                        .Replace("&#351;", "ş")
                        .Replace("&#350;", "Ş");

            return text;
        }

    }
}
