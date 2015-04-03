using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace SavannahState.SocialMedia
{
    public class TwitterRepository : IFeedRepository
    {
        private String _oauthtoken  ="";
        private String _oauthtokensecret = "";
        private String _oauthconsumersecret ="";
        private String _oauthconsumerkey = "";
        private String _screen_name = "";

        private const String _queryURL = "https://api.twitter.com/1.1/statuses/user_timeline.json";
        private Int16 _queryLimit;
        private Int32 _daySpan;
        private List<Post> _allPosts;
        private Int64 _maxId = 0;
        private DateTime _min_timestamp;
        private DateTime _max_timestamp;
        private String _feedParamsProvided = "";

        public TwitterRepository() {
            _allPosts = new List<Post>();
        }

        public Result GetFeed(String accessToken, String feedParameters, Int16 maxResults, Int16 numberDays, String accessTokenSecret, String consumerKey, String consumerSecret, String screenName)
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
                _daySpan = (numberDays * -1);
            }
            else
            {
                _daySpan = -1;
            }

            DateTime today = DateTime.Now;
            DateTime pastDate = DateTime.Now.AddDays(-1 * numberDays);
            _max_timestamp = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
            _min_timestamp = new DateTime(pastDate.Year, pastDate.Month, pastDate.Day, 23, 59, 59);

            if (!String.IsNullOrEmpty(accessToken)  && !String.IsNullOrEmpty(accessTokenSecret)  && !String.IsNullOrEmpty(consumerKey)  && !String.IsNullOrEmpty(consumerSecret)  && !String.IsNullOrEmpty(screenName))
            {
                _oauthtoken = accessToken;
                _oauthtokensecret = accessTokenSecret;
                _oauthconsumersecret = consumerSecret;
                _oauthconsumerkey = consumerKey;
                _screen_name = screenName;

                //Make sure there are any old posts
                _allPosts.Clear();
                String feedURL = _queryURL + "?screen_name=" + _screen_name + "&count=" + _queryLimit;
                
                if (!String.IsNullOrEmpty(feedParameters))
                {
                    _maxId = GetMaxIdFromQuerystring(feedParameters);
                    if (_maxId > 0)
                    {
                        feedURL += "&max_id=" + _maxId.ToString();
                    }
                    _feedParamsProvided = feedParameters;
                    ProcessDateRange(feedParameters,false);
                }

                
                return MakeAPIRequest(feedURL);
            }else{
                throw new Exception("No access token provided.");
            }
        }

        private Result MakeAPIRequest(String apiUrl)
        {
            String feed = "";
            string url = apiUrl;
            string oauthconsumerkey = _oauthconsumerkey;
            string oauthtoken = _oauthtoken;
            string oauthconsumersecret = _oauthconsumersecret;
            string oauthtokensecret = _oauthtokensecret;
            string oauthsignaturemethod = "HMAC-SHA1";
            string oauthversion = "1.0";
            string oauthnonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            string oauthtimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            SortedDictionary<string, string> basestringParameters = new SortedDictionary<string, string>();

            basestringParameters.Add("screen_name", _screen_name);
            basestringParameters.Add("count", _queryLimit.ToString());
            if (_maxId > 0)
            {
                basestringParameters.Add("max_id", _maxId.ToString());
            }
            basestringParameters.Add("oauth_version", oauthversion);
            basestringParameters.Add("oauth_consumer_key", oauthconsumerkey);
            basestringParameters.Add("oauth_nonce", oauthnonce);
            basestringParameters.Add("oauth_signature_method", oauthsignaturemethod);
            basestringParameters.Add("oauth_timestamp", oauthtimestamp);
            basestringParameters.Add("oauth_token", oauthtoken);
            //Build the signature string
            string baseString = String.Empty;
            baseString += "GET" + "&";
            baseString += Uri.EscapeDataString(url.Split('?')[0]) + "&";
            foreach (KeyValuePair<string, string> entry in basestringParameters)
            {
                baseString += Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&");
            }

            //Remove the trailing ambersand char last 3 chars - %26
            baseString = baseString.Substring(0, baseString.Length - 3);

            //Build the signing key
            string signingKey = Uri.EscapeDataString(oauthconsumersecret) + "&" + Uri.EscapeDataString(oauthtokensecret);

            //Sign the request
            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));
            string oauthsignature = Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(baseString)));

            //Tell Twitter we don't do the 100 continue thing
            ServicePointManager.Expect100Continue = false;

            //authorization header
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@url);
            string authorizationHeaderParams = String.Empty;
            authorizationHeaderParams += "OAuth ";
            authorizationHeaderParams += "oauth_nonce=" + "\"" + Uri.EscapeDataString(oauthnonce) + "\",";
            authorizationHeaderParams += "oauth_signature_method=" + "\"" + Uri.EscapeDataString(oauthsignaturemethod) + "\",";
            authorizationHeaderParams += "oauth_timestamp=" + "\"" + Uri.EscapeDataString(oauthtimestamp) + "\",";
            authorizationHeaderParams += "oauth_consumer_key=" + "\"" + Uri.EscapeDataString(oauthconsumerkey) + "\",";
            authorizationHeaderParams += "oauth_token=" + "\"" + Uri.EscapeDataString(oauthtoken) + "\",";
            authorizationHeaderParams += "oauth_signature=" + "\"" + Uri.EscapeDataString(oauthsignature) + "\",";
            authorizationHeaderParams += "oauth_version=" + "\"" + Uri.EscapeDataString(oauthversion) + "\"";
            webRequest.Headers.Add("Authorization", authorizationHeaderParams);

            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            //Allow us a reasonable timeout in case Twitter's busy
            webRequest.Timeout = 3 * 60 * 1000;
            try
            {
                //Proxy settings
                //webRequest.Proxy = new WebProxy("enter proxy details/address");
                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                Stream dataStream = webResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                feed = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                feed = "";
            }
     

            ProcessFeed(feed);

            Result result = new Result();

            if (!String.IsNullOrEmpty(_feedParamsProvided))
            {
                result.NextUrl = ProcessDateRange(_feedParamsProvided,true) + "&max_id=" + (_maxId-1).ToString();
            } else {
                result.NextUrl = "&min_timestamp=" + _min_timestamp.AddDays(_daySpan).ToString("yyyy-MM-dd") + "&max_timestamp=" + _max_timestamp.AddDays(_daySpan).ToString("yyyy-MM-dd") + "&max_id="+(_maxId-1).ToString();
            }
            result.posts = _allPosts;
            return result;
        }

        private String ProcessDateRange(String rawDateRange,Boolean addDayRange)
        {
            String output = "";
            String since = "";
            String until = "";
            DateTime sinceDate;
            DateTime untilDate;


            String[] arrParams = rawDateRange.Split('&');
            foreach (String param in arrParams)
            {
                if (param.IndexOf("min_timestamp") > -1)
                {
                    since = param.Replace("min_timestamp=", "");
                    if (DateTime.TryParse(since, out sinceDate))
                    {
                        if (addDayRange)
                        {
                            output += "&min_timestamp=" + sinceDate.AddDays(_daySpan).ToString("yyyy-MM-dd");
                        }else{
                            _min_timestamp = sinceDate;
                        }
                    }
                }
                if (param.IndexOf("max_timestamp") > -1)
                {
                    until = param.Replace("max_timestamp=", "");
                    if (DateTime.TryParse(until, out untilDate))
                    {
                        if (addDayRange)
                        {
                            output += "&max_timestamp=" + untilDate.AddDays(_daySpan).ToString("yyyy-MM-dd");
                        } else {
                            _max_timestamp = untilDate;
                        }
                    }
                }
            }
            
            return output;
        }

        
        private Int64 GetMaxIdFromQuerystring(String queryString)
        {
            Int64 output = 0;
            String maxId = "";
            String[] arrParams = queryString.Split('&');
            foreach (String param in arrParams)
            {
                if (param.IndexOf("max_id=") > -1)
                {
                    maxId = param.Replace("max_id=", "");
                    break;
                }
            }

            if (!String.IsNullOrEmpty(maxId)) {
                Int64.TryParse(maxId, out output);
            }
            return output;
        }


        private void ProcessFeed(String feed)
        {
            List<Post> rawPosts = new List<Post>();
            if (!String.IsNullOrEmpty(feed))
            {
                Tweets tweets = new JavaScriptSerializer().Deserialize<Tweets>("{\"data\":" + feed + "}");
                String dateCreated = "";
                String url = "";
                String caption = "";

                foreach (var tweet in tweets.data)
                {
                    try
                    {
                        dateCreated = tweet.created_at;
                    }
                    catch (Exception ex)
                    {
                        dateCreated = "";
                    }
                    if (!string.IsNullOrEmpty(dateCreated))
                    {
                        DateTime twitterDateCreated = Utility.ParseDateTime(dateCreated);
                        if (twitterDateCreated > _min_timestamp && twitterDateCreated <= _max_timestamp)
                        {
                            try
                            {
                                if (_maxId == 0)
                                {
                                    _maxId = tweet.id;
                                }
                                else
                                {
                                    if (tweet.id < _maxId)
                                    {
                                        _maxId = tweet.id;
                                    }
                                }
                                url = "https://twitter.com/savannahstate/status/" + tweet.id;
                            }
                            catch (Exception ex)
                            {
                                url = "";
                            }

                            try
                            {
                                caption = tweet.text;
                            }
                            catch (Exception ex)
                            {
                                caption = "";
                            }

                            if (!string.IsNullOrEmpty(caption))
                            {
                                Post tempPost = new Post();
                                tempPost.type = "twitter";
                                tempPost.postDate = twitterDateCreated;
                                tempPost.url = url;
                                tempPost.image = "";
                                tempPost.content = Utility.FormatPost(Utility.UrlFinder(caption), "http://www.twitter.com/hashtag/");
                                rawPosts.Add(tempPost);
                            }
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

            if (rawPosts.Count == 20)
            {
                String feedURL = _queryURL + "?screen_name=" + _screen_name + "&count=" + _queryLimit + "&max_id=" + (_maxId-1).ToString();
                MakeAPIRequest(feedURL);
            }
        }

        
        public class Tweets
        {
            public List<Tweet> data { get; set; }
        }
    }
}
