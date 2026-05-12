using Unity.Netcode;
using UnityEngine;

public class NetworkTimedDestroy : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 10f;

    public void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
    }

    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroyObject), lifeTime);
        }
    }

    private void DestroyObject()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}
