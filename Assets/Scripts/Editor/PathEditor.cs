using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path path;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if(GUILayout.Button("Create new")){
            Undo.RecordObject(creator, "Create new");
            creator.CreatePath();
            path = creator.path;
        }

        if(GUILayout.Button("Toggle closed")){
            Undo.RecordObject(creator, "Toggle closed");
            path.ToggleClosed();
        }
        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if(autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set controls");
            path.AutoSetControlPoints = autoSetControlPoints;
        }

        if(EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
    }

    void OnSceneGUI(){
        Input();
        Draw();
    }

    void Input(){
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift){
            Undo.RecordObject(creator, "Add Segment");
            path.AddSegment(mousePos);
        }
    }

    void Draw(){

        //fixing path created at the start
        try 
        {
            if (path.NumPoints == path.NumPoints) { }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            creator = (PathCreator)target;
            creator.CreatePath();
            path = creator.path;
        }
        
        //Setting Lines
        Handles.color = Color.white;
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
        }

        //Setting Handles
        Handles.color = Color.red;
        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector2 newPos = Handles.FreeMoveHandle(path[i], 0.1f, Vector2.zero, Handles.CylinderHandleCap);
            if(path[i] != newPos){
                Undo.RecordObject(creator, "Move point");
                path.MovePoint(i, newPos);
            }
        }
    }
    
    void OnEnable(){
        creator = (PathCreator)target;
        if(creator.path == null) {
            creator.CreatePath();
        }
        path = creator.path;
    }
}
