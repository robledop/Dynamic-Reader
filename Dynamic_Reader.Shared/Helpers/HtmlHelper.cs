using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Dynamic_Reader.Helpers
{
    public static class HtmlHelper
    {
        private const string HTML_TAG_PATTERN = "<.*?>";

        public static string StripHtml(string chapterHtmlContent, string htmlPosition = null)
        {
            if (chapterHtmlContent == null) throw new ArgumentNullException("chapterHtmlContent");

            chapterHtmlContent = Regex.Replace(chapterHtmlContent, @"\s+", " ").Trim();

            if (htmlPosition != null)
            {
                var regex = new Regex(htmlPosition);
                var match = regex.Match(chapterHtmlContent);

                if (match.Success)
                {
                    chapterHtmlContent = chapterHtmlContent.Substring(match.Index);
                    chapterHtmlContent = chapterHtmlContent.Insert(0, "<");
                }
            }

            chapterHtmlContent = Regex.Replace(chapterHtmlContent, "<a.*?</a>", " ");
            chapterHtmlContent = Regex.Replace(chapterHtmlContent, "<xmp>.*?</xmp>", " ");
            chapterHtmlContent = Regex.Replace(chapterHtmlContent, "<style.*?</style>", " ");
            chapterHtmlContent = Regex.Replace(chapterHtmlContent, "<title.*?</title>", " ");
            chapterHtmlContent = Regex.Replace
                (chapterHtmlContent, HTML_TAG_PATTERN, " ");

            chapterHtmlContent = Regex.Replace(chapterHtmlContent, @"\s+", " ").Trim();

            return WebUtility.HtmlDecode(chapterHtmlContent);
        }
    }
}