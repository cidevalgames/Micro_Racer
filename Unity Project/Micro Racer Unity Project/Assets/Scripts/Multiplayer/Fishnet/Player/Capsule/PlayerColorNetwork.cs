using UnityEngine;
using FishNet.Object;

public class PlayerColorNetwork : NetworkBehaviour
{
    public GameObject body;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {

        }
        else
        {
            GetComponent<PlayerColorNetwork>().enabled = false;
        }
    }

    [ServerRpc]
    public void ChangeColorServer(GameObject player)
    {
        Color color = Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);

        ChangeColor(player, color);
    }

    [ObserversRpc]
    public void ChangeColor(GameObject player, Color color)
    {
        player.GetComponent<PlayerColorNetwork>().body.GetComponent<Renderer>().material.color = color;
    }

    private void Update()
    {
        if (base.IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeColorServer(gameObject);
            }
        }
    }
}
