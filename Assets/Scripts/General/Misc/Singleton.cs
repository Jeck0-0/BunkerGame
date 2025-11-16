using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component 
{
    private static T instance;

    public static bool HasInstance => instance != null;
    public static T TryGetInstance() => HasInstance ? instance : null;

    public static T Instance 
    {
        get 
        {
            if (instance == null) 
                (FindAnyObjectByType<T>() as Singleton<T>)?.InitializeSingleton();

            return instance;
        }
    }

    protected virtual void Awake() 
    {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton() 
    {
        if (!Application.isPlaying) return;

        instance = this as T;
    }
}