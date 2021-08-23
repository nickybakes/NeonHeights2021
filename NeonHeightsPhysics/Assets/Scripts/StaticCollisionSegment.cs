using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StaticCollisionSegment : MonoBehaviour
{

    private LineRenderer lineRenderer;

    public Vector2 a;
    public Vector2 b;

    public Vector2 tangentNormalized;

    public Vector2 leftPointingTangent;
    public Vector2 rightPointingTangent;

    public Vector2 downPointingTangent;
    public Vector2 upPointingTangent;

    public Vector2 normalNormalized;
    public Vector2 midPoint;
    public float angleFromHorizontalRadians;
    public float angleFromHorizontalDegrees;

    public float slope;
    public float yInt;

    public float length;

    public Vector2 topVertex;

    // Start is called before the first frame update
    void Awake()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.startWidth = .5f;
        lineRenderer.endWidth = lineRenderer.startWidth;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.positionCount = 4;
    }

    public void Init(Vector2 a, Vector2 b)
    {
        this.a = a;
        this.b = b;
        if (b.y > a.y)
        {
            topVertex = b;
        }
        else
        {
            topVertex = a;
        }
        length = (b - a).magnitude;
        tangentNormalized = (b - a).normalized;
        //if B vert is to the left
        if (b.x < a.x)
        {
            leftPointingTangent = tangentNormalized;
            rightPointingTangent = (a - b).normalized;
        }
        else
        {
            leftPointingTangent = (a - b).normalized;
            rightPointingTangent = tangentNormalized;
        }

        if (b.y < a.y)
        {
            downPointingTangent = (b - a).normalized;
            upPointingTangent = (a - b).normalized;
        }
        else
        {
            downPointingTangent = (a - b).normalized;
            upPointingTangent = (b - a).normalized;
        }
        normalNormalized = new Vector2(-tangentNormalized.y, tangentNormalized.x);
        midPoint = Vector2.Lerp(a, b, .5f);
        angleFromHorizontalRadians = Mathf.Abs(Mathf.Atan2(tangentNormalized.y, tangentNormalized.x));
        angleFromHorizontalDegrees = Mathf.Rad2Deg * angleFromHorizontalRadians;
        if (b.x == a.x)
        {
            slope = float.NaN;
            yInt = float.NaN;
        }
        else
        {
            slope = (b.y - a.y) / (b.x - a.x);
            yInt = a.y - (slope * a.x);
        }

        lineRenderer.startColor = new Color(angleFromHorizontalDegrees / 180, angleFromHorizontalDegrees / 180, 0);
        lineRenderer.endColor = lineRenderer.startColor;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, a);
        lineRenderer.SetPosition(1, b);
        lineRenderer.SetPosition(2, midPoint);
        lineRenderer.SetPosition(3, midPoint + 1.3f * normalNormalized);

    }
}
