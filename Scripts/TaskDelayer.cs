using System;
using System.Collections;
using UnityEngine;


class TaskDelayer : MonoBehaviour
{

    public static IEnumerator createDelayedTask(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}

