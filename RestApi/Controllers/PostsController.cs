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
    public class PostsController : ApiController
    {
        private static readonly DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0);

        [HttpPost]
        [ActionName("create")]
        public HttpResponseMessage CreatePost(string userName, string imageUrl, string description, string longitude, string latitude)
        {
            CreatePostResult result = new CreatePostResult();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();

                    // TODO: check authorization header
                    // TODO: validation like user name exist

                    // Create post
                    string createPostQuery = "INSERT INTO dbo.posts ([UserName], [ImageUrl], [Description], [Longitude], [Latitude], [Timestamp])" +
                        " VALUES (@userName, @imageUrl, @description, @longitude, @latitude, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(createPostQuery, connection))
                    {
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        cmd.Parameters.Add("@imageUrl", SqlDbType.VarChar, 1024).Value = imageUrl;
                        cmd.Parameters.Add("@description", SqlDbType.VarChar, 255).Value = description;
                        cmd.Parameters.Add("@longitude", SqlDbType.VarChar, 255).Value = longitude;
                        cmd.Parameters.Add("@latitude", SqlDbType.VarChar, 255).Value = latitude;

                        cmd.ExecuteNonQuery();

                        result.success = true;
                        result.message = $"Post created";
                        return Utilities.CreateJsonReponse(Request, result);
                    }
                }
            }
            catch (Exception e)
            {
                result.message = $"Failed to create post: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }

        /// <summary>
        /// Get top N posts sorted by timestamp
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("list")]
        public HttpResponseMessage ListPost(int top = 50)
        {
            ListPostResult result = new ListPostResult();
            List<PostInfo> posts = new List<PostInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();
                    string listQuery = "SELECT TOP (@limit) p.*, u.PhotoUrl FROM posts AS p " +
                        "LEFT JOIN users AS u " +
                        "ON p.UserName = u.userName " +
                        "ORDER BY Timestamp desc";
                    using (SqlCommand cmd = new SqlCommand(listQuery, connection))
                    {
                        cmd.Parameters.Add("@limit", SqlDbType.Int).Value = top;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var post = new PostInfo()
                                {
                                    postId = reader.GetInt32(0),
                                    userName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    imageUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    longitude = reader.GetSqlSingle(4).Value,
                                    latitude = reader.GetSqlSingle(5).Value,
                                    timestamp = (long)reader.GetDateTime(6).Subtract(StartTime).TotalMilliseconds,
                                    userPhotoUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                                };
                                posts.Add(post);
                            }
                        }
                    }

                    result.success = true;
                    result.message = $"List post succeeded";
                    result.posts = posts.ToArray();
                    return Utilities.CreateJsonReponse(Request, result);
                }
            }
            catch (Exception e)
            {
                result.message = $"Failed to list posts: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }

        [HttpGet]
        [ActionName("get")]
        public HttpResponseMessage GetPost(int postId, Boolean includeComment = true) {
            GetPostResult result = new GetPostResult();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();
                    string getQuery = "SELECT p.*, u.PhotoUrl FROM posts AS p " +
                                        "LEFT JOIN users AS u " +
                                        "ON p.UserName = u.userName " +
                                        "WHERE p.PostId = (@postId)";
                                        
                    using (SqlCommand cmd = new SqlCommand(getQuery, connection))
                    {
                        cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var post = new PostInfo()
                                {
                                    postId = reader.GetInt32(0),
                                    userName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    imageUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    longitude = reader.GetSqlSingle(4).Value,
                                    latitude = reader.GetSqlSingle(5).Value,
                                    timestamp = (long)reader.GetDateTime(6).Subtract(StartTime).TotalMilliseconds,
                                    userPhotoUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                                };
                                result.postInfo = post;
                            }
                            else
                            {
                                result.message = $"Post with id {postId} not found!";
                                return Utilities.CreateJsonReponse(Request, result);
                            }
                        }
                    }

                    if (includeComment)
                    {
                        List<PostCommentInfo> comments = new List<PostCommentInfo>();
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
                        result.comments = comments.ToArray();
                    }

                    result.success = true;
                    result.message = $"Get post succeeded";
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
