using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Base class containing useful helper methods for all mono behaviours
/// </summary>
public class JMonoBehaviour : MonoBehaviour
{
    public bool log = false;

    /// <summary>
    /// Includes component type in logs to make it easier to spot the log source
    /// </summary>
    /// <param name="message"></param>
    protected void JLog(string message)
    {
#if UNITY_EDITOR
        if (log)
            Debug.Log("<Color=Green>[" + this.GetType().ToString() + ":" + this.name + "]</Color> " + message);
#else
        if (Debug.isDebugBuild)
            Debug.Log("[" + this.GetType().ToString() + ":" + this.name + "] " + message);
#endif
    }

    protected void JLogWarning(string message)
    {
#if UNITY_EDITOR
        if (log)
            Debug.LogWarning("<Color=Green>[" + this.GetType().ToString() + ":" + this.name + "]</Color> " + message);
#else
        if (Debug.isDebugBuild)
            Debug.LogWarning("[" + this.GetType().ToString() + ":" + this.name + "] " + message);
#endif
    }

    protected void JLogError(string message)
    {
        Debug.LogError("[" + this.GetType().ToString() + ":" + this.name + "] " + message);
    }
}

public abstract class JMonoSingleton<T> : JMonoBehaviour where T : JMonoBehaviour
{
    protected static T _onlyInstance = null;
    protected static bool _instanceChecked = false;
    protected static bool _keepInstanceAliveWhenDeactivated = false;

    public static bool Exists
    {
        get
        {
            if (_instanceChecked)
                return _onlyInstance != null && _onlyInstance.enabled && _onlyInstance.gameObject.activeInHierarchy;
            if (_onlyInstance == null)
                _onlyInstance = FindObjectOfType<T>();
            if (_onlyInstance != null)
                _instanceChecked = true;
            return _onlyInstance != null;
        }
    }

    public static T Instance
    {
        get
        {
            //you sure we need this ?
            if (_onlyInstance != null && (!_onlyInstance.enabled || !_onlyInstance.gameObject.activeInHierarchy) && !_keepInstanceAliveWhenDeactivated)
                _onlyInstance = null;
            if (_onlyInstance == null)
            {
                Profiler.BeginSample("InitSingleton<"+typeof(T)+">");
                _instanceChecked = true;
                _onlyInstance = FindObjectOfType<T>();
                if (_onlyInstance==null && !_logged)
                {
                    _logged = true;
                    Debug.LogError("Could not locate a " + typeof(T).ToString() + " object. You have to have exactly one in the scene.");
                }
                Profiler.EndSample();
            }
            return _onlyInstance;
        }
    }

    private static bool _logged = false;

    public bool keepAliveWhenDeactivated = false;

    protected virtual void Awake()
    {
        _keepInstanceAliveWhenDeactivated = keepAliveWhenDeactivated;
        //init service
        if (_onlyInstance == null)
        {
            //Debug.Log("Init singleton " + typeof(T));
            _onlyInstance = gameObject.GetComponent<T>();
        }
        else if (_onlyInstance!=this)
        {
            //Debug.LogError("Duplicate singleton: " + gameObject.name + " type=" + GetType());
            DestroyImmediate(gameObject);
        }
            
    }
    protected virtual void OnDisable()
    {
        _onlyInstance = null;
        _instanceChecked = false;
    }
}
