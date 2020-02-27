using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineGame.Models
{
    public class Response
    {
        public string msg { get; set; }
        //dont stay as an object need to make a different response model for each type of response
        public object body { get; set; }
    }
}