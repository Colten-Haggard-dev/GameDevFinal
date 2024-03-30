using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using StateDict = System.Collections.Generic.Dictionary<string, GState>;

public struct GState
{
    public object value;
    public Type type;

    public readonly bool CheckState(GState state)
    {
        object s_value = state.value;
        Type s_type = state.type;

        return s_value.ConvertTo(s_type) == value.ConvertTo(type);
    }

    public GState(object value, Type type)
    {
        this.value = value;
        this.type = type;

        Debug.Log(value.GetType());
    }
}

public struct GGoal
{
    public readonly string Name;
    public int Priority;
    public StateDict DesiredState;

    public GGoal(string name, int priority, StateDict state)
    {
        Name = name;
        Priority = priority;
        DesiredState = state;
    }

    public readonly bool IsSatisfied(StateDict state)
    {
        int i = 0;

        foreach (string key in DesiredState.Keys)
        {
            if (!state.ContainsKey(key))
            {
                continue;
            }

            if (state[key].CheckState(DesiredState[key]))
            {
                i++;
            }
        }

        return i == DesiredState.Count;
    }
}
