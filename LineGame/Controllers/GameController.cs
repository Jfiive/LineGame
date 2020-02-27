using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LineGame.Models;

namespace LineGame.Controllers
{
    public class GameController : ApiController
    {
        //can either use a private static varibale representing what nodes have been clicked already or can use session caching to store what has already been clicked
        public IHttpActionResult Initialize()
        {
            //link for information https://docs.microsoft.com/en-us/aspnet/web-api/overview/getting-started-with-aspnet-web-api/tutorial-your-first-web-api
            //look at the routing link so that the requests can be the right URI's
            //this also returns nothing right now need to fix
            return Ok(new InitializeResponse());
        }
        //have to find out where the current client map can be referenced so I can reference all the points that were clicked
        public IHttpActionResult NodeClick()
        {
            return Ok(new ServerResponse());
        }
    }
}
