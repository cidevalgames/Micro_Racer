using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;

public class BootstrapSceneManager : MonoBehaviour
{
    public static BootstrapSceneManager Instance;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
            Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        SceneLoadData sceneLoadData = new SceneLoadData(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
    }
    
    public void UnloadScene(string sceneName)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        SceneUnloadData sceneUnloadData = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sceneUnloadData);
    }
}
