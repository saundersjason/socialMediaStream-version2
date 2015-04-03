using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SavannahState.SocialMedia
{
    public class Result
    {
        public List<Post> posts { get; set; }
        public String NextUrl { get; set; }
        public Result() { }
    }
}
