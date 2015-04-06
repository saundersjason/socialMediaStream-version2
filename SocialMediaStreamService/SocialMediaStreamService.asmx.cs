using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using SavannahState.SocialMedia;
using System.Web.Script.Services;
using System.Web.Configuration;

namespace SavannahState
{
    [WebService(Namespace = "http://www.savannahstate.edu", Description = "Returns all social media posts.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class SocialMediaStreamService : System.Web.Services.WebService
    {
        private String _FacebookAccessToken = WebConfigurationManager.AppSettings["facebook_access_token"];
        private String _InstagramAccessToken = WebConfigurationManager.AppSettings["instagram_access_token"];
        private String _TwitterAccessToken = WebConfigurationManager.AppSettings["twitter_oauthtoken"];
        private String _TwitterAccessTokenSecret = WebConfigurationManager.AppSettings["twitter_oauthtokensecret"];
        private String _TwitterConsumerKey = WebConfigurationManager.AppSettings["twitter_oauthconsumerkey"];
        private String _TwitterConsumerSecret = WebConfigurationManager.AppSettings["twitter_oauthconsumersecret"];
        private String _TwitterScreenName = WebConfigurationManager.AppSettings["twitter_screenname"];



        //_FacebookAccessToken - 111f44b4b528cf408f9662ab629e39d8Reset

        //Consumer Key (API Key)9gRaRlR5zAWPPQVXZ9P5xA
        //Consumer Secret (API Secret)OJrKFw1sxnmTHOBWF3SoWtTbzjQDNP8w2sy90wy8
        //Access Token87486291-4Wh2ZA5kCxsB68AhTIbUsTkIL4c6HyodsX8AODZE
        //Access Token SecretK7cG41W4Wicemn2syKmSxmTjPbCA2JtheONAcfB6mP8






        public SocialMediaStreamService()
        {
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public MediaItem GetAllSocialPosts(Int32 numberOfPosts, String mediaType, String facebookParams, String twitterParams, String instagramParams)
        {





            /*
             NEXTURL's must be sent out in the same order as they are processed when dealing with multiple accounts from the same media service. If a nexturl is not available for a particular account, send a blank space or null as a placeholder.
             */





            Result facebookResult=null;
            Result instagramResult=null;
            Result twitterResult=null;

            MediaItem mediaItems = new MediaItem();
            if (mediaType == "instagram" || mediaType == "all")
            {
                if (!String.IsNullOrEmpty(instagramParams))
                {
                    String[] instagramParamArr = instagramParams.Split('|');
                    foreach (String instagramParam in instagramParamArr)
                    {
                        instagramResult = new InstagramRepository().GetFeed(_InstagramAccessToken, instagramParam, 20, 2, "", "", "", "");
                        if (instagramResult != null)
                        {
                            mediaItems.posts = mediaItems.posts.Concat(instagramResult.posts).ToList();
                            if (!String.IsNullOrEmpty(instagramResult.NextUrl))
                            {
                                mediaItems.nextUrls.Add(new NextUrl { urlType = "instagram", url = instagramResult.NextUrl });
                            }
                        }
                    }
                }
            }
            if (mediaType == "facebook" || mediaType == "all")
            {
                if (!String.IsNullOrEmpty(facebookParams))
                {
                    String[] facebookParamArr = facebookParams.Split('|');
                    foreach (String facebookParam in facebookParamArr)
                    {
                        facebookResult = new FacebookRepository().GetFeed(_FacebookAccessToken, facebookParam, 20, 2, "", "", "", "");
                        if (facebookResult != null)
                        {
                            mediaItems.posts = mediaItems.posts.Concat(facebookResult.posts).ToList();
                            if (!String.IsNullOrEmpty(facebookResult.NextUrl))
                            {
                                mediaItems.nextUrls.Add(new NextUrl { urlType = "facebook", url = facebookResult.NextUrl });
                            }
                        }
                    }
                }
            }
            if (mediaType == "twitter" || mediaType == "all")
            {
                if (!String.IsNullOrEmpty(twitterParams))
                {
                    String[] twitterParamArr = twitterParams.Split('|');
                    foreach (String twitterParam in twitterParamArr)
                    {
                        twitterResult = new TwitterRepository().GetFeed(_TwitterAccessToken, twitterParam, 20, 2, _TwitterAccessTokenSecret, _TwitterConsumerKey, _TwitterConsumerSecret, _TwitterScreenName);
                        if (twitterResult != null)
                        {
                            mediaItems.posts = mediaItems.posts.Concat(twitterResult.posts).ToList();
                            if (!String.IsNullOrEmpty(twitterResult.NextUrl))
                            {
                                mediaItems.nextUrls.Add(new NextUrl { urlType = "twitter", url = twitterResult.NextUrl });
                            }
                        }
                    }
                }
            }

            if (mediaItems.posts.Count > 0)
            {
                if (numberOfPosts > 0)
                {
                    mediaItems.posts = mediaItems.posts.OrderByDescending(o => o.postDate).Take(numberOfPosts).ToList();
                }
                else
                {
                    mediaItems.posts = mediaItems.posts.OrderByDescending(o => o.postDate).ToList();
                }
            }

            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            Context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            Context.Response.AddHeader("Access-Control-Max-Age", "1");
            return mediaItems;
        }
    }

    public class MediaItem {

        public List<NextUrl> nextUrls { get; set; }
        public List<Post> posts { get; set; }
        
        public MediaItem() {
            nextUrls = new List<NextUrl>();
            posts = new List<Post>();
        }
    }

    public class NextUrl
    {
        public String urlType { get; set; }
        public String url { get; set; }

        public NextUrl()
        {
        }
    }
}
