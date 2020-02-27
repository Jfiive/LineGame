using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineGame.Models
{
    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Line
    {
        public Point start { get; set; }
        public Point end { get; set; }
    }

    public class State_Update
    {
        public Line newLine { get; set; }
        public string heading { get; set; }
        public string message { get; set; }
    }
}