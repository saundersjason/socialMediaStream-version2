using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SavannahState.SocialMedia
{
    public class FacebookPost
    {
        public string created_time { get; set; }
        public string id { get; set; }
        public string picture { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public string object_id { get; set; }
    }
}
