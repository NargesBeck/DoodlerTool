using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public Vector2 A;
    public Vector2 B;

    public Line(Vector2 _A, Vector2 _B)
    {
        A = _A;
        B = _B;
    }

    public void DrawLine()
    {
        Debug.DrawLine(A, B, Color.magenta, 100);
    }
}
