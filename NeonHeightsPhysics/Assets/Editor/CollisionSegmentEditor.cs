using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad, CanEditMultipleObjects, CustomEditor(typeof(CollisionSegment))]
public class CollisionSegmentEditor : Editor
{
    public Event currentEvent;
    public Event previousEvent;
    public CollisionSegment segment;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);
        if (GUILayout.Button("Flip Normal Direction"))
        {
            List<CollisionSegment> selectedSegments = GetSelectedSegments();
            foreach(CollisionSegment seg in selectedSegments)
            {
                Undo.RecordObject(seg, "Flip Normal Direction");
                CollisionVertex a = seg.a;
                seg.a = seg.b;
                seg.b = a;
            }
            SceneView.RepaintAll();
        }
    }

    public void OnSceneGUI()
    {
        Tools.hidden = true;

        //Debug.Log("aawadd");
        previousEvent = currentEvent;
        currentEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition).origin;


        if (currentEvent.type == EventType.ExecuteCommand)
        {
            //Debug.Log(currentEvent);
        }
        if (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.Delete)
        {
            //Debug.Log("delete");
        }
        if (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.V && !currentEvent.shift && !currentEvent.control && !currentEvent.alt)
        {
            List<CollisionSegment> selectedSegments = GetSelectedSegments();
            List<Object> newSelection = new List<Object>();
            foreach (CollisionSegment s in selectedSegments)
            {
                GameObject[] verts = { s.a.gameObject, s.b.gameObject };
                newSelection.Add(s.a.gameObject);
                newSelection.Add(s.b.gameObject);
            }

            Object[] objects = Selection.objects;
            for (int i = 0; i < objects.Length; i++)
            {
                if (!((GameObject)objects[i]).GetComponent<CollisionSegment>() || !selectedSegments.Contains(((GameObject)objects[i]).GetComponent<CollisionSegment>()))
                {
                    newSelection.Add(objects[i]);
                }
            }
            Selection.objects = newSelection.ToArray();
            return;
        }
        if (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.B && !currentEvent.shift && !currentEvent.control && !currentEvent.alt && segment != null)
        {
            CollisionMap map = segment.transform.gameObject.GetComponentInParent<CollisionMap>();
            List<CollisionSegment> selectedSegments = GetSelectedSegmentsInMap(map);
            foreach(CollisionSegment seg in selectedSegments)
            {
                CollisionVertex v = map.AddVertex(seg.midPoint);
                CollisionSegment segA = map.AddSegment(seg.a, v);
                CollisionSegment segB = map.AddSegment(v, seg.b);
                Undo.DestroyObjectImmediate(seg.gameObject);
                Undo.RegisterCreatedObjectUndo(v.gameObject, "Split Segment");
                Undo.RegisterCreatedObjectUndo(segA.gameObject, "Split Segment");
                Undo.RegisterCreatedObjectUndo(segB.gameObject, "Split Segment");
            }
        }

        if (currentEvent.commandName == "Delete" || currentEvent.commandName == "SoftDelete" && segment != null)
        {
            currentEvent.Use();
            Undo.DestroyObjectImmediate(segment.gameObject);
        }
    }

    static List<CollisionSegment> GetSelectedSegmentsInMap(CollisionMap map)
    {
        List<CollisionSegment> segments = new List<CollisionSegment>();
        Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] is GameObject)
            {
                GameObject g = (GameObject)selectedObjects[i];
                if (g.GetComponent<CollisionSegment>() != null && map.segments.Contains(g.GetComponent<CollisionSegment>()))
                {
                    segments.Add(g.GetComponent<CollisionSegment>());
                }
            }
        }
        return segments;
    }

    List<CollisionSegment> GetSelectedSegments()
    {
        List<CollisionSegment> segments = new List<CollisionSegment>();
        Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] is GameObject)
            {
                GameObject g = (GameObject)selectedObjects[i];
                if (g.GetComponent<CollisionSegment>() != null)
                {
                    segments.Add(g.GetComponent<CollisionSegment>());
                }
            }
        }
        return segments;
    }

    private void OnDestroy()
    {
        //if (segment == null)
        //    Debug.Log("destroy");
    }

    void OnInspectorGui()
    {

        previousEvent = currentEvent;
        currentEvent = Event.current;

        if (currentEvent.type == EventType.ExecuteCommand)
        {
            Debug.Log(currentEvent);
        }
        if (currentEvent.keyCode == KeyCode.Delete)
        {
            Debug.Log("delete");
        }
    }


    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(CollisionSegment segment, GizmoType gizmo)
    {

        //Vector2 pos = vert.transform.position;
        //vert.ResetZCoord();

        if (segment.a == null || segment.b == null)
        {
            Undo.DestroyObjectImmediate(segment.gameObject);
            return;
        }
        CollisionMap map = segment.transform.gameObject.GetComponentInParent<CollisionMap>();
        Tools.hidden = false;
        Vector2 aPos = segment.a.transform.position;
        Vector2 bPos = segment.b.transform.position;


        if ((gizmo & GizmoType.Selected) != 0)
        {
            Gizmos.color = map.selectionColor;
        }
        else
        {
            //Handles.color = Color.red;
            Gizmos.color = map.segmentColor;
        }
        //Handles.DrawSolidDisc(pos, Vector3.back, .5f);
        //Gizmos.DrawSphere(pos, .4f);
        if (segment.a != null && segment.b != null)
        {
            Gizmos.DrawLine(segment.a.transform.position, segment.b.transform.position);
            Vector3 direction = segment.b.transform.position - segment.a.transform.position;
            Vector3 normalDirection = new Vector3(-direction.y, direction.x, 0).normalized;
            Gizmos.DrawLine(segment.a.transform.position + normalDirection * .1f, segment.b.transform.position + normalDirection * .1f);
            Gizmos.DrawLine(segment.a.transform.position - normalDirection * .1f, segment.b.transform.position - normalDirection * .1f);

            //draw the normal of the segment
            Gizmos.color = map.segmentNormalColor;
            Gizmos.DrawLine(segment.midPoint, segment.midPoint + segment.normal * map.segmentNormalLength);
            segment.transform.position = segment.midPoint;
        }
    }


    void OnEnable()
    {
        segment = (CollisionSegment)target;
    }
}
