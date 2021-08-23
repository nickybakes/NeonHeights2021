using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad, CustomEditor(typeof(CollisionMap))]
public class CollisionMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //Debug.Log("OnInspector");

        //EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("Add ur mom"))
        //{
        //    Debug.Log("add");
        //}
        //if (GUILayout.Button("Del ur mom"))
        //{
        //    Debug.Log("del");

        //}
        //EditorGUILayout.EndHorizontal();
    }

    [MenuItem("MyMenu/Do Something")]
    private static void OnScene()
    {
        Debug.Log("OnScene");

    }

    private void OnSceneGUI()
    {
        //Debug.Log("www");
    }

    public static Event currentEvent;
    public static Event previousEvent;

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(CollisionMap map, GizmoType gizmo)
    {
        if (map.vertices.Count == 0)
        {
            map.Init();
        }

        //Debug.Log("update");
        if (map.editorUpdateFrame == 0)
        {
            Input(map);
            map.editorUpdateFrame++;
        }
        else
        {
            map.editorUpdateFrame = 0;
        }

        if (map.mirrorHorizontal)
        {
            List<CollisionVertex> vertices = map.vertices;
            List<CollisionSegment> segments = map.segments;

            Handles.color = map.vertexColor * .7f;
            foreach(CollisionVertex v in vertices)
            {
                Handles.DrawSolidDisc(new Vector3(-v.transform.position.x, v.transform.position.y, 0), Vector3.back, .3f);
            }

            foreach (CollisionSegment s in segments)
            {
                Handles.color = map.segmentColor * .7f;
                Vector3 a = new Vector3(-s.a.transform.position.x, s.a.transform.position.y, 0);
                Vector3 b = new Vector3(-s.b.transform.position.x, s.b.transform.position.y, 0);
                Handles.DrawLine(a, b, 2);
                Vector3 direction = b - a;
                Vector3 normalDirection = new Vector3(-direction.y, direction.x, 0).normalized;
                Handles.DrawLine(a + normalDirection * .1f, b + normalDirection * .1f, 2);
                Handles.DrawLine(a - normalDirection * .1f, b - normalDirection * .1f, 2);

                //draw the normal of the segment
                Handles.color = map.segmentNormalColor * .7f;
                Vector2 normal = new Vector2(-s.normal.x, s.normal.y);
                Handles.DrawLine(Vector2.Lerp(a, b, .5f), Vector2.Lerp(a, b, .5f) + normal * map.segmentNormalLength, 2);
            }
        }
    }

    //private void OnSceneGUI()
    //{
    //    Draw();
    //}

    static void Input(CollisionMap map)
    {
        previousEvent = currentEvent;
        currentEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition).origin;

        //SceneView.duringSceneGui += view =>
        //{
        //    var e = Event.current;
        //    if (e != null)
        //    {
        //        if (e.commandName == "Delete" || e.commandName == "SoftDelete")
        //        {
        //            Debug.Log("delete00");
        //        }
        //    }
        //};
        if (currentEvent.type == EventType.ExecuteCommand)
        {
            Debug.Log(currentEvent);

        }
        if (currentEvent.keyCode == KeyCode.Delete)
        {
            //Debug.Log("delete");
        }
        if (currentEvent.type == EventType.MouseUp && currentEvent.button == 2 && currentEvent.shift && currentEvent.control)
        {
            if (ExtrudeVertices(map, mousePos))
            {
                currentEvent.Use();
                return;
            }
        }
    }

    public static bool ExtrudeVertices(CollisionMap map, Vector2 mousePos)
    {
        List<CollisionVertex> selectedVerts = GetSelectedVerticesInMap(map);
        if (selectedVerts.Count > 0)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Extrude Vertices");
            int undoGroupIndex = Undo.GetCurrentGroup();

            List<GameObject> newVerts = new List<GameObject>();
            Vector2 posDifference = mousePos - (Vector2)selectedVerts[selectedVerts.Count - 1].transform.position;
            foreach (CollisionVertex v in selectedVerts)
            {
                List<CollisionSegment> segmentsAttachedToThisVertex = map.GetSegmentsAttachedToVertex(v);
                CollisionVertex newVert = map.AddVertex((Vector2)v.transform.position + posDifference);
                Undo.RegisterCreatedObjectUndo(newVert, "vert");
                CollisionSegment newSeg = map.AddSegment(v, newVert);
                Undo.RegisterCreatedObjectUndo(newSeg, "segment");
                newVerts.Add(newVert.gameObject);
            }

            Selection.objects = newVerts.ToArray();
            Undo.CollapseUndoOperations(undoGroupIndex);
            return true;
        }
        return false;
    }

    static List<CollisionVertex> GetSelectedVerticesInMap(CollisionMap map)
    {
        List<CollisionVertex> verts = new List<CollisionVertex>();
        Object[] selectedObjects = Selection.objects;
        for(int i = 0; i < selectedObjects.Length; i++)
        {
            if(selectedObjects[i] is GameObject)
            {
                GameObject g = (GameObject)selectedObjects[i];
                if (g.GetComponent<CollisionVertex>() != null && map.vertices.Contains(g.GetComponent<CollisionVertex>()))
                {
                    verts.Add(g.GetComponent<CollisionVertex>());
                }
            }
        }
        return verts;
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


    //void Draw()
    //{
    //    Handles.color = Color.red;
    //    for (int i = 0; i < map.vertices.Count; i++)
    //    {
    //        //Vector2 pos = map.vertices[i].pos;
    //        //Handles.CylinderHandleCap(0, pos, Quaternion.identity, 5, EventType.MouseDown);
    //        //Vector2 newPos = Handles.PositionHandle(pos, Quaternion.identity);
    //        //if (pos != newPos)
    //        //{
    //        //    map.vertices[i].pos = newPos;
    //        //}
    //    }
    //}

    void OnEnable()
    {


    }
}
