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


        List<Account> _FacebookAccounts = new List<Account>() { 
                new Account(){
                    id = "81383047843",
                    name = "savannahstate"
                },
                new Account(){
                    id = "186864151626",
                    name = "Asa H. Gordon Library at Savannah State University"
                }
            };

        List<Account> _InstagramAccounts = new List<Account>() { 
                new Account(){
                    id = "455868730",
                    name = "savannahstate"
                },
                new Account(){
                    id = "324926923",
                    name = "write_ssu"
                }
            };

        List<Account> _TwitterAccounts = new List<Account>() { 
                new Account(){
                    id = "savannahstate",
                    name = "savannahstate"
                },
                new Account(){
                    id = "SavStUTigerAdm",
                    name = "SavStUTigerAdm"
                }
            };




        public SocialMediaStreamService()
        {
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public MediaItem GetAllSocialPosts(Int32 numberOfPosts, String mediaType, String facebookParams, String twitterParams, String instagramParams)
        {

            Result facebookResult = null;
            Result instagramResult = null;
            Result twitterResult = null;

            MediaItem mediaItems = new MediaItem();
            if (mediaType == "instagram" || mediaType == "all")
            {
                AccessToken instagramAccessToken = new AccessToken() { accessToken = _InstagramAccessToken };
                if (!String.IsNullOrEmpty(instagramParams))
                {
                    String[] instagramParamArr = instagramParams.Split('~');
                    Int32 instagramAccountCnt = 0;
                    if (instagramParamArr.Length == _InstagramAccounts.Count)
                    {
                        foreach (String instagramParam in instagramParamArr)
                        {
                            instagramResult = new InstagramRepository().GetFeed(instagramParam, 20, 2, instagramAccessToken, _InstagramAccounts[instagramAccountCnt].id, _InstagramAccounts[instagramAccountCnt].name);
                            if (instagramResult != null)
                            {
                                mediaItems.posts = mediaItems.posts.Concat(instagramResult.posts).ToList();
                                if (!String.IsNullOrEmpty(instagramResult.NextUrl))
                                {
                                    mediaItems.nextUrls.Add(new NextUrl { urlType = "instagram", url = instagramResult.NextUrl });
                                }
                            }
                            instagramAccountCnt++;
                        }
                    }
                }
                else
                {
                    foreach (Account instagramAccount in _InstagramAccounts)
                    {
                        instagramResult = new InstagramRepository().GetFeed("", 20, 2, instagramAccessToken, instagramAccount.id, instagramAccount.name);
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
                AccessToken facebookAccessToken = new AccessToken() { accessToken = _FacebookAccessToken };
                if (!String.IsNullOrEmpty(facebookParams))
                {
                    String[] facebookParamArr = facebookParams.Split('~');
                    Int32 facebookAccountCnt = 0;
                    if (facebookParamArr.Length == _FacebookAccounts.Count)
                    {
                        foreach (String facebookParam in facebookParamArr)
                        {
                            facebookResult = new FacebookRepository().GetFeed(facebookParam, 20, 2, facebookAccessToken, _FacebookAccounts[facebookAccountCnt].id, _FacebookAccounts[facebookAccountCnt].name);
                            if (facebookResult != null)
                            {
                                mediaItems.posts = mediaItems.posts.Concat(facebookResult.posts).ToList();
                                if (!String.IsNullOrEmpty(facebookResult.NextUrl))
                                {
                                    mediaItems.nextUrls.Add(new NextUrl { urlType = "facebook", url = facebookResult.NextUrl });
                                }
                            }
                            facebookAccountCnt++;
                        }
                    }
                }
                else
                {
                    foreach (Account facebookAccount in _FacebookAccounts)
                    {
                        facebookResult = new FacebookRepository().GetFeed("", 20, 2, facebookAccessToken, facebookAccount.id, facebookAccount.name);
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
                AccessToken twitterAccessToken = new AccessToken() { accessToken = _TwitterAccessToken, accessTokenSecret = _TwitterAccessTokenSecret, consumerKey = _TwitterConsumerKey, consumerSecret = _TwitterConsumerSecret };
                if (!String.IsNullOrEmpty(twitterParams))
                {
                    String[] twitterParamArr = twitterParams.Split('~');
                    Int32 twitterAccountCnt = 0;
                    foreach (String twitterParam in twitterParamArr)
                    {
                        twitterResult = new TwitterRepository().GetFeed(twitterParam, 20, 2, twitterAccessToken, _TwitterAccounts[twitterAccountCnt].id, _TwitterAccounts[twitterAccountCnt].name);
                        if (twitterResult != null)
                        {
                            mediaItems.posts = mediaItems.posts.Concat(twitterResult.posts).ToList();
                            if (!String.IsNullOrEmpty(twitterResult.NextUrl))
                            {
                                mediaItems.nextUrls.Add(new NextUrl { urlType = "twitter", url = twitterResult.NextUrl });
                            }
                        }
                        twitterAccountCnt++;
                    }
                }
                else
                {
                    foreach (Account twitterAccount in _TwitterAccounts)
                    {
                        Int32 twitterAccountCnt = 0;
                        twitterResult = new TwitterRepository().GetFeed("", 20, 2, twitterAccessToken, twitterAccount.id, twitterAccount.name);
                        if (twitterResult != null)
                        {
                            mediaItems.posts = mediaItems.posts.Concat(twitterResult.posts).ToList();
                            if (!String.IsNullOrEmpty(twitterResult.NextUrl))
                            {
                                mediaItems.nextUrls.Add(new NextUrl { urlType = "twitter", url = twitterResult.NextUrl });
                            }
                        }
                        twitterAccountCnt++;
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

    public struct Account
    {

        public String id { get; set; }
        public String name { get; set; }
    }
}
