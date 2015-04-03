using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace SavannahState.SocialMedia
{
    public class InstagramRepository:IFeedRepository
    {
        private String _accessToken = "";
        private const String _queryURL = "https://api.instagram.com/v1/users/455868730/media/recent/";
        private Int16 _queryLimit;
        private Int32 _daySpan;
        private List<Post> _allPosts;
        private String _nextUrl;
        private DateTime _min_timestamp;
        private DateTime _max_timestamp;
        private String _feedParamsProvided = "";

        public InstagramRepository() {
            _allPosts = new List<Post>();
        }

        public Result GetFeed(String accessToken, String feedParameters, Int16 maxResults, Int16 numberDays, String accessTokenSecret, String consumerKey, String consumerSecret, String userId)
        {
            if (maxResults > 0)
            {
                _queryLimit = maxResults;
            }
            else
            {
                _queryLimit = 1;
            }

            if (numberDays > 0)
            {
                _daySpan = (60 * 60 * 24 * numberDays * -1);
            }
            else
            {
                _daySpan = -86400;
            }

            DateTime today = DateTime.Now;
            DateTime pastDate = DateTime.Now.AddDays(-1*numberDays);
            _max_timestamp = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
            _min_timestamp = new DateTime(pastDate.Year, pastDate.Month, pastDate.Day, 23, 59, 59);

            if (!String.IsNullOrEmpty(accessToken))
            {
                _accessToken = accessToken;
                if (_allPosts == null)
                {
                    _allPosts = new List<Post>();
                }
                else
                {
                    _allPosts.Clear();
                }
                String feedURL = _queryURL + "?access_token=" + _accessToken + "&count=" + _queryLimit;
                if (!String.IsNullOrEmpty(feedParameters))
                {
                    _feedParamsProvided = feedParameters;
                    feedURL += "&" + feedParameters;
                }
                else
                {
                    feedURL += "&min_timestamp=" + Utility.DateTimeToUnixTimeStamp(_min_timestamp) + "&max_timestamp=" + Utility.DateTimeToUnixTimeStamp(_max_timestamp);
                }
                return MakeAPIRequest(feedURL);
            }
            else
            {
                throw new Exception("No access token provided.");
            }
        }

        private Result MakeAPIRequest(String apiUrl)
        {
            
            String feed = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    feed = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                throw new Exception("Call to retrieve Instagram posts failed. URL :" + apiUrl + ". Message: " + ex.Message);
            }
            ProcessFeed(feed);

            Result result = new Result();
            if (!String.IsNullOrEmpty(_nextUrl))
            {
                result.NextUrl = ProcessNextURL(_nextUrl);
            } else {
                if(!String.IsNullOrEmpty(_feedParamsProvided)){
                    result.NextUrl = ProcessNextURL(_feedParamsProvided);
                }else{
                    result.NextUrl = "min_timestamp=" + (Utility.DateTimeToUnixTimeStamp(_min_timestamp) + _daySpan) + "&max_timestamp=" + (Utility.DateTimeToUnixTimeStamp(_max_timestamp)+_daySpan);
                }
            }
            result.posts = _allPosts;
            return result;
        }

        private String ProcessDateRange(String rawDateRange)
        {
            String output = "";
            String minTimestamp = "";
            Int32 minTimestampUnix;
            String maxTimestamp = "";
            Int32 maxTimestampUnix;


            String[] arrParams = rawDateRange.Split('&');
            foreach (String param in arrParams)
            {
                if (param.IndexOf("min_timestamp") > -1)
                {
                    minTimestamp = param.Replace("min_timestamp=", "");
                    if (minTimestamp.Length > 0)
                    {
                        if (Int32.TryParse(minTimestamp, out minTimestampUnix))
                        {
                            output += "&min_timestamp=" + (minTimestampUnix + _daySpan);
                        }
                    }
                }
                if (param.IndexOf("max_timestamp") > -1)
                {
                    maxTimestamp = param.Replace("max_timestamp=", "");
                    if (maxTimestamp.Length > 0)
                    {
                        if (Int32.TryParse(maxTimestamp, out maxTimestampUnix))
                        {
                            output += "&max_timestamp=" + (maxTimestampUnix + _daySpan);
                        }
                    }
                }
            }

            return output;
        }

        private String ProcessNextURL(String nextUrl)
        {
            String output = "";
            String[] arrParams = nextUrl.Split('&');//\u0026
            foreach (String param in arrParams)
            {
                if (param.IndexOf("max_id=") > -1)
                {
                    output = param;
                    break;
                }
            }
            if (!String.IsNullOrEmpty(_feedParamsProvided))
            {
                output += ProcessDateRange(_feedParamsProvided);
            }
            return output;
        }

        private void ProcessFeed(String feed)
        {
            List<Post> rawPosts = new List<Post>();
            if (!String.IsNullOrEmpty(feed))
            {
                InstagramPosts instagramPosts = new JavaScriptSerializer().Deserialize<InstagramPosts>(feed);
                String dateCreated = "";
                String url = "";
                String image = "";
                String caption = "";

                if (instagramPosts.pagination != null)
                {
                    if (!String.IsNullOrEmpty(instagramPosts.pagination.next_url))
                    {
                        _nextUrl = instagramPosts.pagination.next_url;
                    }
                }

                foreach (var post in instagramPosts.data)
                {
                    try
                    {
                        dateCreated = post.created_time;
                    }
                    catch (Exception ex)
                    {
                        dateCreated = "";
                    }
                    if (!string.IsNullOrEmpty(dateCreated))
                    {
                        try
                        {
                            url = post.link;
                        }
                        catch (Exception ex)
                        {
                            url = "";
                        }
                        try
                        {
                            image = post.images.standard_resolution.url;
                        }
                        catch (Exception ex)
                        {
                            image = "";
                        }
                        try
                        {
                            caption = post.caption.text;
                        }
                        catch (Exception ex)
                        {
                            caption = "";
                        }

                        if (!string.IsNullOrEmpty(image))
                        {
                            DateTime instagramPostDate = Utility.UnixTimeStampToDateTime(post.created_time);
                            Post tempPost = new Post();
                            tempPost.type = "instagram";
                            tempPost.postDate = instagramPostDate;
                            tempPost.url = url;
                            tempPost.image = image;
                            tempPost.content = Utility.FormatPost(Utility.UrlFinder(caption), "http://www.twitter.com/hashtag/");
                            rawPosts.Add(tempPost);
                        }
                    }
                }
            }

            if (_allPosts.Count == 0)
            {
                _allPosts = rawPosts;
            }
            else
            {
                _allPosts = _allPosts.Concat(rawPosts).ToList();
            }

            if (rawPosts.Count == 20) {
                MakeAPIRequest(_nextUrl);
            }
        }

        public class InstagramPosts
        {
            public List<InstagramPost> data { get; set; }
            public Pagination pagination { get; set; }
        }

        public class Pagination
        {
            public String next_url { get; set; }
        }
    }
}
