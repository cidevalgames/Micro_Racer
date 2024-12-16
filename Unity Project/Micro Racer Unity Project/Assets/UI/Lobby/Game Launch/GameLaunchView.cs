using System.Collections;
using FishNet;
using Network;
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

            StartCoroutine(OnLaunchGame());            
        });

        base.Initialize();
    }

    private IEnumerator OnLaunchGame()
    {
        BootstrapSceneManager.Instance.LoadScene("MainScene");
        BootstrapSceneManager.Instance.UnloadScene("Lobby");

        yield return new WaitForSeconds(1f);

        FindAnyObjectByType<RoleSelection>(FindObjectsInactive.Exclude).SpawnPlayer();
    }
}
