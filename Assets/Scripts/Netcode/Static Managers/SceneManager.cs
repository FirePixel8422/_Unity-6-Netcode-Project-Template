using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class NetworkSceneManager
{
    /// <summary>
    /// MUST be called on server. Load a scene on the network.
    /// </summary>
    public static SceneEventProgressStatus LoadSceneOnNetwork_OnServer(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        return NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
    }
    /// <summary>
    /// MUST be called on server. Unload a scene on the network.
    /// </summary>
    public static SceneEventProgressStatus UnLoadSceneOnNetwork_OnServer(Scene scene)
    {
        return NetworkManager.Singleton.SceneManager.UnloadScene(scene);
    }
}
