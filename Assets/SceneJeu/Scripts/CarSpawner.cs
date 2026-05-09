using Unity.Netcode;
using UnityEngine;

public class CarSpawner : NetworkBehaviour
{
    public GameObject carPrefab;
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
    {
        // Seulement écouter les nouveaux clients
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        // Spawner la voiture du host
        SpawnCarForClient(NetworkManager.Singleton.LocalClientId);
    }
    }

    void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
    
        if (IsServer)
            SpawnCarForClient(clientId);
    }

    void SpawnCarForClient(ulong clientId)
    {
        int index = (int)(clientId % (ulong)spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        GameObject car = Instantiate(carPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        NetworkObject netObj = car.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}