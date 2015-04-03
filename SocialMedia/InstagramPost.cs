using System;

namespace SavannahState.SocialMedia
{
    public class InstagramPost
    {
        public String created_time { get; set; }
        public String link { get; set; }
        public InstagramPostImageResolution images { get; set; }
        public InstagramPostCaption caption { get; set; }
    }
}
