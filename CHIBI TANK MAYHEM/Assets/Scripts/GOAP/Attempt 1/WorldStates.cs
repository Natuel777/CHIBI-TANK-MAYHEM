using UnityEngine;
using System.Collections.Generic;

public enum GoapStates
{
    Idle
}

// Se usa a modo de diccionario serializado para setear las condiciones de mundo predefinidas para cada acción de GAgent
[System.Serializable]
public class WorldState
{
    public GoapStates key;
    public int value;
}

public class WorldStates 
{
    private Dictionary<GoapStates, int> _states;

    public Dictionary<GoapStates, int> States => _states;

    public WorldStates()
    {
        _states = new Dictionary<GoapStates, int>();
    }

    public void Add(GoapStates key, int value) => _states.Add(key, value);

    public void ModifyState(GoapStates key, int value)
    {
        if(_states.ContainsKey(key))
        {
            _states[key] += value;

            if(_states[key] <= 0) Remove(key);
        } 
    }

    public bool Contains(GoapStates state) => _states.ContainsKey(state);

    public bool Remove(GoapStates state)
    {
        if(_states.ContainsKey(state))
        {
            _states.Remove(state);
            return true;
        }         

        return false;
    }

    public void SetState(GoapStates key, int value)
    {
        if(_states.ContainsKey(key))
            _states[key] = value;

        else _states.Add(key, value);
    }
}
