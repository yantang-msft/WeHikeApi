using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace RestApi
{
    public class Utilities
    {
        public static HttpResponseMessage CreateJsonReponse(HttpRequestMessage request, object result)
        {
            HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(new JavaScriptSerializer().Serialize(result), Encoding.UTF8, "application/json");

            return response;
        }
    }
}