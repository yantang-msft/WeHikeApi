﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RestApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserApi",
                routeTemplate: "users/{action}",
                defaults: new { controller = "users" }
            );

            config.Routes.MapHttpRoute(
                name: "PostApi",
                routeTemplate: "posts/{action}",
                defaults: new { controller = "posts" }
            );

            config.Routes.MapHttpRoute(
                name: "PostCommentApi",
                routeTemplate: "post/comments/{action}",
                defaults: new { controller = "postComments" }
            );
        }
    }
}
