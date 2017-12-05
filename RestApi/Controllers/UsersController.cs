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
    public class UsersController : ApiController
    {

        [HttpPost]
        [ActionName("create")]
        public HttpResponseMessage CreateUser(string userName, string password, string email)
        {
            CreateUserResult result = new CreateUserResult();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();

                    // Check if user name already exist
                    string checkExistenceQuery = "SELECT * FROM dbo.users WHERE [UserName] = @userName";
                    using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, connection))
                    {
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result.message = $"User name {userName} already exist!";
                                return Utilities.CreateJsonReponse(Request, result);
                            }
                        }
                    }

                    // Create new user
                    string createNewUserQuery = "INSERT INTO dbo.users ([UserName], [Password], [Email], [AuthToken]) VALUES (@userName, @password, @email, @authToken)";
                    using (SqlCommand cmd = new SqlCommand(createNewUserQuery, connection))
                    {
                        string authToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        cmd.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = password;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 255).Value = email;
                        cmd.Parameters.Add("@authToken", SqlDbType.VarChar, 255).Value = authToken;

                        cmd.ExecuteNonQuery();

                        result.success = true;
                        result.message = $"User {userName} created";
                        return Utilities.CreateJsonReponse(Request, result);
                    }
                }
            }
            catch (Exception e)
            {
                result.message = $"Failed to create new user: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }

        [HttpGet]
        [ActionName("login")]
        public HttpResponseMessage LoginByPwd(string userName, string password)
        {
            LoginByPwdResult result = new LoginByPwdResult();
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();

                    string loginQuery = "SELECT [AuthToken] FROM dbo.users WHERE [UserName] = @userName and [Password] = @password";
                    using (SqlCommand cmd = new SqlCommand(loginQuery, connection))
                    {
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        cmd.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = password;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result.success = true;
                                result.message = $"Login succeeded";
                                result.authToken = reader.GetString(0);

                                return Utilities.CreateJsonReponse(Request, result);
                            }
                        }
                    }

                    result.message = $"User name or password is incorrect";
                    return Utilities.CreateJsonReponse(Request, result);
                }
            }
            catch (Exception e)
            {
                result.message = $"Login failed: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }

        [HttpGet]
        [ActionName("verify")]
        public HttpResponseMessage VerifyAuthorizationToken()
        {
            VerifyLoginByTokenResult result = new VerifyLoginByTokenResult();
            try
            {

                IEnumerable<string> authHeaders;
                if (Request.Headers.TryGetValues(Constants.AuthorizationHeader, out authHeaders)
                    && authHeaders.Count() == 1)
                {
                    var authToken = authHeaders.ElementAt(0);

                    // TODO: only check token or token plus user name?
                    using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                    {
                        connection.Open();
                        string verifyTokenQuery = "SELECT * FROM dbo.users WHERE [AuthToken] = @authToken";
                        using (SqlCommand cmd = new SqlCommand(verifyTokenQuery, connection))
                        {
                            cmd.Parameters.Add("@authToken", SqlDbType.VarChar, 255).Value = authToken;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    result.success = true;
                                    return Utilities.CreateJsonReponse(Request, result);
                                }
                                else
                                {
                                    result.message = $"AuthorizationHeader is not set";
                                    return Utilities.CreateJsonReponse(Request, result);
                                }
                            }
                        }
                    }
                }
                else
                {
                    result.message = $"AuthorizationHeader is not set";
                    return Utilities.CreateJsonReponse(Request, result);
                }
            }
            catch (Exception e)
            {
                result.message = $"Verify login failed: {e.Message}";
                return Utilities.CreateJsonReponse(Request, result);
            }
        }

        [HttpPut]
        [ActionName("update")]
        public HttpResponseMessage UpdateUserProfile(string userName, string password = null, string email = null, string photoUrl = null, string phoneNumber = null, string facebookUrl = null, string twitterUrl = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();

                    // TODO: need to authenticate user

                    // If everything is null, report error
                    if (password == null && email == null && photoUrl == null && phoneNumber == null && facebookUrl == null && twitterUrl == null)
                    {
                        return Utilities.CreateBadRequestResponse(Request, $"Nothing to update");
                    }

                    List<string> parameters = new List<string>();
                    if (password != null)
                    {
                        parameters.Add("Password=(@password)");
                    }
                    if (email != null)
                    {
                        parameters.Add("Email=(@email)");
                    }
                    if (photoUrl != null)
                    {
                        parameters.Add("PhotoUrl=(@photoUrl)");
                    }
                    if (phoneNumber != null)
                    {
                        parameters.Add("PhoneNumber=(@phoneNumber)");
                    }
                    if (facebookUrl != null)
                    {
                        parameters.Add("FacebookUrl=(@facebookUrl)");
                    }
                    if (twitterUrl != null)
                    {
                        parameters.Add("TwitterUrl=(@twitterUrl)");
                    }
                    string parameterStr = string.Join(", ", parameters);

                    string query = "UPDATE dbo.users " +
                        "SET " + parameterStr + " " +
                        "WHERE UserName = (@userName)";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        if (password != null)
                        {
                            cmd.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = password;
                        }
                        if (email != null)
                        {
                            cmd.Parameters.Add("@email", SqlDbType.VarChar, 255).Value = email;
                        }
                        if (photoUrl != null)
                        {
                            cmd.Parameters.Add("@photoUrl", SqlDbType.VarChar, 1024).Value = photoUrl;
                        }
                        if (phoneNumber != null)
                        {
                            cmd.Parameters.Add("phoneNumber", SqlDbType.VarChar, 255).Value = phoneNumber;
                        }
                        if (facebookUrl != null)
                        {
                            cmd.Parameters.Add("@facebookUrl", SqlDbType.VarChar, 1024).Value = facebookUrl;
                        }
                        if (twitterUrl != null)
                        {
                            cmd.Parameters.Add("@twitterUrl", SqlDbType.VarChar, 1024).Value = twitterUrl;
                        }

                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            return Utilities.CreateBadRequestResponse(Request, $"User {userName} not found!");
                        }
                        else
                        {
                            return Utilities.CreateJsonReponse(Request, new { });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return Utilities.CreateBadRequestResponse(Request, e.Message);
            }
        }

        [HttpGet]
        [ActionName("get")]
        public HttpResponseMessage GetUserProfile(string userName) {
            try
            {
                using (SqlConnection connection = new SqlConnection(Constants.ConnectionStr))
                {
                    connection.Open();


                    string query = "SELECT UserName, Email, PhotoUrl, PhoneNumber, FacebookUrl, TwitterUrl FROM dbo.users WHERE [UserName] = @userName";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var profile = new UserProfile()
                                {
                                    userName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                    email = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    photoUrl = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    phoneNumber = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    facebookUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    twitterUrl = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                };
                                return Utilities.CreateJsonReponse(Request, profile);
                            }
                            else
                            {
                                return Utilities.CreateBadRequestResponse(Request, $"User {userName} not found!");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return Utilities.CreateBadRequestResponse(Request, e.Message);
            }
        }
    }
}
