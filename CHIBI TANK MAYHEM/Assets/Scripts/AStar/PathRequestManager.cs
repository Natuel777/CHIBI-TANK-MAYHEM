using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

[DefaultExecutionOrder(-400)]
public class PathRequestManager : MonoBehaviour
{
    private static PathRequestManager _instance;
    private AStarPathfinder _pathfinder;
    private Queue<PathResult> _results = new Queue<PathResult>();

    private void Awake()
    {
        _instance = this;
        ServiceLocator.Instance.Register<PathRequestManager>(this);
        _pathfinder = GetComponent<AStarPathfinder>();
    }

    private void Update() 
    {
        if(_results.Count > 0)
        {
            int itemsInQueue = _results.Count;
            lock(_results)
            {
                for(int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = _results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }

        }
    }

    public void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            _instance._pathfinder.FindPath(request, _instance.FinishProcessingPath);
        };
        threadStart.Invoke();
    }

    public void FinishProcessingPath(PathResult result)
    {
        lock(_results)
            _results.Enqueue(result);
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success; 
    public Action<Vector3[], bool> callback;
    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

public struct PathRequest
{
    public Vector3 pathStart, pathEnd;
    public Action<Vector3[], bool> callback;
    public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
    }
}
