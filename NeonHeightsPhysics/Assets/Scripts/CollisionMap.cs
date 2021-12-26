using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionMap : MonoBehaviour
{
    //[HideInInspector]
    //public List<CollisionVertex> vertices;
    //[HideInInspector]
    //public List<CollisionSegment> segments;
    [HideInInspector]
    public GameObject collisionVertexPrefab;
    [HideInInspector]
    public GameObject collisionSegmentPrefab;
    [HideInInspector]
    public int editorUpdateFrame;

    public Color vertexColor = Color.red;
    public Color segmentColor = Color.red;
    public Color segmentNormalColor = Color.green;
    [Range(0, 25)]
    public float segmentNormalLength = 1.3f;
    public Color selectionColor = Color.yellow;

    [Header("")]
    public bool mirrorHorizontal = true;


    public List<CollisionVertex> vertices
    {
        get
        {
            List<CollisionVertex> verts = new List<CollisionVertex>();
            foreach(Transform child in transform)
            {
                CollisionVertex vert = child.gameObject.GetComponent<CollisionVertex>();
                if (vert)
                {
                    verts.Add(child.gameObject.GetComponent<CollisionVertex>());
                }
            }
            return verts;
        }
    }

    public List<CollisionSegment> segments
    {
        get
        {
            List<CollisionSegment> segs = new List<CollisionSegment>();
            foreach (Transform child in transform)
            {
                CollisionSegment segment = child.gameObject.GetComponent<CollisionSegment>();
                if (segment)
                {
                    segs.Add(child.gameObject.GetComponent<CollisionSegment>());
                }
            }
            return segs;
        }
    }

    void Start()
    {
        GameObject staticCollisionMapPrefab = (GameObject)Resources.Load("Static Collision Map", typeof(GameObject));
        GameObject newMapObject = Instantiate(staticCollisionMapPrefab);

        StaticCollisionMap newMap = newMapObject.GetComponent<StaticCollisionMap>();

        List<CollisionSegment> segs = segments;
        foreach(CollisionSegment segment in segs)
        {
            newMap.AddSegment(segment.a.transform.position, segment.b.transform.position);
            if (mirrorHorizontal)
            {
                newMap.AddSegment(new Vector2(-segment.b.transform.position.x, segment.b.transform.position.y), new Vector2(-segment.a.transform.position.x, segment.a.transform.position.y));
            }
        }
        newMap.Init();


        Destroy(gameObject);
    }


    public void Init()
    {
        editorUpdateFrame = 0;
        transform.position = new Vector3(0, 0, 0);

        collisionVertexPrefab = (GameObject)Resources.Load("Collision Vertex", typeof(GameObject));
        collisionSegmentPrefab = (GameObject)Resources.Load("Collision Segment", typeof(GameObject));
        
        //vertices = new List<CollisionVertex>();
        AddVertex(Vector2.left*10);
        AddVertex(Vector2.left*5);

        gameObject.isStatic = true;

        //segments = new List<CollisionSegment>();
        AddSegment(vertices[0], vertices[1]);
    }

    public List<CollisionSegment> GetSegmentsAttachedToVertex(CollisionVertex v)
    {
        List<CollisionSegment> segmentsAttachedToThisVertex = new List<CollisionSegment>();
        List<CollisionSegment> segments = this.segments;
        foreach (CollisionSegment segment in segments)
        {
            if (segment != null && (v == segment.a || v == segment.b))
            {
                segmentsAttachedToThisVertex.Add(segment);
            }
        }
        return segmentsAttachedToThisVertex;
    }

    public CollisionVertex AddVertex(Vector2 pos)
    {
        GameObject vertexObject = Instantiate(collisionVertexPrefab, gameObject.transform);
        CollisionVertex vertex = vertexObject.GetComponent<CollisionVertex>();
        vertex.Init(pos);
        //vertices.Add(vertex);
        return vertex;
    }

    public CollisionSegment AddSegment(CollisionVertex a, CollisionVertex b)
    {
        GameObject segmentObject = Instantiate(collisionSegmentPrefab, gameObject.transform);
        CollisionSegment segment = segmentObject.GetComponent<CollisionSegment>();
        segment.Init(a, b);
        //segments.Add(segment);
        return segment;
    }
}
