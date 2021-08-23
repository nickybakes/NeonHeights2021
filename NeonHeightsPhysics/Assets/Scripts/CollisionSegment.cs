using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(LineRenderer))]
public class CollisionSegment : MonoBehaviour
{
    public CollisionVertex a;
    public CollisionVertex b;


    public Vector2 normal
    {
        get
        {
            Vector2 direction = (b.transform.position - a.transform.position).normalized;
            return new Vector2(-direction.y, direction.x);
        }
    }

    public Vector2 midPoint
    {
        get
        {
            return Vector2.Lerp(a.transform.position, b.transform.position, .5f);
        }
    }

    public void Init(CollisionVertex a, CollisionVertex b)
    {
        transform.position = Vector3.zero;
        this.a = a;
        this.b = b;
    }

    //private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        //lineRenderer = gameObject.GetComponent<LineRenderer>();
        //lineRenderer.positionCount = 2;
        //lineRenderer.startWidth = .5f;
        //lineRenderer.endWidth = lineRenderer.startWidth;
    }

    // Update is called once per frame
    void Update()
    {
        //lineRenderer.SetPosition(0, a.pos);
        //lineRenderer.SetPosition(1, b.pos);
    }
}
