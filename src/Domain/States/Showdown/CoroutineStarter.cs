using System.Collections;
using UnityEngine;

namespace Showdown4.Domain.States.Showdown;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter _instance;
    private Coroutine startedCoroutine;

    public static CoroutineStarter Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject if no instance exists
                GameObject obj = new GameObject("CoroutineStarter");
                _instance = obj.AddComponent<CoroutineStarter>();
                DontDestroyOnLoad(obj); // Optional: Keeps this object across different scenes
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Ensure there is only one instance
        }
    }

    public void StartExternalCoroutine(IEnumerator coroutine)
    {
        if (_instance == null)
        {
            ModLogger.LogError("CoroutineStarter is not initialized!");
            return;
        }

        startedCoroutine = _instance.StartCoroutine(coroutine);
    }

    public void StopExternalCoroutine()
    {
        if (_instance == null)
        {
            ModLogger.LogError("CoroutineStarter is not initialized!");
            return;
        }

        if (startedCoroutine != null)
        {
            _instance.StopCoroutine(startedCoroutine);
        }
    }
}