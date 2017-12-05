using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestApi.DataContract
{
    public class CreateUserResult
    {
        public bool success;
        public string message;
    }

    public class LoginByPwdResult
    {
        public bool success;
        public string message;
        public string authToken;
    }

    public class VerifyLoginByTokenResult
    {
        public bool success;
        public string message;
    }

    public class UserProfile
    {
        public string userName;
        public string email;
        public string photoUrl;
        public string phoneNumber;
        public string facebookUrl;
        public string twitterUrl;
    }
}