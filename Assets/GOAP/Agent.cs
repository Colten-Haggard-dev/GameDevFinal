using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using StateDict = System.Collections.Generic.Dictionary<string, GState>;

public class Agent : MonoBehaviour
{
    private readonly StateDict CurrentState = null;
    private GGoal CurrentGoal = new("none", -1, null);
    private List<GAction> Actions = null;
    private Queue<GAction> CurrentActions = new();
    private GAction CurrentAction = null;
    private List<GGoal> Goals = null;

    public Agent()
    {
        CurrentState = new()
        {
            { "position", new GState(new Vector3(0, 0, 0), typeof(Vector3)) },
            { "health", new GState(100f, typeof(float)) },
            { "target", new GState(null, typeof(GameObject)) },
        };
    }

    public StateDict PullState()
    {
        return CurrentState;
    }

    private GGoal GetBestGoal()
    {
        GGoal best = CurrentGoal;

        foreach (GGoal goal in Goals)
        {
            if (goal.Priority > best.Priority && !goal.IsSatisfied(CurrentState))
            {
                best = goal;
            }
        }

        return best;
    }

    private void Percieve()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Percieve();
        GGoal new_goal = GetBestGoal();
        if (new_goal.Name != CurrentGoal.Name)
        {
            CurrentGoal = new_goal;
            // plan actions
            return;
        }

        if (CurrentAction.IsValid(CurrentState, CurrentGoal.DesiredState))
        {
            CurrentAction.Perform();
        }
        else if (CurrentActions.Count > 0)
        {
            CurrentAction = CurrentActions.Dequeue();
        }
        else
        {
            Debug.Log("Plan finished!");

            if (CurrentGoal.IsSatisfied(CurrentState))
                CurrentGoal.Priority = 0;
            else
                CurrentGoal.Priority++;
        }
    }
}
