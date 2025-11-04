using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class ScriptableVariable<T> : ScriptableObject
{
    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if(value.Equals(_value))
                return;
            
            _value = value;
            OnChange?.Invoke(this);
        }
    }

    public UnityEvent<ScriptableVariable<T>> OnChange;
}
