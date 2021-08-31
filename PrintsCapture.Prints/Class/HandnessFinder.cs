namespace PrintsCapture.Prints
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using PrintsCapture.Prints.Enum;

    public class HandnessFinder
    {       

        public static Hand ReturnHandness(PrintInfo print)
        {
            if (print.HandPart != HandPart.FourFlats)
            {
                return Hand.None;
            }

            if (print.Segments.Any(x => !x.IsExpected || x.Part.IsMissing || x.Position.IsEmpty))
            {
                return Hand.None;
            }

            var segments = print.Segments;
            var incorrectHand = print.Hand == Hand.Left ? Hand.Right : Hand.Left;
            var middle = segments.Single(x => x.Part.HandPart == HandPart.Middle);
            var ring = segments.Single(x => x.Part.HandPart == HandPart.Ring);
            var index = segments.Single(x => x.Part.HandPart == HandPart.Index);
            var little = segments.Single(x => x.Part.HandPart == HandPart.Little);

            var sizes = segments.Select(x => new KeyValuePair<PrintSegment, int>(x, x.Position.Height * x.Position.Width)).ToList();
            var smallest = sizes.First(x => x.Value == sizes.Min(y => y.Value));
            var largest = sizes.First(x => x.Value == sizes.Max(y => y.Value));

            var score = 0;

            if (smallest.Key == little)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            if (largest.Key == middle)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            //if (little.Position.Bottom < index.Position.Bottom)
            //{
            //    score += 3;
            //}
            //else
            //{
            //    score -= 1;
            //}

            var indexMiddle = GetDistance(index.Position, middle.Position);
            var ringLittle = GetDistance(ring.Position, little.Position);

            if (indexMiddle < ringLittle)
            {
                score += 2;
            }
            else
            {
                score -= 1;
            }

            // if hands were inverted, the distance between middle and ring won't be the smallest

            var topPosition = segments.Min(x => x.Position.Top);
            var isMiddleOnTop = middle.Position.Top == topPosition;
            var isRingOnTop = ring.Position.Top == topPosition;

            if (isMiddleOnTop)
            {
                score += 2;
            }
            else
            {
                score -= 1;
            }

            var posLittle = GetCenter(little.Position);
            var posRing = GetCenter(ring.Position);
            var posMiddle = GetCenter(middle.Position);
            var posIndex = GetCenter(index.Position);
            PointF closest;

            var distMiddle = FindDistanceToSegment(posMiddle, posRing, posIndex, out closest);
            var distRing = FindDistanceToSegment(posRing, posMiddle, posLittle, out closest);

            if (distMiddle > distRing)
            {
                score += 3;
            }
            else
            {
                score -= 1;
            }

            Console.WriteLine("Self detect Score: {0}", score);

            if (score >= 3)
            {
                return print.Hand;
            }

            if (score <= -2)
            {
                return incorrectHand;
            }
            
            return Hand.None;                        
        }

        private static PointF GetCenter(Rectangle rect)
        {
            var r = new PointF();
            r.X = rect.Left + rect.Width / 2;
            r.Y = rect.Top + rect.Width / 2;
            return r;
        }

        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        private static double FindDistanceToSegment(
            PointF pt, PointF p1, PointF p2, out PointF closest)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new PointF(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new PointF(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new PointF(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static int GetDistance(Rectangle rect1, Rectangle rect2)
        {

            var x1 = rect1.Left + (rect1.Width / 2);
            var y1 = rect1.Top + (rect1.Height / 2);

            var x2 = rect2.Left + (rect2.Height / 2);
            var y2 = rect2.Top + (rect2.Height / 2);

            var deltaX = Math.Pow( Math.Abs(x1-x2),2);
            var deltaY = Math.Pow( Math.Abs(y1 - y2),2);

            var distance = Math.Sqrt(deltaX + deltaY);

            return (int)distance;
        }

        public static int GetAngleTriangles(Rectangle rect1, Rectangle rect2)
        {
            /* |\
             * | \
             * |  \
             * |   \
             * -----\
            */

            // 1st --> Hypothenuse ! Based on two points that create the line \
            var deltaX = Math.Pow(Math.Abs(rect2.X - rect1.X),2);
            var deltaY = Math.Pow(Math.Abs(rect2.Y - rect1.Y),2);
            var hyp = Math.Sqrt(deltaX + deltaY);

            // 2nd --> Opposite. Straight line from lowest y to highest -
            var opp = Math.Abs(rect1.X - rect2.X);

            var sinRatio = opp / hyp;

            // 3rd --> The angle |\
            var angle = Math.Asin(sinRatio) * (180 / Math.PI);
            
            // 4th --> Opposite angle
            angle = 180 - 90 - angle;
            

            return (int)angle;
        }

        public static int GetAngle2Lines(Rectangle rect1, Rectangle rect2)
        {

            //    Line1: (x1, y1), (x2, y2) (rect1.X, rect1.Y), (rect2.X, rect2.Y)
            //    Line2: (x3, y3), (x4, y4) 
            //    Math.Atan2(y2-y1, x2-x1) - Math.Atan2(y4-y3,x4-x3)
            var angle = (Math.Atan2(rect2.Bottom - rect2.Top, rect2.Right - rect2.Left) - 
                         Math.Atan2(Math.Abs(rect1.Y - rect2.Y), 0)) 
                        * (180 / Math.PI);

            return (int)angle;
        }

        /// <summary>
        /// Angle between 2 points ... Nope
        /// </summary>
        /// <param name="rect1"></param>
        /// <param name="rect2"></param>
        /// <returns></returns>
        public static int GetAngle2Points(Rectangle rect1, Rectangle rect2)
        {
            var deltaX = rect2.X - rect1.X;
            var deltaY = rect2.Y - rect1.Y;

            var angle = Math.Atan2(deltaY, deltaX) * (180 / Math.PI);

            return (int)angle;
        }

        public static double GetAngleOfLineBetweenTwoPoints(PointF p1, PointF p2)
        {
            float xDiff = p2.X - p1.X; 
            float yDiff = p2.Y - p1.Y; 
            return Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
        }

    }
}
