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

        public SocialMediaStreamService()
        {
        }

        [WebMethod(CacheDuration = 40)]
        [ScriptMethod(UseHttpGet = true)]
        public MediaItem GetAllSocialPostsFiltered(Int32 numberOfPosts, String mediaType, String hashTag, String facebookParams, String twitterParams, String instagramParams)
        {
            MediaItem filteredResult = new MediaItem();

            filteredResult = GetAllSocialPosts(numberOfPosts, mediaType, facebookParams, twitterParams, instagramParams);
            if (filteredResult.posts.Count > 0)
            {
                if (!String.IsNullOrEmpty(hashTag))
                {
                    filteredResult.posts = filteredResult.posts.Where(p => p.content.ToLower().Contains(hashTag.ToLower())).ToList();
                }
                
                if (numberOfPosts > 0)
                {
                    filteredResult.posts = filteredResult.posts.OrderByDescending(o => o.postDate).Take(numberOfPosts).ToList();
                }
            }

            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            Context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            return filteredResult;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public MediaItem GetAllSocialPosts(Int32 numberOfPosts, String mediaType, String facebookParams, String twitterParams, String instagramParams)
        {
            Result facebookResult=null;
            Result instagramResult=null;
            Result twitterResult=null;
            if (mediaType == "instagram" || mediaType == "all")
            {
                instagramResult = new InstagramRepository().GetFeed(_InstagramAccessToken, instagramParams, 20, 2, "", "", "", "");
            }
            if (mediaType == "facebook" || mediaType == "all")
            {
                facebookResult = new FacebookRepository().GetFeed(_FacebookAccessToken, facebookParams, 20, 2, "", "", "", "");
            }
            if (mediaType == "twitter" || mediaType == "all")
            {
                twitterResult = new TwitterRepository().GetFeed(_TwitterAccessToken, twitterParams, 20, 2, _TwitterAccessTokenSecret, _TwitterConsumerKey, _TwitterConsumerSecret, _TwitterScreenName);
            }

            MediaItem mediaItems = new MediaItem();
            if (instagramResult != null || facebookResult != null || twitterResult != null)
            {
                if (instagramResult != null)
                {
                    mediaItems.posts = mediaItems.posts.Concat(instagramResult.posts).ToList();
                    if (!String.IsNullOrEmpty(instagramResult.NextUrl))
                    {
                        mediaItems.nextUrls.Add(new NextUrl { urlType = "instagram", url = instagramResult.NextUrl });
                    }
                }

                if (facebookResult != null)
                {
                    mediaItems.posts = mediaItems.posts.Concat(facebookResult.posts).ToList();
                    if (!String.IsNullOrEmpty(facebookResult.NextUrl))
                    {
                        mediaItems.nextUrls.Add(new NextUrl { urlType="facebook",url=facebookResult.NextUrl});
                    }
                }

                if (twitterResult != null)
                {
                    mediaItems.posts = mediaItems.posts.Concat(twitterResult.posts).ToList();
                    if (!String.IsNullOrEmpty(twitterResult.NextUrl))
                    {
                        mediaItems.nextUrls.Add(new NextUrl { urlType = "twitter", url = twitterResult.NextUrl });
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
