using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestApi
{
    public class Constants
    {
        public static readonly string ConnectionStr = "Server=tcp:wehike.database.windows.net,1433;Initial Catalog=WeHike;Persist Security Info=False;User ID=wehike;Password=#Bugsfor";
        public static readonly string AuthorizationHeader = "AuthorizationHeader";
    }
}