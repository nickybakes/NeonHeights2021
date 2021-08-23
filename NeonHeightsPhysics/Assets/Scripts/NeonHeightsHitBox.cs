using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonHeightsHitBox
{
    /// <summary>
    /// the four corners of the hitbox, going in clockwise order
    /// </summary>

    public List<Vector2> CheckCollisionWithStaticMap(StaticCollisionMap map)
    {
        List<Vector2> collisions = new List<Vector2>();
        foreach(StaticCollisionSegment segment in map.segments)
        {
            
        }

        return collisions;
    }

    public static List<Vector2> StaticLineSegmentAndBoxCollision(StaticCollisionSegment line, Rect box)
    {
        List<Vector2> collisions = new List<Vector2>();

        Vector2 leftSideIntersection = LineSegmentIntersection(line.a, line.b, line.slope, line.yInt, new Vector2(box.x, box.y), new Vector2(box.x, box.y + box.height), float.NaN, float.NaN);
        //LineSegmentIntersectWithVerticalLine(new Vector2(box.x, box.y), box.height, line.a, line.b, line.slope);
        if (!float.IsNaN(leftSideIntersection.x))
        {
            collisions.Add(leftSideIntersection);
        }

        Vector2 rightSideIntersection = LineSegmentIntersection(line.a, line.b, line.slope, line.yInt, new Vector2(box.x + box.width, box.y), new Vector2(box.x + box.width, box.y + box.height), float.NaN, float.NaN);
        //LineSegmentIntersectWithVerticalLine(new Vector2(box.x + box.width, box.y), box.height, line.a, line.b, line.slope);
        if (!float.IsNaN(rightSideIntersection.x))
        {
            collisions.Add(rightSideIntersection);
        }


        Vector2 topSideIntersection = LineSegmentIntersection(line.a, line.b, line.slope, line.yInt, new Vector2(box.x, box.y + box.height), new Vector2(box.x + box.width, box.y + box.height), 0, box.y + box.height);
        //(0, box.y + box.height, line.slope, line.yInt, box.x, box.x + box.width, line.a.x, line.b.x
        if (!float.IsNaN(topSideIntersection.x))
        {
            collisions.Add(topSideIntersection);
        }

        Vector2 bottomSideIntersection = LineSegmentIntersection(line.a, line.b, line.slope, line.yInt, new Vector2(box.x, box.y), new Vector2(box.x + box.width, box.y), 0, box.y);
        //0, box.y, line.slope, line.yInt, box.x, box.x + box.width, line.a.x, line.b.x
        if (!float.IsNaN(bottomSideIntersection.x))
        {
            collisions.Add(bottomSideIntersection);
        }

        return collisions;
    }

    public static Vector2 LineSegmentIntersectWithVerticalLine(Vector2 verticalLinePos, float verticalLineLength, Vector2 a, Vector2 b, float slope)
    {
        if(float.IsNaN(slope))
        {
            return new Vector2(float.NaN, float.NaN);
        }
        Vector2 adjustedA = a - verticalLinePos;
        Vector2 adjustedB = b - verticalLinePos;

        if ((adjustedA.x >= 0 && adjustedB.x <= 0) || (adjustedA.x <= 0 && adjustedB.x >= 0))
        {
            float y =  adjustedA.y - (slope * adjustedA.x);
            if (y <= verticalLineLength && y >= 0)
            {
                return new Vector2(verticalLinePos.x, verticalLinePos.y + y);
            }
        }

        return new Vector2(float.NaN, float.NaN);
    }

    public static Vector2 LineSegmentIntersection(Vector2 vert1A, Vector2 vert1B, float slope1, float yInt1, Vector2 vert2A, Vector2 vert2B, float slope2, float yInt2)
    {
        if (slope1 == slope2)
        {
            return new Vector2(float.NaN, float.NaN);
        }

        if (float.IsNaN(slope1))
        {
            if (vert1A.y < vert1B.y)
            {
                return LineSegmentIntersectWithVerticalLine(vert1A, Mathf.Abs(vert1B.y - vert1A.y), vert2A, vert2B, slope2);
            }
            else
            {
                return LineSegmentIntersectWithVerticalLine(vert1B, Mathf.Abs(vert1A.y - vert1B.y), vert2A, vert2B, slope2);
            }
        }

        if (float.IsNaN(slope2))
        {
            if (vert2A.y < vert2B.y)
            {
                return LineSegmentIntersectWithVerticalLine(vert2A, Mathf.Abs(vert2B.y - vert2A.y), vert1A, vert1B, slope1);
            }
            else
            {
                return LineSegmentIntersectWithVerticalLine(vert2B, Mathf.Abs(vert2A.y - vert2B.y), vert1A, vert1B, slope1);
            }
        }

        float x = (yInt2 - yInt1) / (slope1 - slope2);

        if (x >= Mathf.Min(vert1A.x, vert1B.x) && x >= Mathf.Min(vert2A.x, vert2B.x) && x <= Mathf.Max(vert1A.x, vert1B.x) && x <= Mathf.Max(vert2A.x, vert2B.x))
        {
            return new Vector2(x, slope1 * x + yInt1);
        }

        return new Vector2(float.NaN, float.NaN);
    }

    //public static Vector2 LineSegmentIntersection(float slope1, float yInt1, float slope2, float yInt2, float line1X1, float line1X2, float line2X1, float line2X2)
    //{
    //    if(slope1 == slope2)
    //    {
    //        return new Vector2(float.NaN, float.NaN);
    //    }

    //    if(slope1 == float.NaN)
    //    {
    //        //return LineSegmentIntersectWithVerticalLine()
    //    }

    //    float x = (yInt2 - yInt1) / (slope1 - slope2);

    //    if(x >= Mathf.Min(line1X1, line1X2) && x >= Mathf.Min(line2X1, line2X2) && x <= Mathf.Max(line1X1, line1X2) && x <= Mathf.Max(line2X1, line2X2))
    //    {
    //        return new Vector2(x, slope1 * x + yInt1);
    //    }

    //    return new Vector2(float.NaN, float.NaN);
    //}

    //public static Vector2 LineSegmentIntersection(Vector2 p, Vector2 r, Vector2 q, Vector2 s)
    //{
    //    float crossRS = Cross(r, s);
    //    float crossQPR = Cross(q - p, r);
    //    if (crossRS == 0 && crossQPR == 0)
    //    {

    //    }
    //    else if (crossRS == 0 && crossQPR != 0)
    //    {

    //    }

    //    float t = Cross(q - p, s) / crossRS;
    //    float u = crossQPR / crossRS;

    //    return new Vector2(float.NaN, float.NaN);
    //}


    public static float Cross(Vector2 v, Vector2 w)
    {
        return v.x * w.y - v.y * w.x;
    }
}
