using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Web;
using System.Text.RegularExpressions;

namespace SavannahState.SocialMedia
{
    public class FacebookRepository : IFeedRepository
    {
        private String _accessToken = "";
        private const String _queryURL = "https://graph.facebook.com/v2.2/81383047843/posts";
        private Int16 _queryLimit;
        private Int16 _daySpan;
        private List<Post> _allPosts;
        private String _nextUrl;
        private DateTime _dateSince;
        private DateTime _dateUntil = DateTime.Now;
        private String _feedParamsProvided = "";
        
        public FacebookRepository()
        {
            _allPosts = new List<Post>();
        }

        public Result GetFeed(String accessToken, String feedParameters, Int16 maxResults, Int16 numberDays, String accessTokenSecret, String consumerKey, String consumerSecret, String userId)
        {
            if (maxResults > 0)
            {
                _queryLimit = maxResults;
            }
            else {
                _queryLimit = 1;
            }
            
            if (numberDays > 0)
            {
                _daySpan = (Int16)(numberDays * -1);
            }else{
                _daySpan = -1;
            }
            _dateSince = DateTime.Now.AddDays(_daySpan);

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
                String feedURL = _queryURL + "?access_token=" + _accessToken + "&limit=" + _queryLimit;
                if (!String.IsNullOrEmpty(feedParameters))
                {
                    _feedParamsProvided = feedParameters;
                    feedURL += "&" + feedParameters;
                }
                else
                {
                    if (_dateUntil.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        feedURL += "&since=" + _dateSince.ToString("yyyy-MM-dd");
                    }
                    else {
                        feedURL += "&since=" + _dateSince.ToString("yyyy-MM-dd") + "&until=" + _dateUntil.ToString("yyyy-MM-dd");
                    }
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
                throw new Exception("Call to retrieve Facebook posts failed. URL :" + apiUrl + ". Message: " + ex.Message);
            }
            ProcessFeed(feed);

            Result result = new Result();
            if (!String.IsNullOrEmpty(_nextUrl))
            {
                result.NextUrl = ProcessNextURL(_nextUrl);
            }
            result.posts = _allPosts;
            return result;
        }

        private String ProcessDateRange(String rawDateRange)
        {
            String output = "";
            String since = "";
            String until = "";
            DateTime sinceDate;
            DateTime untilDate;

    
            String[] arrParams = rawDateRange.Split('&');
            foreach (String param in arrParams)
            {
                if (param.IndexOf("since") > -1)
                {
                    since = param.Replace("since=","");
                    if (DateTime.TryParse(since, out sinceDate)) {
                        output += "&since=" + sinceDate.AddDays(_daySpan).ToString("yyyy-MM-dd");
                    }
                }
                if (param.IndexOf("until") > -1)
                {
                    until = param.Replace("until=","");
                    if (DateTime.TryParse(until, out untilDate))
                    {
                        output += "&until=" + untilDate.AddDays(_daySpan).ToString("yyyy-MM-dd");
                    }
                }
            }

            return output;
        }

        private String ProcessNextURL(String nextUrl) {
            String output = "";
            String[] arrParams = nextUrl.Split('&');
            foreach (String param in arrParams)
            {
                if (param.IndexOf("_paging_token=") > -1)
                {
                    output = param;
                    break;
                }
            }
            if (String.IsNullOrEmpty(_feedParamsProvided))
            {
                output += "&since=" + _dateSince.AddDays(_daySpan).ToString("yyyy-MM-dd") + "&until=" + _dateSince.ToString("yyyy-MM-dd");
            }
            else
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
                FacebookPosts facebookPosts = new JavaScriptSerializer().Deserialize<FacebookPosts>(feed);
                String dateCreated = "";
                String url = "";
                String image = "";
                String caption = "";

                if (facebookPosts.paging != null)
                {
                    if (!String.IsNullOrEmpty(facebookPosts.paging.next))
                    {
                        _nextUrl = facebookPosts.paging.next;
                    }
                }

                foreach (var post in facebookPosts.data)
                {
                    try
                    {
                        dateCreated = post.created_time;
                    }
                    catch
                    {
                        dateCreated = "";
                    }
                    if (!string.IsNullOrEmpty(dateCreated))
                    {
                        try
                        {
                            if (post.id.IndexOf("_") != -1)
                            {
                                url = "https://www.facebook.com/" + post.id.Replace("_", "/posts/");
                            }
                            else
                            {
                                url = "https://www.facebook.com/" + post.id;
                            }
                        }
                        catch
                        {
                            url = "";
                        }
                        try
                        {
                            image = post.picture;
                        }
                        catch
                        {
                            image = "";
                        }
                        try
                        {
                            caption = post.message;
                        }
                        catch
                        {
                            caption = "";
                        }

                        if (!string.IsNullOrEmpty(image))
                        {
                            char[] delimiterChars = { '&' };
                            if (post.type == "photo")
                            {
                                if (image.IndexOf("_s") != -1)
                                {
                                    image = image.Replace("_s", "_o");
                                }
                                else if (!string.IsNullOrEmpty(post.object_id))
                                {
                                    image = "http://graph.facebook.com/" + post.object_id + "/picture?width=9999&height=9999";
                                }
                            }
                            else
                            {
                                string[] qps = image.Split(delimiterChars);
                                foreach (string s in qps)
                                {
                                    if (s.IndexOf("url=") != -1)
                                    {
                                        image = HttpUtility.UrlDecode(s.Replace("url=", ""));
                                    }
                                    else
                                    {
                                        if (s.IndexOf("src=") != -1)
                                        {
                                            image = HttpUtility.UrlDecode(s.Replace("src=", ""));
                                        }
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(image) || !string.IsNullOrEmpty(caption))
                        {
                            DateTime facebookDateCreated = Convert.ToDateTime(dateCreated);
                            Post tempPost = new Post();
                            tempPost.type = "facebook";
                            tempPost.postDate = facebookDateCreated;
                            tempPost.url = url;
                            tempPost.image = image;
                            tempPost.content = Utility.FormatPost(Utility.UrlFinder(caption), "http://www.facebook.com/hashtag/");
                            rawPosts.Add(tempPost);
                        }
                    }
                }
            }

            rawPosts = ProcessImages(rawPosts);
            if (_allPosts.Count == 0)
            {
                _allPosts = rawPosts;
            }
            else {
                _allPosts = _allPosts.Concat(rawPosts).ToList();
            }

            if (rawPosts.Count == 20) {
                MakeAPIRequest(_nextUrl);
            }
        }

        private List<Post> ProcessImages(List<Post> posts) {
            foreach (Post post in posts) {
                if (!String.IsNullOrEmpty(post.image))
                {
                    if (!Utility.ImageExists(post.image))
                    {
                        post.image = "";
                    }
                }
            }
            return posts;
        }

        private class FacebookPosts
        {
            public List<FacebookPost> data { get; set; }
            public Paging paging { get; set; }
        }

        private class Paging
        {
            public String next { get; set; }
        }
    }
}
