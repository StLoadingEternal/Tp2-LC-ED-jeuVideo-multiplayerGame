using Unity.Netcode;
using UnityEngine;

public class NetworkTimedDestroy : NetworkBehaviour
{

    //Temps de vie du shield
    [SerializeField] private float lifeTime = 20f;

    public void SetLifeTime(float newLifeTime)
    {
        lifeTime = newLifeTime;
    }

    private void Start()
    {
        if (IsServer)
        {
            //Destrucction de l'objet après un certain temps
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
