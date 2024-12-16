using UnityEngine;
using FishNet;
using FishNet.Object;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private string sceneToLoadName;

    public override void OnStartClient()
    {
        base.OnStartClient();

        BootstrapSceneManager.Instance.LoadScene(sceneToLoadName);
        BootstrapSceneManager.Instance.UnloadScene("Lobby");
    }
}
