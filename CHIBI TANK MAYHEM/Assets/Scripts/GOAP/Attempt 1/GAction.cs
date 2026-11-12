using UnityEngine;
using System.Collections.Generic;

public abstract class GAction : MonoBehaviour
{
    public GoapStates actionName;
    public Transform target;
    [SerializeField] private float _cost = 1f, _duration;
    [SerializeField] private WorldState[] _preconditions, _afterEffects;
    public bool executing = false;
    [SerializeField] private string _targetTag = string.Empty;
    public Dictionary<GoapStates, int> preconditionsDic, effectsDic;
    public WorldStates agentBeliefs;
    
    #region Getters
    public float Cost => _cost;
    public float Duration => _duration;
    public string TargetTag => _targetTag;
    #endregion

    public GAction()
    {
        preconditionsDic = new Dictionary<GoapStates, int>();
        effectsDic = new Dictionary<GoapStates, int>();
    }

    private void Awake() 
    {
        if(preconditionsDic != null)
        {
            foreach(var item in _preconditions)
                preconditionsDic.Add(item.key, item.value);
        }

        if(effectsDic != null)
        {
            foreach(var item in _afterEffects)
                effectsDic.Add(item.key, item.value);
        }    
    }

    public bool IsAchivable() => true;

    public bool IsAchievableGiven(Dictionary<GoapStates, int> conditions)
    {
        foreach(KeyValuePair<GoapStates, int> item in preconditionsDic)
            if(!conditions.ContainsKey(item.Key))
                return false;
        
        return true;
    }

    public abstract bool PrePerform();

    public abstract bool PostPerform();
}
