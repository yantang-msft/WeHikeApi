using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestApi.DataContract
{
    // TODO: inherit from BaseResult
    public class CreatePostResult
    {
        public bool success;
        public string message;
    }

    public class PostInfo
    {
        public int postId;
        public string userName;
        public string userPhotoUrl;
        public string imageUrl;
        public string description;
        public double longitude;
        public double latitude;
        public long timestamp;
    }

    public class ListPostResult
    {
        public bool success;
        public string message;
        public PostInfo[] posts;
    }

    public class GetPostResult
    {
        public bool success;
        public string message;
        public PostInfo postInfo;
        public PostCommentInfo[] comments;
    }
}