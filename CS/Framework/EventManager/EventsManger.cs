using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//public class EventNameBase
//{
//    protected string EventName = "";
//}

public static class EventsManager
{
    static Dictionary<string, UnityEvent> EventsList = new Dictionary<string, UnityEvent>();
    public static void Invoke(string eventName)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName]?.Invoke();
        }
    }

    public static void AddListener(string eventName,UnityAction call)
    {
        if (!EventsList.ContainsKey(eventName))
            EventsList.Add(eventName, new UnityEvent());
        EventsList[eventName].AddListener(call);
    }

    public static void RemoveListener(string eventName, UnityAction call)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName].RemoveListener(call);
        }
    }
}

public static class EventsManager<ArgType>
{
    static Dictionary<string, UnityEvent<ArgType>> EventsList = new Dictionary<string, UnityEvent<ArgType>>();
    public static void Invoke(string eventName, ArgType arg)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName]?.Invoke(arg);
        }
    }
    public static void AddListener(string eventName, UnityAction<ArgType> call)
    {
        if (!EventsList.ContainsKey(eventName))
            EventsList.Add(eventName, new UnityEvent<ArgType>());
        EventsList[eventName].AddListener(call);
    }

    public static void RemoveListener(string eventName, UnityAction<ArgType> call)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName].RemoveListener(call);
        }
    }
}

public static class EventsManager<ArgType0,ArgType1>
{
    static Dictionary<string, UnityEvent<ArgType0, ArgType1>> EventsList = new Dictionary<string, UnityEvent<ArgType0, ArgType1>>();
    public static void Invoke(string eventName, ArgType0 arg0, ArgType1 arg1)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName]?.Invoke(arg0,arg1);
        }
    }
    public static void AddListener(string eventName, UnityAction<ArgType0, ArgType1> call)
    {
        if (!EventsList.ContainsKey(eventName))
            EventsList.Add(eventName, new UnityEvent<ArgType0, ArgType1>());
        EventsList[eventName].AddListener(call);
    }

    public static void RemoveListener(string eventName, UnityAction<ArgType0, ArgType1> call)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName].RemoveListener(call);
        }
    }
}

public static class EventsManager<ArgType0, ArgType1, ArgType2>
{
    static Dictionary<string, UnityEvent<ArgType0, ArgType1, ArgType2>> EventsList = new Dictionary<string, UnityEvent<ArgType0, ArgType1, ArgType2>>();
    public static void Invoke(string eventName, ArgType0 arg0, ArgType1 arg1, ArgType2 arg2)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName]?.Invoke(arg0, arg1,arg2);
        }
    }
    public static void AddListener(string eventName, UnityAction<ArgType0, ArgType1, ArgType2> call)
    {
        if (!EventsList.ContainsKey(eventName))
            EventsList.Add(eventName, new UnityEvent<ArgType0, ArgType1, ArgType2>());
        EventsList[eventName].AddListener(call);
    }

    public static void RemoveListener(string eventName, UnityAction<ArgType0, ArgType1, ArgType2> call)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName].RemoveListener(call);
        }
    }
}

public static class EventsManager<ArgType0, ArgType1, ArgType2, ArgType3>
{
    static Dictionary<string, UnityEvent<ArgType0, ArgType1, ArgType2, ArgType3>> EventsList = new Dictionary<string, UnityEvent<ArgType0, ArgType1, ArgType2, ArgType3>>();
    public static void Invoke(string eventName, ArgType0 arg0, ArgType1 arg1, ArgType2 arg2, ArgType3 arg3)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName]?.Invoke(arg0, arg1, arg2, arg3);
        }
    }
    public static void AddListener(string eventName, UnityAction<ArgType0, ArgType1, ArgType2, ArgType3> call)
    {
        if (!EventsList.ContainsKey(eventName))
            EventsList.Add(eventName, new UnityEvent<ArgType0, ArgType1, ArgType2, ArgType3>());
        EventsList[eventName].AddListener(call);
    }

    public static void RemoveListener(string eventName, UnityAction<ArgType0, ArgType1, ArgType2, ArgType3> call)
    {
        if (EventsList.ContainsKey(eventName))
        {
            EventsList[eventName].RemoveListener(call);
        }
    }
}