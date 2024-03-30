using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using StateDict = System.Collections.Generic.Dictionary<string, GState>;

public abstract class GAction
{
    public StateDict Effect;
    public StateDict Preconditions;
    public int Cost;

    public virtual bool CheckPreconditions(StateDict cur_state)
    {
        foreach (string key in Preconditions.Keys)
        {
            if (!cur_state.ContainsKey(key))
                return false;

            if (cur_state[key].CheckState(Preconditions[key]))
            {
                return false;
            }
        }

        return true;
    }

    public virtual bool CheckEffect(StateDict cur_goal)
    {
        foreach (string key in Effect.Keys)
        {
            if (!cur_goal.ContainsKey(key))
            {
                continue;
            }

            if (cur_goal[key].CheckState(Effect[key]))
            {
                return false;
            }
        }

        return true;
    }

    public virtual bool IsValid(StateDict cur_state, StateDict cur_goal)
    {
        return CheckPreconditions(cur_state) && CheckEffect(cur_goal);
    }

    public virtual void Perform()
    {
        Debug.Log("Action perfoming...");
    }
}
