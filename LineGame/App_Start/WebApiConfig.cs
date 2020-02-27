using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LineGame
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
                name: "Initialize",
                routeTemplate: "initialize",
                defaults: new { controller = "Game", action = "Initialize" }
                );

            config.Routes.MapHttpRoute(
                name: "NodeClicked",
                routeTemplate: "node-clicked",
                defaults: new { controller = "Game", action = "NodeClick" }
                );

            config.Routes.MapHttpRoute(
                name: "Error",
                routeTemplate: "error",
                defaults: new { controller = "Game", action = "Error" }
                );
        }
    }
}
