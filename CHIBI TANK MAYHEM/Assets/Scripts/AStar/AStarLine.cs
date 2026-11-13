using UnityEngine;

public struct Line
{
    private const float _verticalLineGradient = 1e5f;
    private float _gradient, _yIntercept, _gradientPerpendicular;
    private Vector2 _pointOnLine_1, _pointOnLine_2;
    private bool _approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        //Calculamos el valor de la linea perperdicular a pointOnLine,
        //Donde se definirá la longitud de nuestra parabola de 
        //Smoothing
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if(dx == 0)  _gradientPerpendicular = _verticalLineGradient;

        else _gradientPerpendicular = dy / dx;

        if(_gradientPerpendicular == 0) _gradient = _verticalLineGradient;

        else _gradient = -1 / _gradientPerpendicular;

        // F(x) = mx + b reconvertido a b = f(x) - mx
        _yIntercept = pointOnLine.y - _gradient * pointOnLine.x;
        _pointOnLine_1 = pointOnLine;
        _pointOnLine_2 = pointOnLine + new Vector2(1, _gradient);
        _approachSide = false;
        _approachSide = GetSide(pointPerpendicularToLine);
    }

    private bool GetSide(Vector2 value)
    {
        return (value.x - _pointOnLine_1.x) * (_pointOnLine_2.y - _pointOnLine_1.y) > (value.y - _pointOnLine_1.y) * (_pointOnLine_2.x - _pointOnLine_1.x);
    }

    public bool HasCrossedLine(Vector2 value) => GetSide(value) != _approachSide;

    public float DistanceFromPoint(Vector2 value)
    {
        float yIntercepPerpendicular = value.y - _gradientPerpendicular * value.x;
        float intersectX = (yIntercepPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
        float intersectY = _gradient * intersectX + _yIntercept;
        return Vector2.Distance(value, new Vector2(intersectX, intersectY));
    }

    #if UNITY_EDITOR
    public void ArtificialOnDrawGizmos(float length)
    {
        Vector3 lineDirection = new Vector3(1, 0, _gradient).normalized;
        Vector3 lineCenter = new Vector3(_pointOnLine_1.x, 0, _pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine(lineCenter - lineDirection * length / 2f, lineCenter + lineDirection * length / 2f);
    }
    #endif
}

public class SmoothPath
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex, slowDownIndex;

    public SmoothPath(Vector3[] waypoints, Vector3 startPosition, float turnDistance, float stoppingDistance)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length -1; 
        Vector2 previousPoint = V3ToV2(startPosition);

        for(int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 directionToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex)? currentPoint : currentPoint - directionToCurrentPoint * turnDistance;
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - directionToCurrentPoint * turnDistance);
            previousPoint = turnBoundaryPoint;
        }

        float distanceFromEndPoint = 0;

        for(int i = lookPoints.Length -1; i > 0; i--)
        {
            distanceFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);

            if(distanceFromEndPoint > stoppingDistance)
            {
                slowDownIndex = i;
                break;
            }
        }
    }

    private Vector2 V3ToV2(Vector3 v3) =>  new Vector2(v3.x, v3.z);
    
    #if UNITY_EDITOR
    public void ArtificialOnDrawGizmos() 
    {
        Gizmos.color = Color.black;

        foreach(Vector3 item in lookPoints)
            Gizmos.DrawCube(item + Vector3.up, Vector3.one);
        
        Gizmos.color = Color.white;

        foreach(Line line in turnBoundaries)
            line.ArtificialOnDrawGizmos(10);
    }
    #endif
}
