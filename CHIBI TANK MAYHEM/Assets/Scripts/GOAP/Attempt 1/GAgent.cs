using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class SubGoal
{
    public Dictionary<GoapStates, int> subGoals;
    public bool remove;

    public SubGoal(GoapStates key, int value, bool remove)
    {
        subGoals = new  Dictionary<GoapStates, int>();
        subGoals.Add(key, value);
        this.remove = remove;
    }
}

public class GAgent : MonoBehaviour, IServiceConsumer
{
    #region GOAP
    public List<GAction> actions = new List<GAction>();
    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();
    private GPlanner _planner;
    private Queue<GAction> actionsQueue;
    public GAction _currentAction;
    private SubGoal _currentGoal;
    private bool _arrived;
    private float _timer;
    [SerializeField] private float _replanDelay = 1f;
    private float _nextPlanTime;
    #endregion

    #region A*
    private PathRequestManager _aStarManager;
    private const float _pathUpdateThreshold = 0.5f, _minPathUpdateTime = 0.2f;
    public float speed = 5f, turnDistance = 5f;
    [SerializeField] private float _turnSpeed = 0f, _stoppingDistance = 10f;
    private SmoothPath _path;
    private Coroutine _followCoroutine; 
    public Transform target;
    #endregion

    private void Start() 
    {
        GAction[] acts = GetComponents<GAction>();

        foreach(GAction item in acts)
            actions.Add(item);

        StartCoroutine(UpdatePath());
    }

    #region GOAP Logic
    private void CompleteAction()
    {
        _currentAction.executing = false;
        _currentAction.PostPerform();
    }

    // Arranca la acción actual: si tiene destino pide el camino, si no, se ejecuta en el lugar.
    private void StartAction()
    {
        _timer = 0f;
        _arrived = false;

        if(_currentAction.target == null && !string.IsNullOrEmpty(_currentAction.TargetTag))
        {
            GameObject targetObject = GameObject.FindWithTag(_currentAction.TargetTag);

            if(targetObject != null)
                _currentAction.target = targetObject.transform;
        }

        // Sin target ni tag la acción no necesita desplazarse.
        if(_currentAction.target == null && string.IsNullOrEmpty(_currentAction.TargetTag))
        {
            _currentAction.executing = true;
            _arrived = true;
            return;
        }

        // Si hay tag pero ningún objeto lo tiene, o no se resuelve el servicio de A*, no hay a dónde ir.
        if(_currentAction.target == null || !TryResolveService())
        {
            AbortPlan();
            return;
        }

        _currentAction.executing = true;
        _aStarManager.RequestPath(transform.position, _currentAction.target.position, OnPathFound);
    }

    // Cancela la acción y el plan actual. El agente vuelve a planificar después de _replanDelay segundos.
    private void AbortPlan()
    {
        Debug.LogWarning($"[GAgent] Plan cancelado en la acción {_currentAction.actionName}");

        StopFollowing();
        _currentAction.executing = false;
        _timer = 0f;
        actionsQueue = null;
        _nextPlanTime = Time.time + _replanDelay;
    }

    private void LateUpdate() 
    {
        // Ejecutamos la acción actual
        if(_currentAction != null && _currentAction.executing)
        {
            // El timer de la acción solo corre cuando el agente ya llegó a su destino.
            if(_arrived)
            {
                if(_timer >= _currentAction.Duration)
                {
                    CompleteAction();
                    _timer = 0f;
                }

                else _timer += Time.deltaTime;
            }

            return;
        }

        // Después de cancelar un plan se espera un momento antes de volver a planificar.
        if(Time.time < _nextPlanTime) return;

        // Creamos un plan para el agente y lo metemos en queue
        if(_planner == null || actionsQueue == null)
        {
            _planner = new GPlanner();
            var sortedGoals = goals.OrderByDescending(entry => entry.Value).Select(entry => entry);

            foreach(KeyValuePair<SubGoal, int> item in sortedGoals)
            {
                actionsQueue = _planner.Plan(actions, item.Key.subGoals, null);

                if(actionsQueue != null)
                {
                    _currentGoal = item.Key;
                    break;
                }
            }
        }

        if(actionsQueue != null && actionsQueue.Count == 0)
        {
            if(_currentGoal.remove)
                goals.Remove(_currentGoal);
        
            _planner = null;
        }

        if(actionsQueue != null && actionsQueue.Count > 0)
        {
            _currentAction = actionsQueue.Dequeue();

            if(_currentAction.PrePerform())
                StartAction();

            else actionsQueue = null;
        }
    }
    #endregion

    #region A* Logic
    private void OnPathFound(Vector3[] waypoints, bool success)
    {
        // El agente se desactivó o la acción se canceló mientras esperábamos el camino: se ignora el resultado.
        if(this == null || !isActiveAndEnabled || _currentAction == null || !_currentAction.executing) return;

        if(!success)
        {
            AbortPlan();
            return;
        }

        // Camino vacío: no hay nada que recorrer, el agente ya está en el destino.
        if(waypoints.Length == 0)
        {
            _arrived = true;
            return;
        }

        _path = new SmoothPath(waypoints, transform.position, turnDistance, _stoppingDistance);
        StopFollowing();
        _followCoroutine = StartCoroutine(FollowPath());
    }

    private void StopFollowing()
    {
        if(_followCoroutine == null) return;

        StopCoroutine(_followCoroutine);
        _followCoroutine = null;
    }

    private IEnumerator UpdatePath()
    {
        // En un iterador (IEnumerator) no se puede usar "return;": para terminar la corrutina se usa "yield break;".
        if(!TryResolveService()) yield break;

        if(Time.timeSinceLevelLoad < 0.3f)
            yield return new WaitForSeconds(0.3f);

        _aStarManager.RequestPath(transform.position, target.position, OnPathFound);
        float sqrThreshold = _pathUpdateThreshold * _pathUpdateThreshold;
        Vector3 previousTargetPos = target.position;

        while(true)
        {
            if(target.position == previousTargetPos) break;

            yield return new WaitForSeconds(_minPathUpdateTime);

            if((target.position - previousTargetPos).sqrMagnitude > sqrThreshold)
            {
                _aStarManager.RequestPath(transform.position, target.position, OnPathFound);
                previousTargetPos = target.position;
            }  
        }
    }

    private IEnumerator FollowPath()
    {
        bool followPath = true;
        int pathIndex = 0;
        transform.LookAt(_path.lookPoints[0]);
        float speedPercent = 1;

        while(followPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            // Si ya recorrió todos los nodos / lines, termina. Esto, en clases que hereden de GAgent, debería hacerse mediante
            // base.FollowPath()
            while(_path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if(pathIndex == _path.finishLineIndex)
                {
                    followPath = false;
                    break;
                }
                
                else pathIndex++;
            }

            // Rotamos y movemos el agente hacia el nodo / line objetivo
            // Esto en realidad lo haran las clases que hereden de GAgent, con otra lógica aplicada
            if(followPath)
            {
                if(pathIndex >= _path.slowDownIndex && _stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01(_path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(pos2D) / _stoppingDistance); 

                    if(speedPercent < 0.01f) followPath = false;
                }
                    
                Quaternion targetRotation = Quaternion.LookRotation(_path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos() 
    {
        if(_path != null)
            _path.ArtificialOnDrawGizmos();
    }
    #endif
    #endregion

    public bool TryResolveService()
    {
        if(_aStarManager != null) return true;

        return ServiceLocator.Instance.TryGet(out _aStarManager);
    }
}
