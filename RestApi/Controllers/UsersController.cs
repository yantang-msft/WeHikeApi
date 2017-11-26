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
        public HttpResponseMessage CreateUser(string userName, string password)
        {
            CreateUserResult result = new CreateUserResult();
            try
            {
                using (SqlConnection connection = new SqlConnection(Resources.connectionStr))
                {
                    connection.Open();

                    // Check if user name already exist
                    string checkExistenceQuery = "SELECT [UserName], [Password], [AuthToken] FROM dbo.users WHERE [UserName] = @userName";
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
                    string createNewUserQuery = "INSERT INTO dbo.users ([UserName], [Password], [AuthToken]) VALUES (@userName, @password, @authToken)";
                    using (SqlCommand cmd = new SqlCommand(createNewUserQuery, connection))
                    {
                        string authToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                        cmd.Parameters.Add("@userName", SqlDbType.VarChar, 255).Value = userName;
                        cmd.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = password;
                        cmd.Parameters.Add("@authToken", SqlDbType.VarChar, 255).Value = authToken;

                        cmd.ExecuteNonQuery();

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
                using (SqlConnection connection = new SqlConnection(Resources.connectionStr))
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
                if (Request.Headers.TryGetValues(Resources.AuthorizationHeader, out authHeaders)
                    && authHeaders.Count() == 1)
                {
                    var authToken = authHeaders.ElementAt(0);

                    // TODO: only check token or token plus user name?
                    using (SqlConnection connection = new SqlConnection(Resources.connectionStr))
                    {
                        connection.Open();
                        string verifyTokenQuery = "SELECT* FROM dbo.users WHERE [AuthToken] = @authToken";
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
    }
}
