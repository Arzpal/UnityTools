using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

[System.Serializable]
public class Path{
    [SerializeField, HideInInspector]
    public List<Vector2> points;//remove public
    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool autoSetControlPoints;

    public Path(Vector2 center) {
        points = new List<Vector2>{
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + (Vector2.right + Vector2.down) * 0.5f,
            center + Vector2.right
        };
    }

    public Vector2 this[int i] 
    {
        get => points[i];
    }

    public bool AutoSetControlPoints{
        get => autoSetControlPoints;
        set{
            if(autoSetControlPoints != value){
                autoSetControlPoints = value;
                if(autoSetControlPoints) {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public int NumPoints
    {
        get => points.Count;
    }

    public int NumSegments
    {
        get => points.Count / 3; 
    }

    public void AddSegment(Vector2 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);

        if(autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(points.Count - 1);
        }
    }

    public Vector2[] GetPointsInSegment(int i) {
        return new Vector2[]{
            points[i * 3],
            points[i * 3 + 1],
            points[i * 3 + 2],
            points[LoopIndex(i * 3 + 3)], //to go all the way around the list
        };
    }

    public void MovePoint(int i, Vector2 pos) 
    {
        Vector2 deltaMove = pos - points[i];
        if(i % 3 == 0 || !autoSetControlPoints)
        {
            points[i] = pos;
        
            if(autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(i);
            }
            else
            {
                if(i % 3 == 0)
                {
                    if(i + 1 < points.Count || isClosed)
                        points[LoopIndex(i + 1)] += deltaMove;
                    if(i - 1 >= 0 || isClosed)
                        points[LoopIndex(i - 1)] += deltaMove;
                }
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int otherIndex = nextPointIsAnchor ? i + 2 : i - 2;
                    int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;
    
                    if(otherIndex >= 0 && otherIndex < points.Count || isClosed)
                    {
                        float dist = (points[LoopIndex(anchorIndex)] - points[LoopIndex(otherIndex)]).magnitude;
                        Vector2 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;
                        points[LoopIndex(otherIndex)] = points[LoopIndex(anchorIndex)] + dir * dist;
                    }
                }
            }
        }
    }
    
    public void ToggleClosed()
    {
        isClosed = !isClosed;
        if(isClosed)
        {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add(points[0] * 2 - points[1]);
            if(autoSetControlPoints) 
            {
                AutoSetAnchorControlPoints(0);
                AutoSetAnchorControlPoints(points.Count -3);
            }
        }
        else{
            points.RemoveRange(points.Count - 2, 2);
            if(autoSetControlPoints)
                AutoSetStartAndEndControls();
        }
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControls();
    }

    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i < updatedAnchorIndex + 3; i += 3)
        {
            if(i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControls();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDists = new float[2];

        if(anchorIndex - 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDists[0] = offset.magnitude;
        }
        if(anchorIndex + 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDists[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDists[i] * 0.5f;
        }
    }

    void AutoSetStartAndEndControls()
    {
        if(!isClosed)
        {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }

    int LoopIndex(int i) => (i + points.Count) % points.Count;
}
