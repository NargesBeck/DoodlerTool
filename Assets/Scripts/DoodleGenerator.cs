using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoodleGenerator : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> frameBoundsList = new List<Vector2>();

    [SerializeField]
    private List<GameObject> gameObjects = new List<GameObject>();

    [SerializeField]
    private List<Line> frameLinesList;

    public float minX, minY, maxX, maxY;

    private float MinX
    {
        get
        {
            if (minX == 0 && minY == 0 && maxX == 0 && maxY == 0)
                AssignMinMaxBounds();
            return minX;
        }
    }
    private float MinY
    {
        get
        {
            if (minX == 0 && minY == 0 && maxX == 0 && maxY == 0)
                AssignMinMaxBounds();
            return minY;
        }
    }
    private float MaxX
    {
        get
        {
            if (minX == 0 && minY == 0 && maxX == 0 && maxY == 0)
                AssignMinMaxBounds();
            return maxX;
        }
    }
    private float MaxY
    {
        get
        {
            if (minX == 0 && minY == 0 && maxX == 0 && maxY == 0)
                AssignMinMaxBounds();
            return maxY;
        }
    }

    private List<Line> FrameLinesList
    {
        get
        {
            if (frameLinesList == null || frameLinesList.Count == 0)
            {
                AssignFramesLines();
            }
            return frameLinesList;
        }
    }

    private List<Line> lines = new List<Line>();

    private new Transform transform;
    private Transform Transform
    {
        get
        {
            if (transform == null)
                transform = GetComponent<Transform>();
            return transform;
        }
    }

    public int angleInDegrees;

    bool calcPositionOnXAxis; // once on X, once on Y

    [ContextMenu("AssignMinMaxBounds")]
    private void AssignMinMaxBounds()
    {
        if (frameBoundsList.Count == 0)
            return;

        minX = maxX = frameBoundsList[0].x;
        minY = maxY = frameBoundsList[0].y; 

        for (int i = 1; i < frameBoundsList.Count; i++)
        {
            if (frameBoundsList[i].x > MaxX)
                maxX = frameBoundsList[i].x;
            if (frameBoundsList [i].y > MaxY)
                maxY = frameBoundsList[i].y;
            if (frameBoundsList[i].x < minX)
                minX = frameBoundsList[i].x;
            if (frameBoundsList[i].y < minY)
                minY = frameBoundsList[i].y;
        }
    }
    
    [ContextMenu("CalculateNewLine")]
    private void CalculateNewLine()
    {
        // upward normal vector
        Vector2 rawNormalVector = Transform.up;
        Debug.DrawLine(Vector2.zero, rawNormalVector, Color.white, 100);

        // random rotation
        angleInDegrees = Random.Range(90, 270);
        Vector2 rotatedVector = ApplyRandomTeta(rawNormalVector, angleInDegrees);
        Debug.DrawLine(Vector2.zero, rotatedVector, Color.yellow, 100);

        // random origin pos for the vector
        Vector2 pointA = RandomPosition(angleInDegrees);
        Vector2 pointB = pointA + rotatedVector;

        pointB *= MaxLineLengthPossibleInFrame();

        Debug.Log(pointA);
        Debug.DrawLine(pointA, pointB, Color.green, 100);

        // let's make a line out of this vector (we only are missing point B)
        CalculatePointB(pointA, pointB);
    }

    /// <summary>
    /// receives degree, it will be changed to radian inside the method
    /// </summary>
    private Vector2 ApplyRandomTeta(Vector2 v1, int angle)
    {
        float teta = (float)angle * Mathf.Deg2Rad;
        /* x2 = cosβ * x1 − sinβ * y1
         * y2 = sinβ * x1 + cosβ * y1 */
        float x2 = Mathf.Cos(teta) * v1.x - Mathf.Sin(teta) * v1.y;
        float y2 = Mathf.Sin(teta) * v1.x - Mathf.Cos(teta) * v1.y;
        return new Vector2(x2, y2);
    }

    /// <returns>returns origin for the direction of method or L(0) = A+v*0</returns>
    private Vector2 RandomPosition(int teta)
    {
        float x, y;
        calcPositionOnXAxis = !calcPositionOnXAxis;
        if (calcPositionOnXAxis)
        {
            /*   \
             * |  \    |
             * |___\___|
             * draw the vector from the bottom border upwards while randomizing its X */

            x = Random.Range(MinX, MaxX);
            y = MinY;
        }
        else
        {
            /* ____
             *     |
             *   \ |
             *    \|
             * ____|
             * draw the vector from right to left if it's 90 to 180 
             *   else then draw from left to right
             *   
             *        180
             *         ^
             *         |
             *   90<---|--->270 */

            y = Random.Range(MinY, MaxY);
            x = (teta <= 90 && teta >= 180) ? MaxX : MinX;
        }
        return new Vector2(x, y);
    }

    private void CalculatePointB(Vector2 pointA, Vector2 v)
    {
        // increase t (in L(t) = A + v*t formula) till you intersect a frame border

        List<float> intersectionWithFrameTs = new List<float>();
        for (int i = 0; i < FrameLinesList.Count; i++)
        {
            // v: our line's vector
            // u: a frame line's vector
            // c: u's origin - v's origin

            Vector2 u = FrameLinesList[i].B - FrameLinesList[i].A;
            Vector2 c = FrameLinesList[i].A - pointA;

            float t;
            if (IntersectAt(u, v, c, pointA, out t))
            {
                Debug.Log($"{pointA + v * t} - {t}");
                gameObjects[i].transform.position = pointA + v * t;
            }
        }
        
        // break if intersected with a line
    }

    private bool IntersectAt(Vector2 u, Vector2 v, Vector2 c, Vector2 vA, out float t)
    {
        // v: our line's vector
        // u: a frame line's vector
        // c: u's origin - v's origin

        t = 0;
        Vector2 uPerp = new Vector2(-u.y, u.x);
        if (DotProduct(uPerp, v) == 0)
            return false;

        t = DotProduct(uPerp, c) / DotProduct(uPerp, v);
        Debug.Log(vA + v * t);

        if (t < 0 || t > 1)
            return false;
        return true;
    }

    private float DotProduct(Vector2 v, Vector2 w)
    {
        return (v.x * w.x + v.y * w.y);
    }

    private void AssignFramesLines()
    {
        // square
        frameLinesList = new List<Line>();
        frameLinesList.Add(new Line(new Vector2(MinX, MinY), new Vector2(MaxX, MinY)));
        frameLinesList.Add(new Line(new Vector2(MinX, MinY), new Vector2(MinX, MaxY)));
        frameLinesList.Add(new Line(new Vector2(MaxX, MaxY), new Vector2(MaxX, MinY)));
        frameLinesList.Add(new Line(new Vector2(MaxX, MaxY), new Vector2(MinX, MaxY)));

        for (int i = 0; i < 4; i++)
        {
            frameLinesList[i].DrawLine();
        }
    }

    private float MaxLineLengthPossibleInFrame()
    {
        // square
        // a = max x - min x
        // v*v = a*a + a*a
        return Mathf.Sqrt(2 * (MaxX - MinX));
    }

}
