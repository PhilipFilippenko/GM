using UnityEngine;
using Unity.Netcode;

public class NetworkButtons : MonoBehaviour
{
    NetworkManager manager;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        if (manager == null) return;
        if (manager.IsClient || manager.IsServer) return;

        if (GUI.Button(new Rect(10, 10, 120, 40), "Host"))
            manager.StartHost();
        if (GUI.Button(new Rect(10, 60, 120, 40), "Client"))
            manager.StartClient();
    }
}