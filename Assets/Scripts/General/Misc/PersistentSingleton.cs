using Networking;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    public bool AutoUnparentOnAwake = true;

    protected static T instance;

    public static bool HasInstance => instance != null;
    public static T TryGetInstance() => HasInstance ? instance : null;
    private bool initialized = false;
    
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                var a = FindAnyObjectByType<T>();
                Debug.Log("a " + (a == null));
                (a as PersistentSingleton<T>)?.InitializeSingleton();
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton()
    {
        if (initialized) return;
        initialized = true;
        
        if (!Application.isPlaying) return;

        if (AutoUnparentOnAwake)
            transform.SetParent(null);

        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
