using Car.Multiplayer.Common;
using Car.Multiplayer.Common.Spawn;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Network;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectionView : View
{
    [Header("Buttons")]
    [SerializeField] private Button hunterButton;
    [SerializeField] private Button bunnyButton;

    public override void Initialize()
    {
        hunterButton.onClick.AddListener(() =>
        {
            ViewManager.Instance.Show<GameLaunchView>();

            FindAnyObjectByType<RoleSelection>(FindObjectsInactive.Exclude).m_playerRole = PlayerRole.Hunter;
        });

        bunnyButton.onClick.AddListener(() =>
        {
            ViewManager.Instance.Show<GameLaunchView>();

            FindAnyObjectByType<RoleSelection>(FindObjectsInactive.Exclude).m_playerRole = PlayerRole.Bunny;
        });

        base.Initialize();
    }
}
