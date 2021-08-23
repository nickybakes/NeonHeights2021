using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad, CanEditMultipleObjects, CustomEditor(typeof(CollisionVertex))]
public class CollisionVertexEditor : Editor
{
    public Event currentEvent;
    public Event previousEvent;
    public CollisionVertex vertex;

    public void OnSceneGUI()
    {
        //Debug.Log("aawadd");
        previousEvent = currentEvent;
        currentEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition).origin;

        if (currentEvent.type == EventType.ExecuteCommand)
        {
            //Debug.Log(currentEvent);
        }
        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Alpha3 && !currentEvent.shift && !currentEvent.control && !currentEvent.alt)
        {
            CollisionMap map = vertex.transform.gameObject.GetComponentInParent<CollisionMap>();

            List<CollisionVertex> selectedVerts = GetSelectedVerticesInMap(map);
            List<CollisionVertex> allVerts = map.vertices;

            CollisionVertex closestVertexToMouse = null;
            foreach (CollisionVertex v in allVerts)
            {
                if (!selectedVerts.Contains(v))
                {
                    if (closestVertexToMouse == null || Mathf.Abs(mousePos.x - v.transform.position.x) < Mathf.Abs(mousePos.x - closestVertexToMouse.transform.position.x))
                    {
                        closestVertexToMouse = v;
                    }
                }
            }
            foreach (CollisionVertex v in selectedVerts)
            {
                if(v.transform.position.x != closestVertexToMouse.transform.position.x)
                {
                    Undo.RecordObject(v.gameObject.transform, "Align Horizontally");
                    v.transform.position = new Vector3(closestVertexToMouse.transform.position.x, v.transform.position.y, 0);
                }
            }
        }
        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Alpha4 && !currentEvent.shift && !currentEvent.control && !currentEvent.alt)
        {
            CollisionMap map = vertex.transform.gameObject.GetComponentInParent<CollisionMap>();

            List<CollisionVertex> selectedVerts = GetSelectedVerticesInMap(map);
            List<CollisionVertex> allVerts = map.vertices;

            CollisionVertex closestVertexToMouse = null;
            foreach (CollisionVertex v in allVerts)
            {
                if (!selectedVerts.Contains(v))
                {
                    if (closestVertexToMouse == null || Mathf.Abs(mousePos.y - v.transform.position.y) < Mathf.Abs(mousePos.y - closestVertexToMouse.transform.position.y))
                    {
                        closestVertexToMouse = v;
                    }
                }
            }
            foreach (CollisionVertex v in selectedVerts)
            {
                if (v.transform.position.y != closestVertexToMouse.transform.position.y)
                {
                    Undo.RecordObject(v.gameObject, "Align Vertically");
                    v.transform.position = new Vector3(v.transform.position.x, closestVertexToMouse.transform.position.y, 0);
                }
            }
        }



        if (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.G && !currentEvent.shift && !currentEvent.control && !currentEvent.alt)
        {
            CollisionMap map = vertex.transform.gameObject.GetComponentInParent<CollisionMap>();

            List<CollisionVertex> selectedVerts = GetSelectedVerticesInMap(map);
            for (int i = 0; i < selectedVerts.Count - 1; i++)
            {
                bool segmentExists = false;
                foreach (CollisionSegment segment in map.segments)
                {
                    if ((segment.a == selectedVerts[i] && segment.b == selectedVerts[i + 1]) || (segment.a == selectedVerts[i + 1] && segment.b == selectedVerts[i]))
                    {
                        segmentExists = true;
                    }
                }
                if (!segmentExists)
                {
                    CollisionSegment segmentAdded = map.AddSegment(selectedVerts[i], selectedVerts[i + 1]);
                    Undo.RegisterCreatedObjectUndo(segmentAdded.gameObject, "Fill Gap");
                }
            }
        }
        if (currentEvent.commandName == "Delete" || currentEvent.commandName == "SoftDelete" && vertex != null)
        {
            currentEvent.Use();
            CollisionMap map = vertex.transform.gameObject.GetComponentInParent<CollisionMap>();
            List<CollisionVertex> verticesToDelete = GetSelectedVerticesInMap(map);
            List<CollisionSegment> segmentsAttachedToThisVertex = new List<CollisionSegment>();
            foreach (CollisionSegment segment in map.segments)
            {
                if (segment != null && (verticesToDelete.Contains(segment.a) || verticesToDelete.Contains(segment.b)))
                {
                    segmentsAttachedToThisVertex.Add(segment);
                }
            }
            foreach (CollisionSegment segment in segmentsAttachedToThisVertex)
            {
                Undo.DestroyObjectImmediate(segment.gameObject);
            }

            foreach (CollisionVertex v in verticesToDelete)
            {
                Undo.DestroyObjectImmediate(v.gameObject);
            }
        }
    }

    static List<CollisionVertex> GetSelectedVertices()
    {
        List<CollisionVertex> verts = new List<CollisionVertex>();
        Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] is GameObject)
            {
                GameObject g = (GameObject)selectedObjects[i];
                if (g.GetComponent<CollisionVertex>() != null)
                {
                    verts.Add(g.GetComponent<CollisionVertex>());
                }
            }
        }
        return verts;
    }

    static List<CollisionVertex> GetSelectedVerticesInMap(CollisionMap map)
    {
        List<CollisionVertex> verts = new List<CollisionVertex>();
        Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] is GameObject)
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


    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(CollisionVertex vert, GizmoType gizmo)
    {
        CollisionMap map = vert.transform.gameObject.GetComponentInParent<CollisionMap>();
        Vector2 pos = vert.transform.position;
        vert.ResetZCoord();

        if ((gizmo & GizmoType.Selected) != 0)
        {
            Gizmos.color = map.selectionColor;
            //Handles.color = Color.yellow;
            //Handles.CylinderHandleCap(0, pos, Quaternion.identity, 5, EventType.MouseDown);
            //Vector2 newPos = Handles.PositionHandle(pos, Quaternion.identity);
            //if (pos != newPos)
            //{
            //    vert.transform.position = newPos;
            //}


        }
        else
        {
            //Handles.color = Color.red;
            Gizmos.color = map.vertexColor;
        }
        //Handles.DrawSolidDisc(pos, Vector3.back, .5f);
        Gizmos.DrawSphere(pos, .4f);

    }


    void OnEnable()
    {
        vertex = (CollisionVertex)target;
    }
}
