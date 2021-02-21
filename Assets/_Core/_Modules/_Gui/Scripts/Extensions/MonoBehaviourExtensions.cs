using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;

public static class MonoBehaviourExtensions
{
    #region Timer

    public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float delay, Action action)
    {
        return monoBehaviour.StartBehaviourTimer(delay, false, false, action);
    }

    public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float delay, bool repeatable, Action action)
    {
        return monoBehaviour.StartBehaviourTimer(delay, repeatable, false, action);
    }

    public static IEnumerator StartBehaviourTimer(this MonoBehaviour monoBehaviour, float delay, bool repeatable, bool ignoreTimeScale, Action action)
    {
        if (monoBehaviour == null)
        {
            Trace.TraceWarning("monoBehaviour is null (MonoBehaviourExtensions.StartBehaviourTimer)");
            return null;
        }
        IEnumerator timerCoroutine = monoBehaviour.TimerBehaviourCoroutine(delay, repeatable, ignoreTimeScale, action);
        monoBehaviour.StartCoroutine(timerCoroutine);
        return timerCoroutine;
    }

    public static void StopBehaviourTimer(this MonoBehaviour monoBehaviour, ref IEnumerator timerCoroutine)
    {
        if (timerCoroutine != null)
        {
            if (monoBehaviour != null)
                monoBehaviour.StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public static IEnumerator TimerBehaviourCoroutine(this MonoBehaviour monoBehaviour, float delay, bool repeatable, bool ignoreTimeScale, Action action)
    {
        do
        {
            if (ignoreTimeScale)
            {
                float start = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup < start + delay)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            action();
        } while (repeatable);
    }

    #endregion

    public static RectTransform RectTransform(this MonoBehaviour monoBehaviour)
    {
        return monoBehaviour.transform as RectTransform;
    }
}
