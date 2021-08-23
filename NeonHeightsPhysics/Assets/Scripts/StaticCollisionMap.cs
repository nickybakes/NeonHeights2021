using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCollisionMap : MonoBehaviour
{
    public GameObject staticCollisionSegmentPrefab;

    public List<StaticCollisionSegment> segments;
    public List<StaticCollisionSegment> northSegments;
    public List<StaticCollisionSegment> eastSegments;
    public List<StaticCollisionSegment> southSegments;
    public List<StaticCollisionSegment> westSegments;

    // Start is called before the first frame update
    void Awake()
    {
        segments = new List<StaticCollisionSegment>();
        northSegments = new List<StaticCollisionSegment>();
        eastSegments = new List<StaticCollisionSegment>();
        southSegments = new List<StaticCollisionSegment>();
        westSegments = new List<StaticCollisionSegment>();
    }

    public void Init()
    {
        
    }

    public void AddSegment(Vector2 a, Vector2 b)
    {
        GameObject newSegment = Instantiate(staticCollisionSegmentPrefab, transform);
        StaticCollisionSegment segment = newSegment.GetComponent<StaticCollisionSegment>();

        segments.Add(segment);

        segment.Init(a, b);

        if(segment.normalNormalized.y > 0)
        {
            northSegments.Add(segment);
        }
        if (segment.normalNormalized.y < 0)
        {
            southSegments.Add(segment);
        }
        if (segment.normalNormalized.x > 0)
        {
            eastSegments.Add(segment);
        }
        if (segment.normalNormalized.x < 0)
        {
            westSegments.Add(segment);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
