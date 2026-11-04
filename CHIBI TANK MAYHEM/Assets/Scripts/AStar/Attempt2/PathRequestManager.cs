using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour
{
    private Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    private PathRequest _current;
    private AStarPathfinder _pathfinder;
    private bool _isProcessing;
    private static PathRequestManager _instance;

    private void Awake()
    {
        ServiceLocator.Instance.Register<PathRequestManager>(this);
        _pathfinder = GetComponent<AStarPathfinder>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        _instance._pathRequestQueue.Enqueue(newRequest);
        _instance.TryProcessNext();
    }

    private void TryProcessNext()
    {
        if(_isProcessing || _pathRequestQueue.Count <= 0) return;

        _current = _pathRequestQueue.Dequeue();
        _isProcessing = true;
        _pathfinder.StartFindPath(_current.pathStart,
                                _current.pathEnd);
    }

    public void FinishProcessingPath(Vector3[] path, bool success)
    {
        _current.callback(path, success);
        _isProcessing = false;
        TryProcessNext();
    }

    struct PathRequest
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
}
