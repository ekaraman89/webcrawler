using System;
using System.Collections.Generic;
using System.Text;

namespace webcrawler
{
    static class StopwordTool
    {
        /// <summary>
        /// Words we want to remove.
        /// </summary>
        static Dictionary<string, bool> _stops = new Dictionary<string, bool>
        {
           {"a", true },{"acaba", true },{"altı", true },{"altmış", true },{"ama", true },{"ancak", true },{"arada", true },{"artık", true },{"asla", true },{"aslında", true },{"ayrıca", true },{"az", true },{"bana", true },{"bazen", true },{"bazı", true },{"bazıları", true },{"belki", true },{"ben", true },{"benden", true },{"beni", true },{"benim", true },{"beri", true },{"beş", true },{"bile", true },{"bilhassa", true },{"bin", true },{"bir", true },{"biraz", true },{"birçoğu", true },{"birçok", true },{"biri", true },{"birisi", true },{"birkaç", true },{"birşey", true },{"biz", true },{"bizden", true },{"bize", true },{"bizi", true },{"bizim", true },{"böyle", true },{"böylece", true },{"bu", true },{"buna", true },{"bunda", true },{"bundan", true },{"bunlar", true },{"bunları", true },{"bunların", true },{"bunu", true },{"bunun", true },{"burada", true },{"bütün", true },{"çoğu", true },{"çoğunu", true },{"çok", true },{"çünkü", true },{"da", true },{"daha", true },{"dahi", true },{"dan", true },{"de", true },{"defa", true },{"değil", true },{"diğer", true },{"diğeri", true },{"diğerleri", true },{"diye", true },{"doksan", true },{"dokuz", true },{"dolayı", true },{"dolayısıyla", true },{"dört", true },{"e", true },{"edecek", true },{"eden", true },{"ederek", true },{"edilecek", true },{"ediliyor", true },{"edilmesi", true },{"ediyor", true },{"eğer", true },{"elbette", true },{"elli", true },{"en", true },{"etmesi", true },{"etti", true },{"ettiği", true },{"ettiğini", true },{"fakat", true },{"falan", true },{"filan", true },{"gene", true },{"gereği", true },{"gerek", true },{"gibi", true },{"göre", true },{"hala", true },{"halde", true },{"halen", true },{"hangi", true },{"hangisi", true },{"hani", true },{"hatta", true },{"hem", true },{"henüz", true },{"hep", true },{"hepsi", true },{"her", true },{"herhangi", true },{"herkes", true },{"herkese", true },{"herkesi", true },{"herkesin", true },{"hiç", true },{"hiçbir", true },{"hiçbiri", true },{"i", true },{"ı", true },{"için", true },{"içinde", true },{"iki", true },{"ile", true },{"ilgili", true },{"ise", true },{"işte", true },{"itibaren", true },{"itibariyle", true },{"kaç", true },{"kadar", true },{"karşın", true },{"kendi", true },{"kendilerine", true },{"kendine", true },{"kendini", true },{"kendisi", true },{"kendisine", true },{"kendisini", true },{"kez", true },{"ki", true },{"kim", true },{"kime", true },{"kimi", true },{"kimin", true },{"kimisi", true },{"kimse", true },{"kırk", true },{"madem", true },{"mi", true },{"mı", true },{"milyar", true },{"milyon", true },{"mu", true },{"mü", true },{"nasıl", true },{"ne", true },{"neden", true },{"nedenle", true },{"nerde", true },{"nerede", true },{"nereye", true },{"neyse", true },{"niçin", true },{"nin", true },{"nın", true },{"niye", true },{"nun", true },{"nün", true },{"o", true },{"öbür", true },{"olan", true },{"olarak", true },{"oldu", true },{"olduğu", true },{"olduğunu", true },{"olduklarını", true },{"olmadı", true },{"olmadığı", true },{"olmak", true },{"olması", true },{"olmayan", true },{"olmaz", true },{"olsa", true },{"olsun", true },{"olup", true },{"olur", true },{"olursa", true },{"oluyor", true },{"on", true },{"ön", true },{"ona", true },{"önce", true },{"ondan", true },{"onlar", true },{"onlara", true },{"onlardan", true },{"onları", true },{"onların", true },{"onu", true },{"onun", true },{"orada", true },{"öte", true },{"ötürü", true },{"otuz", true },{"öyle", true },{"oysa", true },{"pek", true },{"rağmen", true },{"sana", true },{"sanki", true },{"şayet", true },{"şekilde", true },{"sekiz", true },{"seksen", true },{"sen", true },{"senden", true },{"seni", true },{"senin", true },{"şey", true },{"şeyden", true },{"şeye", true },{"şeyi", true },{"şeyler", true },{"şimdi", true },{"siz", true },{"sizden", true },{"size", true },{"sizi", true },{"sizin", true },{"sonra", true },{"şöyle", true },{"şu", true },{"şuna", true },{"şunları", true },{"şunu", true },{"ta", true },{"tabii", true },{"tam", true },{"tamam", true },{"tamamen", true },{"tarafından", true },{"trilyon", true },{"tüm", true },{"tümü", true },{"u", true },{"ü", true },{"üç", true },{"un", true },{"ün", true },{"üzere", true },{"var", true },{"vardı", true },{"ve", true },{"veya", true },{"ya", true },{"yani", true },{"yapacak", true },{"yapılan", true },{"yapılması", true },{"yapıyor", true },{"yapmak", true },{"yaptı", true },{"yaptığı", true },{"yaptığını", true },{"yaptıkları", true },{"ye", true },{"yedi", true },{"yerine", true },{"yetmiş", true },{"yi", true },{"yı", true },{"yine", true },{"yirmi", true },{"yoksa", true },{"yu", true },{"yüz", true },{"zaten", true },{"zira", true }
        };

        /// <summary>
        /// Chars that separate words.
        /// </summary>
        static char[] _delimiters = new char[] { ' ', ',', ';', '.' };

        /// <summary>
        /// Remove stopwords from string.
        /// </summary>
        public static string RemoveStopwords(string input)
        {
            var words = input.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);

            var found = new Dictionary<string, bool>();

            StringBuilder builder = new StringBuilder();

            foreach (string currentWord in words)
            {
                string lowerWord = currentWord.ToLower();

                if (!_stops.ContainsKey(lowerWord) && !found.ContainsKey(lowerWord))
                {
                    builder.Append(currentWord).Append(' ');
                    found.Add(lowerWord, true);
                }
            }
            return builder.ToString().Trim();
        }
    }
}