using Networking;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    public bool AutoUnparentOnAwake = true;

    protected static T instance;

    public static bool HasInstance => instance != null;
    public static T TryGetInstance() => HasInstance ? instance : null;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                (FindAnyObjectByType<T>() as PersistentSingleton<T>)?.InitializeSingleton();
                if (instance == null)
                {
                    var go = new GameObject(typeof(T).Name + " Auto-Generated");
                    var component = go.AddComponent<T>();
                    (component as PersistentSingleton<T>)?.InitializeSingleton();
                }
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
