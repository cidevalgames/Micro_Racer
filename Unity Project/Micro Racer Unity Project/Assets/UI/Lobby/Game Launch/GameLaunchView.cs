using FishNet;
using UnityEngine;
using UnityEngine.UI;

public class GameLaunchView : View
{
    [SerializeField] private Button launchGameButton;

    public override void Initialize()
    {
        launchGameButton.onClick.AddListener(() =>
        {
            if (InstanceFinder.ClientManager.Clients.Count < 2)
                return;

            BootstrapSceneManager.Instance.LoadScene("MainScene");
            BootstrapSceneManager.Instance.UnloadScene("Lobby");
        });

        base.Initialize();
    }
}
