using FishNet;
using FishNet.Transporting.Tugboat;
using System;
using System.RandomExtension;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoadAttribute]
public static class PlayModeStateChangedExample
{
    static PlayModeStateChangedExample()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Random rnd = new Random();
            ushort u = rnd.NextUShort();

            InstanceFinder.NetworkManager.GetComponent<Tugboat>().SetPort(u);
        }
    }
}
#endif