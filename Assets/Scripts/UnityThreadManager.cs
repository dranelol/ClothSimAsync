using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Describes a single action
/// </summary>
public struct QueuedAction : IComparable
{
    /// <summary>
    /// Name of the action
    /// </summary>
    public string name;

    /// <summary>
    /// Action to be queued
    /// </summary>
    public Action action;

    /// <summary>
    /// Decides when this action will take place
    /// </summary>
    public float activateTime;

    public int CompareTo(object obj)
    {
        QueuedAction other = (QueuedAction)obj;

        return activateTime.CompareTo(other.activateTime);
    }
}

public class UnityThreadManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for the thread manager
    /// </summary>
    //private static UnityThreadManager instance;

    /// <summary>
    /// Description: Public access for the singleton instance for the thread manager
    /// Operation: If the instance has not be initialized yet, initialize will be called. 
    /// In all cases, return the instance of the thread manager.
    /// Returns: Instance of this thread manager
    /// </summary>
    /// 
    /*
    public static UnityThreadManager Instance
    {
        get
        {
            if (instance == null)
            {
                //instance = new UnityThreadManager();
                Initialize();
               
            }

            return instance;
        }
    }
    */
    private static List<Action> actions;
    private static List<Action> currentActions;
    private static List<QueuedAction> delayedActions; 
    private static List<QueuedAction> currentDelayedActions; 

    private int maxThreads = 32;
    private int currentThreads;
    public int CurrentThreads
    {
        get
        {
            return currentThreads;
        }
    }


    public void QueueAction(Action action, float time)
    {
        if (time == 0)
        {
            lock (currentActions)
            {
                currentActions.Add(action);
            }
        }

        else
        {
            lock (delayedActions)
            {
                QueuedAction newQueuedAction = new QueuedAction();
                newQueuedAction.name = "";
                newQueuedAction.action = action;
                newQueuedAction.activateTime = Time.time + time;
                delayedActions.Add(newQueuedAction);
            }
        }
    }


    private void Initialize()
    {
        currentThreads = 0;
        actions = new List<Action>();
        currentActions = new List<Action>();
        delayedActions = new List<QueuedAction>();
        currentDelayedActions = new List<QueuedAction>();
    }

    public Thread Run(Action action)
    {
        while (currentThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }

        // semaphore p()
        Interlocked.Increment(ref currentThreads);

        // critical section
        ThreadPool.QueueUserWorkItem(DoAction, action);

        // semaphore v()
        Interlocked.Decrement(ref currentThreads);

        return null;
    }

    private void DoAction(object action)
    {
        // action has to be passed as an object here, so cast as an Action and run it
        Action runAction = (Action)action;
        runAction();
    }


	private void Awake () 
    {
        Initialize();
	}
	
	private void Update () 
    {
        lock (actions)
        {
            currentActions.Clear();
            currentActions.AddRange(actions);
            actions.Clear();
        }

        foreach (Action action in currentActions)
        {
            action();
        }

        lock (delayedActions)
        {
            // clear current delayed actions
            currentDelayedActions.Clear();

            // search through all queued actions, if its time to activate is less than or equal to the current time, add them to the current list
            foreach (QueuedAction delayedAction in delayedActions)
            {
                if (delayedAction.activateTime <= Time.time)
                {
                    currentDelayedActions.Add(delayedAction);
                }
            }

            foreach (QueuedAction currentDelayedAction in currentDelayedActions)
            {
                delayedActions.Remove(currentDelayedAction);
            }
        }

        foreach (QueuedAction currentDelayedAction in currentDelayedActions)
        {
            currentDelayedAction.action();
        }


	}
}
