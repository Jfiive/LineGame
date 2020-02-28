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
        //for the scope of this test having variables like this to keep track of the current game information between requests works fine since there is only one person using the application at a time
        //normally this would be not ideal to do since if there is more than one user at a time every user would be changing the same variable which would cause issues
        //if this wasn't just an example program I would send all this information back and forth between the client and server on each request to make sure game info doesnt get changed by people in a different game
        private static List<Point> UsedNodes;
        private static int PlayerTurn;
        private static string TurnType;

        private static Point EndPoint1;
        private static Point EndPoint2;
        private static Point StartOfLine;
        [HttpGet]
        public IHttpActionResult Initialize()
        {
            UsedNodes = new List<Point>();
            PlayerTurn = 1;
            TurnType = "Start Line";
            EndPoint1 = new Point() { x = -1, y = -1 };
            EndPoint2 = new Point() { x = -1, y = -1 };
            StartOfLine = new Point() { x = -1, y = -1 };
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
                        output.body.message = "Select node to complete the line.";
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
                        output.msg = "VALID_START_NODE";
                        output.body.newLine = null;
                        output.body.heading = "Player " + PlayerTurn;
                        output.body.message = "Select node to complete line.";
                        EndRequest();
                        StartOfLine = ClickedPoint;
                    }
                    else
                    {
                        output.msg = "INVALID_START_NODE";
                        output.body.newLine = null;
                        output.body.heading = "Player " + PlayerTurn;
                        output.body.message = "You must start on an endpoint of the line.";
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
                    if (IsValidLine(StartOfLine, ClickedPoint))
                    {
                        output.msg = "VALID_END_NODE";
                        output.body.newLine = new Line() { start = StartOfLine, end = ClickedPoint };
                        output.body.message = null;
                        EndRequest();
                        output.body.heading = "Player " + PlayerTurn;
                    }
                    else
                    {
                        output.msg = "INVALID_END_NODE";
                        output.body.newLine = null;
                        output.body.heading = "Player " + PlayerTurn;
                        output.body.message = "Invalid move. Try again.";
                        //have to go back to previous move since this one was invalid
                        TurnType = "Start Line";
                    }
                }
            }
            if (IsGameOver())
            {
                output.msg = "GAME_OVER";
                //output.body.newLine is already the correct thing from above when it already made the line
                output.body.heading = "Game Over";
                //the turn already gets changed from the EndRequest() call from above so the player turn is already on the winning player
                output.body.message = "Player " + PlayerTurn + " Wins!";
            }
            return Json(output);
        }
        #region Non request methods
        private bool IsValidLine(Point StartPoint, Point EndPoint)
        {
            var IsValid = false;
            var ValidLine = false;
            var PointsOnLine = new List<Point>() { StartPoint, EndPoint };
            
            if (StartPoint.x == EndPoint.x)
            {
                //vertical line
                ValidLine = true;
                if (StartPoint.y > EndPoint.y)
                {
                    //bottom to top
                    for (int i = StartPoint.y - 1; i > EndPoint.y; i--)
                    {
                        PointsOnLine.Add(new Point() { x = EndPoint.x, y = i });
                    }
                }
                else if (StartPoint.y < EndPoint.y)
                {
                    //top to bottom
                    for (int i = StartPoint.y + 1; i < EndPoint.y; i++)
                    {
                        PointsOnLine.Add(new Point() { x = EndPoint.x, y = i });
                    }
                }
            }
            else if (StartPoint.y == EndPoint.y)
            {
                //horizontal line
                ValidLine = true;
                if (StartPoint.x < EndPoint.x)
                {
                    //left to right
                    for (int i = StartPoint.x + 1; i < EndPoint.x; i++)
                    {
                        PointsOnLine.Add(new Point() { x = i, y = EndPoint.y });
                    }
                }
                else if (StartPoint.x > EndPoint.x)
                {
                    //right to left
                    for (int i = StartPoint.x - 1; i > EndPoint.x; i--)
                    {
                        PointsOnLine.Add(new Point() { x = i, y = EndPoint.y });
                    }
                }
            }
            else
            {
                if ((StartPoint.x + StartPoint.y) == (EndPoint.x + EndPoint.y))
                {
                    //     / type line
                    ValidLine = true;
                    if (StartPoint.y < EndPoint.y)
                    {
                        //top to bottom
                        var numPoints = (EndPoint.y - StartPoint.y) - 1;
                        for (int i = 1; i <= numPoints; i++)
                        {
                            PointsOnLine.Add(new Point() { x = StartPoint.x - i, y = StartPoint.y + i });
                        }
                    }
                    else if (StartPoint.y > EndPoint.y)
                    {
                        //bottom to top
                        var numPoints = (StartPoint.y - EndPoint.y) - 1;
                        for (int i = 1; i <= numPoints; i++)
                        {
                            PointsOnLine.Add(new Point() { x = StartPoint.x + i, y = StartPoint.y - i });
                        }
                    }
                }
                else if ((StartPoint.x - StartPoint.y) == (EndPoint.x - EndPoint.y))
                {
                    //       \ type line
                    ValidLine = true;
                    if (StartPoint.y < EndPoint.y)
                    {
                        //top to bottom
                        var numPoints = (EndPoint.y - StartPoint.y) - 1;
                        for (int i = 1; i <= numPoints; i++)
                        {
                            PointsOnLine.Add(new Point() { x = StartPoint.x + i, y = StartPoint.y + i });
                        }
                    }
                    else if (StartPoint.y > EndPoint.y)
                    {
                        //bottom to top
                        var numPoints = (StartPoint.y - EndPoint.y) - 1;
                        for (int i = 1; i <= numPoints; i++)
                        {
                            PointsOnLine.Add(new Point() { x = StartPoint.x - i, y = StartPoint.y - i });
                        }
                    }
                }
            }

            //check if the line crossed a line that was already made
            if (ValidLine)
            {
                if (UsedNodes.Count > 1)
                {
                    var cross = false;
                    foreach (var point in PointsOnLine)
                    {
                        if (IsInUsedNodes(point) && !point.IsSame(StartPoint))
                        {
                            cross = true;
                            break;
                        }
                    }
                    if (cross == false)
                    {
                        IsValid = true;
                        foreach (var point in PointsOnLine)
                        {
                            if (!IsInUsedNodes(point))
                            {
                                UsedNodes.Add(point);
                            }
                        }
                    }
                }
                else
                {
                    //cannot intersect another line if it is the first line of the game
                    IsValid = true;
                    foreach (var point in PointsOnLine)
                    {
                        if (!IsInUsedNodes(point))
                        {
                            UsedNodes.Add(point);
                        }
                    }
                }
            }

            if (IsValid && (EndPoint1.x >= 0 && EndPoint2.x >= 0))
            {
                if (EndPoint1.IsSame(StartPoint))
                {
                    EndPoint1 = EndPoint;
                }
                else if (EndPoint2.IsSame(StartPoint))
                {
                    EndPoint2 = EndPoint;
                }
            }
            return IsValid;
        }
        private bool IsInUsedNodes(Point Check)
        {
            var output = false;
            foreach (var point in UsedNodes)
            {
                if (point.x == Check.x && point.y == Check.y)
                {
                    output = true;
                    break;
                }
            }
            return output;
        }
        private bool IsEndPoint(Point Check)
        {
            if (EndPoint1.IsSame(Check) || EndPoint2.IsSame(Check))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsGameOver()
        {
            //game is over if the surrounding nodes on both endpoints are already apart of a line
            if (TurnType == "End Line")
            {
                //Turn types are reversed because the turn type gets changed for the next term before this is called
                //this skips doing all the logic on a start line turn since theres no way the game can end of that turn type
                return false;
            }

            var End1Done = false;
            var End2Done = false;

            var end1Surroundings = new List<Point>();
            end1Surroundings.Add(new Point() { x = EndPoint1.x - 1, y = EndPoint1.y - 1 });
            end1Surroundings.Add(new Point() { x = EndPoint1.x, y = EndPoint1.y - 1 });
            end1Surroundings.Add(new Point() { x = EndPoint1.x + 1, y = EndPoint1.y - 1 });
            end1Surroundings.Add(new Point() { x = EndPoint1.x + 1, y = EndPoint1.y });
            end1Surroundings.Add(new Point() { x = EndPoint1.x + 1, y = EndPoint1.y + 1 });
            end1Surroundings.Add(new Point() { x = EndPoint1.x, y = EndPoint1.y + 1 });
            end1Surroundings.Add(new Point() { x = EndPoint1.x - 1, y = EndPoint1.y + 1 });
            end1Surroundings.Add(new Point() { x = EndPoint1.x - 1, y = EndPoint1.y });

            foreach (var node in end1Surroundings.ToArray())
            {
                if ((node.x < 0 || node.x > 3) || (node.y < 0 || node.y > 3))
                {
                    end1Surroundings.Remove(node);
                }
            }

            var end2Surroundings = new List<Point>();
            end2Surroundings.Add(new Point() { x = EndPoint2.x - 1, y = EndPoint2.y - 1 });
            end2Surroundings.Add(new Point() { x = EndPoint2.x, y = EndPoint2.y - 1 });
            end2Surroundings.Add(new Point() { x = EndPoint2.x + 1, y = EndPoint2.y - 1 });
            end2Surroundings.Add(new Point() { x = EndPoint2.x + 1, y = EndPoint2.y });
            end2Surroundings.Add(new Point() { x = EndPoint2.x + 1, y = EndPoint2.y + 1 });
            end2Surroundings.Add(new Point() { x = EndPoint2.x, y = EndPoint2.y + 1 });
            end2Surroundings.Add(new Point() { x = EndPoint2.x - 1, y = EndPoint2.y + 1 });
            end2Surroundings.Add(new Point() { x = EndPoint2.x - 1, y = EndPoint2.y });

            foreach (var node in end2Surroundings.ToArray())
            {
                if ((node.x < 0 || node.x > 3) || (node.y < 0 || node.y > 3))
                {
                    end2Surroundings.Remove(node);
                }
            }


            var BeenUsed = 0;
            foreach (var node in end1Surroundings)
            {
                if (IsInUsedNodes(node))
                {
                    BeenUsed++;
                }
            }
            if (BeenUsed == end1Surroundings.Count)
            {
                End1Done = true;
            }

            BeenUsed = 0;
            foreach (var node in end2Surroundings)
            {
                if (IsInUsedNodes(node))
                {
                    BeenUsed++;
                }
            }
            if (BeenUsed == end2Surroundings.Count)
            {
                End2Done = true;
            }

            if (End1Done && End2Done)
            {
                return true;
            }
            else
            {
                return false;
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
