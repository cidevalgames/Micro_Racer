using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using Network;
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

        RoleSelection[] rs = FindObjectsByType<RoleSelection>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        NetworkObject[] no = new NetworkObject[rs.Length];

        for (int i = 0; i < rs.Length; i++)
        {
            no[i] = rs[i].GetComponent<NetworkObject>();
        }

        sceneLoadData.MovedNetworkObjects = no;
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
