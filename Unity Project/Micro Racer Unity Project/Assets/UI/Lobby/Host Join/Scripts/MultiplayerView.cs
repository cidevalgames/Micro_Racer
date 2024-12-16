using FishNet;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerView : View
{
    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    public override void Initialize()
    {
        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();

            ViewManager.Instance.Show<RoleSelectionView>();
        });

        joinButton.onClick.AddListener(() =>
        {
            InstanceFinder.ClientManager.StartConnection();

            ViewManager.Instance.Show<RoleSelectionView>();
        });
        
        base.Initialize();
    }

    public override void Show(object args = null)
    {
        if (args is string message)
        {
            Debug.Log(message);
        }

        base.Show(args);
    }
} 
