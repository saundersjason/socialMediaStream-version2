using System;
namespace SavannahState.SocialMedia
{
    public interface IFeedRepository
    {
        Result GetFeed(String accessToken, String feedParameters, Int16 maxResults, Int16 numberDays, String accessTokenSecret, String consumerKey, String consumerSecret, String screenName);
    }
}
