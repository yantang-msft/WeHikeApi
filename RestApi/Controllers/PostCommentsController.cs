using RestApi.DataContract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace RestApi.Controllers
{
    public class PostCommentsController : ApiController
    {
        private static readonly DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0);

        [HttpPost]
        [ActionName("create")]
        public HttpResponseMessage CreateComment(int postId, string userName, string content)
        {
            CreatePostCommentResult result = new CreatePostCommentResult();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();

                    // TODO: check authorization header
                    // TODO: check valid parameter

                    // Create post comment
                    string query = "INSERT INTO dbo.post_comments ([PostId], [UserName], [Content], [Timestamp])" +
                        " VALUES (@postId, @userName, @content, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId;
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        cmd.Parameters.Add("@content", SqlDbType.VarChar, 1024).Value = content;

                        cmd.ExecuteNonQuery();

                        result.success = true;
                        result.message = $"Comment created";
                        return Utilities.CreateJsonReponse(Request, result);
                    }
                }
            }
            catch (Exception e)
            {
                result.message = $"Failed to create comment: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }

        [HttpGet]
        [ActionName("get")]
        public HttpResponseMessage GetPostComments(int postId)
        {
            GetPostCommentsResult result = new GetPostCommentsResult();
            result.postId = postId;

            List<PostCommentInfo> comments = new List<PostCommentInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();
                    string query = "SELECT * FROM post_comments where PostId = (@postId) ORDER BY Timestamp asc";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var comment = new PostCommentInfo()
                                {
                                    commentId = reader.GetInt32(0),
                                    // index 1 is postId
                                    userName = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    content = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    timestamp = (long)reader.GetDateTime(4).Subtract(StartTime).TotalMilliseconds,
                                };
                                comments.Add(comment);
                            }
                        }
                    }

                    result.success = true;
                    result.message = $"Get post comments succeeded";
                    result.comments = comments.ToArray();
                    return Utilities.CreateJsonReponse(Request, result);
                }
            }
            catch (Exception e)
            {
                result.message = $"Failed to list posts: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }
    }
}
