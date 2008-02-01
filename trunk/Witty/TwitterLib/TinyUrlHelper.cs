using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitterLib
{
    /// <summary>
    /// Wrapper for interacting with TinyUrl API
    /// </summary>
    public class TinyUrlHelper
    {
        public string ConvertUrlsToTinyUrls(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            string[] textSplitIntoWords = text.Split(' ');

            bool foundUrl = false;
            for (int i = 0; i < textSplitIntoWords.Length; i++)
            {
                if (IsUrl(textSplitIntoWords[i]))
                {
                    foundUrl = true;
                    // replace found url with tinyurl
                    textSplitIntoWords[i] = GetNewTinyUrl(textSplitIntoWords[i]);
                }
            }

            // reassemble if we found at least 1 url, otherwise return unaltered
            return foundUrl ? String.Join(" ", textSplitIntoWords) : text;
        }

        public string GetNewTinyUrl(string sourceUrl)
        {
            if (sourceUrl == null)
                throw new ArgumentNullException("sourceUrl");

            // fallback will be source url
            string result = sourceUrl;
            //Added 11/3/2007 scottckoon
            //20 is the shortest a tinyURl can be (http://tinyurl.com/a)
            //so if the sourceUrl is shorter than that, don't make a request to TinyURL
            if (sourceUrl.Length > 20 && !sourceUrl.Contains("http://tinyurl.com"))
            {
                // tinyurl doesn't like urls w/o protocols so we'll ensure we have at least http
                string requestUrl = BuildRequestUrl(EnsureMinimalProtocol(sourceUrl));
                WebRequest request = HttpWebRequest.Create(requestUrl);

                try
                {
                    using (Stream responseStream = request.GetResponse().GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.ASCII);
                        result = reader.ReadToEnd();
                    }
                }
                catch
                {
                    // eat it and return original url
                }
            }
            //scottckoon - It doesn't make sense to return a TinyURL that is longer than the original.
            if (result.Length > sourceUrl.Length) { result = sourceUrl; }
            return result;
        }

        private static string BuildRequestUrl(string sourceUrl)
        {
            const string tinyUrlFormat = "http://tinyurl.com/api-create.php?url={0}";
            return String.Format(tinyUrlFormat, sourceUrl);
        }

        // REFACTOR: DRY vs. StringUtils - didn't want this static
        private static bool IsUrl(string word)
        {
            const string urlRegex =
               @"\b(((https?)://)?[-\w]+(\.\w[-\w]*)+|\w+\@|mailto:|[a-z0-9](?:[-a-z0-9]*[a-z0-9])?\.)+(com\b|edu\b|biz\b|gov\b|in(?:t|fo)\b|mil\b|net\b|org\b|[a-z][a-z]\b)(:\d+)?(/[-a-z0-9_:\@&?=+,.!/~*'%\$]*)*(?<![.,?!])(?!((?!(?:<a )).)*?(?:</a>))(?!((?!(?:<!--)).)*?(?:-->))";

            bool isUrl;
            try
            {
                isUrl = Regex.IsMatch(word, urlRegex, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                // issue with the regular expression
                // TODO: not sure what overarching exhandling strategy is
                throw;
            }
            return isUrl;
        }

        private static string EnsureMinimalProtocol(string url)
        {
            // if our url doesn't have a protocol, we'll at least assume it's plain old http, otherwise good to go
            const string minimalProtocal = @"http://";
            if (url.ToLower().StartsWith("http"))
            {
                return url;
            }
            else
            {
                return minimalProtocal + url;
            }
        }
    }
}
