#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class InputSystemEventSystemCompatibility
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StandaloneInputModule[] legacyModules = Object.FindObjectsByType<StandaloneInputModule>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (StandaloneInputModule legacyModule in legacyModules)
        {
            GameObject eventSystemObject = legacyModule.gameObject;
            legacyModule.enabled = false;

            if (eventSystemObject.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            Object.Destroy(legacyModule);
        }
    }
}
#endif
