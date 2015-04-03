using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SavannahState.SocialMedia
{
    public class Utility
    {
        public static String UrlFinder(String rawText)
        {
            string output = "";
            if (!String.IsNullOrEmpty(rawText))
            {
                Regex linkParser = new Regex(@"\b((https?|ftp|file)://|(www|ftp)\.)[-A-Z0-9+&@#/%?=~_|$!:,.;\(\)]*[A-Z0-9+&@#/%=~_|$]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (linkParser.Matches(rawText).Count > 0)
                {
                    output = rawText;
                    foreach (Match m in linkParser.Matches(rawText))
                    {
                        output = output.Replace(m.Value, "<a target='_blank' href='" + m.Value + "'>" + m.Value + "</a>");
                    }
                    return output;
                }
                else
                {
                    return rawText;
                }
            }
            else
            {
                return rawText;
            }
        }


        public static DateTime ParseDateTime(String date) {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return DateTime.ParseExact(date, format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static DateTime UnixTimeStampToDateTime(String sUnixTimeStamp)
        {

            double unixTimeStamp = Convert.ToDouble(sUnixTimeStamp);

            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static Int32 DateTimeToUnixTimeStamp(DateTime TimeStamp)
        {
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (TimeStamp == DateTime.MinValue)
            {
                    return -1;
                }
            TimeSpan span = (TimeStamp - UnixEpoch);
            return (int)Math.Floor(span.TotalSeconds);
        }
        public static Boolean ImageExists(String imageURL) {
            String test = "";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(imageURL);
            request.Timeout = 500;
            request.Method = "HEAD";

            Boolean exists = false;
            try
            {
                request.GetResponse();
                exists = true;
            }
            catch
            {
                exists = false;
            }
            return exists;
        }

        public static String FormatPost(String content, String hashtagURL)
        {
            if (!String.IsNullOrEmpty(content))
            {
                var regex = new Regex(@"(?<=#)\w+");
                var matches = regex.Matches(content);

                foreach (Match m in matches)
                {
                    content = content.Replace("#" + m.Value, "<a target='_blank' href='" + hashtagURL + m.Value + "'>#" + m.Value + "</a>");
                }
            }
            return content;
        }
    }
}
