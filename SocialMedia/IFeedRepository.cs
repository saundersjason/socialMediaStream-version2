using System;
namespace SavannahState.SocialMedia
{
    public interface IFeedRepository
    {
        Result GetFeed(String feedParameters, Int16 maxResults, Int16 numberDays, AccessToken accessToken, String userAccount, String userHandle);
    }
}
