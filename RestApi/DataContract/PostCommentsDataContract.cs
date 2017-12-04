using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestApi.DataContract
{
    // TODO: inherit from BaseResult
    public class CreatePostCommentResult
    {
        public bool success;
        public string message;
    }

    public class PostCommentInfo
    {
        public int commentId;
        public string userName;
        public string content;
        public long timestamp;
    }

    public class GetPostCommentsResult
    {
        public bool success;
        public string message;
        public int postId;
        public PostCommentInfo[] comments;
    }
}