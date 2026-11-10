using System.Collections;
using UnityEngine;

// Script de prueba: le pide un camino al PathRequestManager y lo dibuja con gizmos.
public class PathRequestTester : MonoBehaviour
{
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _target;
    [SerializeField] private bool _requestOnStart = true;

    private Vector3[] _path;

    // Se espera un frame para que el Grid ya haya creado sus nodos en su Start.
    private IEnumerator Start()
    {
        yield return null;

        if(_requestOnStart) RequestPath();
    }

    // Menú del componente (⋮) -> "Pedir camino". Sirve para repetir el pedido después de mover _start o _target.
    [ContextMenu("Pedir camino")]
    private void RequestPath()
    {
        if(!Application.isPlaying)
        {
            Debug.LogWarning("[PathRequestTester] Solo funciona en Play.");
            return;
        }

        if(_start == null || _target == null)
        {
            Debug.LogWarning("[PathRequestTester] Falta asignar _start o _target.");
            return;
        }

        if(!ServiceLocator.Instance.TryGet(out PathRequestManager manager)) return;

        _path = null;
        manager.RequestPath(_start.position, _target.position, OnPathFound);
    }

    private void OnPathFound(Vector3[] path, bool success)
    {
        if(!success)
        {
            Debug.LogWarning("[PathRequestTester] No hay camino: inicio o destino sobre un obstáculo, o sin ruta entre ambos.");
            return;
        }

        _path = path;

        // El último waypoint es el centro del nodo del destino: tiene que quedar a menos de un nodo del target.
        float distanceToTarget = path.Length > 0 ? Vector3.Distance(path[path.Length - 1], _target.position) : -1f;
        Debug.Log($"[PathRequestTester] Camino encontrado: {path.Length} waypoints. El último queda a {distanceToTarget:F2} del destino.");
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(_start != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_start.position, 0.5f);
        }

        if(_target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_target.position, 0.5f);
        }

        if(_path == null || _start == null) return;

        Gizmos.color = Color.yellow;
        Vector3 previous = _start.position;

        foreach(Vector3 waypoint in _path)
        {
            Gizmos.DrawLine(previous, waypoint);
            Gizmos.DrawCube(waypoint, Vector3.one * 0.4f);
            previous = waypoint;
        }
    }
    #endif
}
