using UnityEngine;
using System.Collections.Generic;

public class GNode
{
    private GNode _parent;
    private float _cost;
    private Dictionary<GoapStates, int> _states;
    private GAction _action;

    #region Getters
    public float Cost => _cost;
    public GAction Action => _action;
    public GNode Parent => _parent;
    public Dictionary<GoapStates, int> States => _states;
    #endregion

    public GNode(GNode parent, float cost, Dictionary<GoapStates, int> states, GAction action)
    {
        _parent = parent;
        _cost = cost;
        _states = states;
        _action = action;
    }
}

public class GPlanner : IServiceConsumer
{
    private GWorld _gWorld;

    public Queue<GAction> Plan(List<GAction> actions, Dictionary<GoapStates, int> goals, WorldState states)
    {
        List<GAction> usableActions = new List<GAction>();

        foreach (GAction action in actions)
            if(action.IsAchivable())
                usableActions.Add(action);

        List<GNode> leaves = new List<GNode>();

        if(!TryResolveService())
        {
            #if UNITY_EDITOR
            Debug.Log("[GPlanner] Could not resolve service");
            #endif
            return null;
        } 
    
        GNode startNode = new GNode(null, 0, _gWorld.World.States, null);
        bool success = BuildGoapGraph(startNode, leaves, usableActions, goals);
        
        if(!success)
        {
            #if UNITY_EDITOR
            Debug.Log("[GPlanner] No Plan");
            #endif
            return null;
        } 
        
        //Buscamos el GNode más barato
        GNode cheapestNode = null;
        
        foreach(GNode leaf in leaves)
        {
            if(cheapestNode == null) cheapestNode = leaf;
            
            else
            {
                if(cheapestNode.Cost < leaf.Cost) continue;
                cheapestNode = leaf;
            }
        }
        
        //Armamos una secuencia de acciones desordenada en base a cheapestNode, el cual va haciendo 
        //un SortUp hasta el nodo padre de todo.
        List<GAction> result = new List<GAction>();
        GNode n = cheapestNode;
        
        while(n != null)
        {
            if(n.Action != null)
                result.Insert(0, n.Action);
            
            n = n.Parent;
        }

        //Damos orden a la secuencia de acciones con una Queue
        Queue<GAction> queue = new Queue<GAction>();
        
        foreach(GAction item in result)
            queue.Enqueue(item);
        
        #if UNITY_EDITOR
        Debug.Log("The plan is:");

        foreach(GAction item in queue)
            Debug.Log($"Q: {item.actionName}");
        #endif

        return queue;
    }

    private bool BuildGoapGraph(GNode parent, List<GNode> leaves, 
                                List<GAction> usableActions, Dictionary<GoapStates, int> goals)
    {
        bool foundPath = false;

        foreach(GAction action in usableActions)
        {
            if(!action.IsAchievableGiven(parent.States)) continue;

            Dictionary<GoapStates, int> currentStates = new Dictionary<GoapStates, int>(parent.States);

            foreach(KeyValuePair<GoapStates, int> item in action.effectsDic)
            {
                    if(currentStates.ContainsKey(item.Key)) continue;

                    currentStates.Add(item.Key, item.Value);
            }

            GNode node = new GNode(parent, parent.Cost + action.Cost, currentStates, action);

            if(GoalAchieved(goals, currentStates))
            {
                leaves.Add(node);
                foundPath = true;
            }

            else
            {
                List<GAction> subset = ActionSubSet(usableActions, action);
                bool found = BuildGoapGraph(node, leaves, subset, goals);

                if(found) foundPath = true;
            }
            
        }

        return foundPath;
    }

    private bool GoalAchieved(Dictionary<GoapStates, int> goals, Dictionary<GoapStates, int> states)
    {
        foreach(KeyValuePair<GoapStates, int> item in goals)
            if(!states.ContainsKey(item.Key)) 
                return false;
        
        return true;
    }

    private List<GAction> ActionSubSet(List<GAction> actions, GAction actionToRemove)
    {
        List<GAction> subset = new List<GAction>();

        foreach(GAction item in actions)
            if(!item.Equals(actionToRemove))
                subset.Add(item);

        return subset;
    }

    public bool TryResolveService()
    {
        if(_gWorld != null) return true;

        return ServiceLocator.Instance.TryGet(out _gWorld);
    }
}
