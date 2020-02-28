using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineGame.Models
{
    public class InitializeResponse
    {
        //this response is always the same thing so there are no setters
        public string msg
        {
            get { return "INITIALIZE"; }
        }
        public State_Update body
        {
            get { return new State_Update() { newLine = null, heading = "Player 1", message = "Awaiting Player 1's Move" }; }
        }
    }
    public class ServerResponse
    {
        public ServerResponse()
        {
            body = new State_Update();
        }
        public string msg { get; set; }
        public State_Update body { get; set; }
    }
}