using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public delegate void BindingListener(string stringOutput);
    Dictionary<string, string> _TextBindings = new Dictionary<string, string>();
    Dictionary<string, List<BindingListener>> bindingListeners = new Dictionary<string, List<BindingListener>>();
    static UIManager singleton;
    public static UIManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindObjectOfType<UIManager>();
            }
            return singleton;
        }
    }
    public  void AddBindable(string hash, string defaultValue)
    {
        _TextBindings.Add(hash, defaultValue);
        BindingChangedNotify(hash, defaultValue);
    }

    private  void BindingChangedNotify(string hash, string defaultValue)
    {
        if (bindingListeners == null) return;

        try
        {
            foreach (BindingListener listener in bindingListeners[hash])
            {
                listener(defaultValue);
            }
        }
        catch (KeyNotFoundException error)
        {
            //Ei löytynyt listasta, jatketaan
        }
    }

    public void SetBindable(string hash, string newValue)
    {
        _TextBindings[hash] = newValue;
        BindingChangedNotify(hash, newValue);
    }
    public string GetBindable(string hash)
    {
        return _TextBindings[hash];
    }
    public void RegisterBinding(string hash, BindingListener bindingListener)
    {
        if (bindingListeners == null)
        {
            bindingListeners = new Dictionary<string, List<BindingListener>>();
        }

        if (!bindingListeners.ContainsKey(hash))
        {
            bindingListeners[hash] = new List<BindingListener>();
        }
        bindingListeners[hash].Add(bindingListener);
    }
    public string FormatTimer(float time)
    {
        float remainingSeconds = (int)(time % 60f);
        float remainingMinutes = Mathf.Floor((time / 60f) % 60f);
        return string.Format("{0:00} {1:00}",remainingMinutes, remainingSeconds);
    }
    private void OnEnable()
    {
        _TextBindings.Clear();
    }
}
