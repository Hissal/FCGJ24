using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private const float TIME_DELTA = 0.01f;

    public bool active { get; private set; }
    public bool paused { get; private set; }
    public float endTime { get; private set; }
    public float timePaused { get; private set; }
    public float startTime { get; private set; }

    private MonoBehaviour caller;
    private System.Action<float> timerDoneAction;

    public float GetTimeLeft()
    {
        if (paused) return Time.time - timePaused + endTime - Time.time;
        return endTime - Time.time;
    }

    /// <summary>
    /// Starts the timer (Caller reference is required). Use SetTime, CheckTimer and FinishTimer if using without caller 
    /// </summary> 
    public void StartTimer(float timeInSeconds)
    {
        if (caller == null)
        {
            Debug.LogError("Timer needs a caller " + this);
            return;
        }

        StartTimer(timeInSeconds, caller);
    }
    public void StartTimer(float timeInSeconds, MonoBehaviour caller)
    {
        if (active)
        {
            float newTime = Time.time + timeInSeconds;

            if (newTime > endTime)
            {
                endTime = newTime;
            }
            return;
        }

        endTime = Time.time + timeInSeconds;
        active = true;

        try
        {
            caller.StartCoroutine(DoTimer());
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogWarning("To Start A Timer You Need A Caller(MonoBehaviour)");
        }
    }
    /// <summary>
    /// Changes the timerDoneAction
    /// </summary> 
    public void StartTimer(float timeInSeconds, MonoBehaviour caller, System.Action<float> newTimerDoneAction)
    {
        timerDoneAction = newTimerDoneAction;
        StartTimer(timeInSeconds, caller);
    }

    /// <summary>
    /// Sets the time and timer as active. Returns the timeInSeconds
    /// </summary> 
    public float SetTime(float timeInSeconds)
    {
        endTime = Time.time + timeInSeconds;
        active = true;
        return timeInSeconds;
    }

    /// <summary>
    /// Returns time left after adding (-1 if timer isn't active)
    /// </summary>
    public float AddTime(float timeInSeconds)
    {
        if (!active) return -1;
        endTime += timeInSeconds;
        return endTime - Time.time;
    }


    /// <summary>
    /// Returns the time left when paused (-1 if timer isn't  active)
    /// </summary> 
    public float PauseTimer()
    {
        if (active)
        {
            paused = true;
            timePaused = Time.time;
            return endTime - Time.time;
        }

        return -1;
    }
    /// <summary>
    /// Returns the time spent paused (-1 if timer isn't  active)
    /// </summary> 
    public float UnPauseTimer()
    {
        if (active)
        {
            float timeSpentPaused = Time.time - timePaused;

            endTime += timeSpentPaused;
            paused = false;

            return timeSpentPaused;
        }

        return -1;
    }

    /// <summary>
    /// Returns total time spent active
    /// </summary> 
    public float FinishTimer()
    {
        float timeSpentActive = Time.time - startTime;
        timerDoneAction?.Invoke(timeSpentActive);
        KillTimer();
        return timeSpentActive;
    }

    public void KillTimer()
    {
        active = false;
        paused = false;
        startTime = -1;
    }

    private IEnumerator DoTimer()
    {
        while (active)
        {
            if (paused) yield return new WaitUntil(() => !paused);
            if (CheckTimer() == false) FinishTimer();
            yield return null;
        }
    }

    /// <summary>
    /// Checks if timer should still be active
    /// </summary> 
    public bool CheckTimer()
    {
        if (GetTimeLeft() > TIME_DELTA) return true;
        return false;
    }

    /// <summary>
    /// Returns total time spent active when done
    /// </summary> 
    public Timer(MonoBehaviour caller, System.Action<float> timerDoneAction)
    {
        this.caller = caller;
        this.timerDoneAction = timerDoneAction;
    }
    public Timer(MonoBehaviour caller)
    {
        this.caller = caller;
    }
    public Timer() { }
}

