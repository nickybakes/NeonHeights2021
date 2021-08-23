using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HitBoxVisualizer : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private Rect rect;
    public float width = 10;
    public float height = 10;
    public float xOffset = 0;
    public float yOffset = 0;
    public bool visualize = true;

    public StaticCollisionMap staticCollisionMap;


    // Start is called before the first frame update
    void Start()
    {
    }

    void Awake()
    {
        rect = new Rect(transform.position + new Vector3(xOffset, yOffset, 0), new Vector2(width, height));

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.startWidth = .5f;
        lineRenderer.endWidth = lineRenderer.startWidth;
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.positionCount = 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + .25f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - .25f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = new Vector2(transform.position.x - .25f, transform.position.y);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = new Vector2(transform.position.x + .25f, transform.position.y);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            width -= .25f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            width += .25f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            height -= .25f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            height += .25f;
        }
        staticCollisionMap = FindObjectOfType<StaticCollisionMap>();

        rect.width = width;
        rect.height = height;
        rect.x = transform.position.x + xOffset;
        rect.y = transform.position.y + yOffset;

        lineRenderer.enabled = visualize;
        if (visualize)
        {
            lineRenderer.SetPosition(0, new Vector2(rect.x, rect.y));
            lineRenderer.SetPosition(1, new Vector2(rect.x + rect.width, rect.y));
            lineRenderer.SetPosition(2, new Vector2(rect.x + rect.width, rect.y + rect.height));
            lineRenderer.SetPosition(3, new Vector2(rect.x, rect.y + rect.height));
            lineRenderer.SetPosition(4, new Vector2(rect.x, rect.y));
        }


        List<Vector2> allIntersections = new List<Vector2>();
        foreach (StaticCollisionSegment segment in staticCollisionMap.segments)
        {
            List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, rect);
            foreach(Vector2 v in intersections)
            {
                allIntersections.Add(v);
            }
        }

        //if(allIntersections.Count != 0)
        //{
        //    Debug.Log(allIntersections.Count);
        //}
        
        foreach (Vector2 v in allIntersections)
        {
            Debug.DrawLine(rect.center, v);
        }
    }
}
