using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] Behaviour[] componentsToDisable;

    private Camera sceneCamera;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
        else
        {
            sceneCamera = Camera.main;

            if (sceneCamera)
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Respawn(gameObject));
            }
        }
    }

    IEnumerator Respawn(GameObject go)
    {
        NetworkServer.UnSpawn(go);
        Transform newPos = NetworkManager.singleton.GetStartPosition();
        go.transform.position = newPos.position;
        go.transform.rotation = newPos.rotation;
        NetworkServer.Spawn(go, go);
        yield return new WaitForSeconds(1);
    }

    //private void OnDisable()
    //{
    //    if (sceneCamera)
    //    {
    //        sceneCamera.gameObject.SetActive(true);
    //    }
    //}
}
