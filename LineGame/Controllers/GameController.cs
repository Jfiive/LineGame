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
        //for the scope of this test having variables like this to keep track of the current game information between requests since there is only one person using the application at a time
        //normally this would be not ideal to do since if there is more than one user at a time every user would be changing the same variable which would cause issues
        //if I had more access to the front end I would send this variable to the client after every request and then at the start of every request send it back to the controller
        private static List<Point> UsedNodes;
        private static int PlayerTurn;
        private static string TurnType;

        private static Point EndPoint1;
        private static Point EndPoint2;
        [HttpGet]
        public IHttpActionResult Initialize()
        {
            //reset the clicked nodes when a new game starts
            UsedNodes = new List<Point>();
            PlayerTurn = 1;
            TurnType = "Start Line";
            EndPoint1 = new Point() { x = -1, y = -1 };
            EndPoint2 = new Point() { x = -1, y = -1 };
            return Json(new InitializeResponse());
        }

        [HttpPost]
        public IHttpActionResult NodeClick(Point ClickedPoint)
        {
            var output = new ServerResponse();
            if (TurnType == "Start Line")
            {
                if (UsedNodes.Count == 0)
                {
                    if (ClickedPoint != null)
                    {
                        output.msg = "VALID_START_NODE";
                        output.body.newLine = null;
                        output.body.heading = "Player 1";
                        output.body.message = "Select another node to complete the line.";
                        UsedNodes.Add(ClickedPoint);
                        EndRequest();
                        EndPoint1 = ClickedPoint;
                    }
                    else
                    {
                        //dont think this can actually happen but it's one of the possible server messages so it's here
                        output.msg = "INVALID_START_NODE";
                        output.body.newLine = null;
                        output.body.heading = "Player 1";
                        output.body.message = "Invalid starting node. Please select a node on the grid.";
                    }
                }
                else
                {
                    //new player turn needs to start line
                    if (IsEndPoint(ClickedPoint))
                    {
                        //success
                    }
                    else
                    {
                        //invalid move and needs to go back to the last move
                    }
                }
            }
            else if (TurnType == "End Line")
            {
                if (UsedNodes.Count == 1)
                {
                    //this is the second part of the first turn of the game
                    if (IsValidLine(UsedNodes[0], ClickedPoint))
                    {
                        output.msg = "VALID_END_NODE";
                        output.body.newLine = new Line() { start = UsedNodes[0], end = ClickedPoint };
                        output.body.heading = "Player 2";
                        output.body.message = null;
                        //needs to add all the nodes that are on the line
                        EndRequest();
                        EndPoint2 = ClickedPoint;
                    }
                    else
                    {
                        output.msg = "INVALID_END_NODE";
                        output.body.newLine = null;
                        output.body.heading = "Player 1";
                        output.body.message = "Invalid move. Try again.";
                        //reset variables because you need to start the line again
                        Initialize();
                    }
                }
                else
                {
                    //end line move that isnt the first move so there are no special rules for this one
                }
            }
            return Json(output);
        }
        #region Non request methods
        private bool IsValidLine(Point StartPoint, Point EndPoint)
        {
            var IsValid = false;
            if (StartPoint.x == EndPoint.x)
            {
                //vertical line
                IsValid = true;
                UsedNodes.Add(EndPoint);
                
                //add all the nodes in between the start and endpoints to the used nodes list
                if (StartPoint.y > EndPoint.y)
                {
                    //bottom to top
                    for (int i = StartPoint.y - 1; i > EndPoint.y; i--)
                    {
                        UsedNodes.Add(new Point() { x = EndPoint.x, y = i });
                    }
                }
                else if (StartPoint.y < EndPoint.y)
                {
                    //top to bottom
                    for (int i = StartPoint.y + 1; i < EndPoint.y; i++)
                    {
                        UsedNodes.Add(new Point() { x = EndPoint.x, y = i });
                    }
                }
            }
            else if (StartPoint.y == EndPoint.y)
            {
                //horizontal line
                IsValid = true;
                UsedNodes.Add(EndPoint);
            }
            else
            {
                // a "/" type line the sum of the x and y positions will be the same
                // a "\" type line the different between the highest and the lowest will be the same
                if ((StartPoint.x + StartPoint.y) == (EndPoint.x + EndPoint.y))
                {
                    IsValid = true;
                }
                else if ((StartPoint.x - StartPoint.y) == (EndPoint.x - EndPoint.y))
                {
                    IsValid = true;
                }
            }
            return IsValid;
        }
        private bool IsEndPoint(Point Check)
        {
            if (EndPoint1 == Check || EndPoint2 == Check)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsGameOver(Point Check)
        {
            //to find out if the point is an endpoint or not all the surrounding nodes cannot be in UsedNodes
            var surroundingNodes = new List<Point>();
            surroundingNodes.Add(new Point() { x = Check.x - 1, y = Check.y - 1 });
            surroundingNodes.Add(new Point() { x = Check.x, y = Check.y - 1 });
            surroundingNodes.Add(new Point() { x = Check.x + 1, y = Check.y - 1 });
            surroundingNodes.Add(new Point() { x = Check.x + 1, y = Check.y });
            surroundingNodes.Add(new Point() { x = Check.x + 1, y = Check.y + 1 });
            surroundingNodes.Add(new Point() { x = Check.x, y = Check.y + 1 });
            surroundingNodes.Add(new Point() { x = Check.x - 1, y = Check.y + 1 });
            surroundingNodes.Add(new Point() { x = Check.x - 1, y = Check.y });

            foreach (var node in surroundingNodes)
            {
                //clean out the surrounding nodes that aren't on the grid
                if ((node.x < 0 || node.x > 3) || (node.y < 0 || node.y > 3))
                {
                    surroundingNodes.Remove(node);
                }
            }

            var freeNodes = 0;
            foreach (var node in surroundingNodes)
            {
                if (UsedNodes.Contains(node))
                {
                    freeNodes++;
                }
            }

            if (freeNodes > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void EndRequest()
        {
            if (TurnType == "Start Line")
            {
                TurnType = "End Line";
            }
            else if (TurnType == "End Line")
            {
                TurnType = "Start Line";

                if (PlayerTurn == 1)
                {
                    PlayerTurn = 2;
                }
                else if (PlayerTurn == 2)
                {
                    PlayerTurn = 1;
                }
            }
        }
        #endregion
    }
}
