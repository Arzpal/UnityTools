using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeCreator))]
public class ShapeEditor : Editor
{
    ShapeCreator shapeCreator;
    SelectionInfo selectionInfo;
    bool needsRepaint;
    
    void OnSceneGUI() {
        try
        {
            if(shapeCreator.points.Count == 0) {}
        }
        catch (System.Exception e)
        {
            shapeCreator = (ShapeCreator)target;
        }

        try
        {
            if(selectionInfo.pointIsSelected) {}
        }
        catch (System.Exception e)
        {
            selectionInfo = new SelectionInfo();
        }

        Event guiEvent = Event.current;

        if(guiEvent.type == EventType.Repaint){
            Draw();
        }
        else if(guiEvent.type == EventType.Layout) {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else{
            HandleInput(guiEvent);
            if(needsRepaint){
                HandleUtility.Repaint();
                needsRepaint = false;
            }
        }
    }

    void HandleInput(Event guiEvent){
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeigth = 0;
        float distToDrawPlane = (drawPlaneHeigth - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(distToDrawPlane);

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None){
            Undo.RecordObject(shapeCreator, "Add Point");
            shapeCreator.points.Add(mousePosition);
            Debug.Log("add:" + mousePosition);
            needsRepaint = true;
        }

        UpdateMouseOverSelection(mousePosition);
    }

    void Draw(){
        for (int i = 0; i < shapeCreator.points.Count; i++)
        {
            Vector3 nextPoint = shapeCreator.points[(i + 1) % shapeCreator.points.Count];
            Handles.color = Color.yellow;
            Handles.DrawDottedLine(shapeCreator.points[i], nextPoint, 4);

            if(i == selectionInfo.pointIndex){
                Handles.color = Color.red;
            }
            else {
                Handles.color = Color.white;
            }
            Handles.DrawSolidDisc(shapeCreator.points[i], Vector3.up, shapeCreator.handleRadius);
        }
        needsRepaint = false;
    }

    void UpdateMouseOverSelection(Vector3 mousePosition){
        int mouseOverPointIndex = -1;
        for (int i = 0; i < shapeCreator.points.Count; i++)
        {
            if(Vector3.Distance(mousePosition, shapeCreator.points[i]) < shapeCreator.handleRadius){
                mouseOverPointIndex = i;
                break;
            }
        }
        if(mouseOverPointIndex != selectionInfo.pointIndex){
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

            needsRepaint = true;
        }
    }

    void OnEnable() {
        shapeCreator = (ShapeCreator)target;
        selectionInfo = new SelectionInfo();
    }

    public class SelectionInfo{
        public int pointIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;
    }
}
