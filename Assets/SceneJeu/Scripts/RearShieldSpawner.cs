using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RearShieldSpawner : NetworkBehaviour
{
    [Header("Shield Prefab")]
    [SerializeField] private GameObject rearShieldPrefab;

    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 60f;

    [Header("Shield")]
    [SerializeField] private float shieldLifeTime = 10f;
    [SerializeField] private float spawnDistanceBehind = 5f;
    [SerializeField] private float spawnHeight = 0.5f;

    //UI
    private Image cooldownIcon;

    private float lastSpawnTime = -999f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        //Récuperer l'icon UI
        GameObject iconObject = GameObject.Find("RearShieldIcon");

        if (iconObject != null)
        {
            cooldownIcon = iconObject.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("RearWallCooldownIcon introuvable dans la scène.");
        }
    }

    private void Update()
    {
        //Action par le propriétaire
        if (!IsOwner) return;

        //Rechargement du shield
        UpdateCooldownIcon();

        //F -> Shield
        if (Input.GetKeyDown(KeyCode.F))
        {
            TrySpawnRearShield();
        }
    }

    private void TrySpawnRearShield()
    {
        //Cooldown terminé ?
        if (Time.time - lastSpawnTime < cooldownDuration)
        {
            return;
        }

        lastSpawnTime = Time.time;
        SpawnRearWallServerRpc();
    }

    //Mis-à-jour UI shield
    private void UpdateCooldownIcon()
    {
        if (cooldownIcon == null) return;

        float elapsedTime = Time.time - lastSpawnTime;
        float cooldownProgress = Mathf.Clamp01(elapsedTime / cooldownDuration);

        cooldownIcon.fillAmount = cooldownProgress;

        if (cooldownProgress >= 1f)
        {
            cooldownIcon.color = Color.white;
        }
        else
        {
            cooldownIcon.color = Color.gray;
        }
    }

    //Spawn par le server à l'arrière du véhicule
    [ServerRpc]
    private void SpawnRearWallServerRpc()
    {
        if (rearShieldPrefab == null)
        {
            Debug.LogWarning("Prefab du bouclier manquant.");
            return;
        }

        //Position et instantiation
        Vector3 spawnPosition = transform.position - transform.forward * spawnDistanceBehind;
        spawnPosition.y += spawnHeight;

        Quaternion spawnRotation = transform.rotation;

        GameObject shield = Instantiate(
            rearShieldPrefab,
            spawnPosition,
            spawnRotation
        );

        //Temps de vie du bouclier
        NetworkTimedDestroy timedDestroy = shield.GetComponent<NetworkTimedDestroy>();
        if (timedDestroy != null)
        {
            timedDestroy.SetLifeTime(shieldLifeTime);
        }

        //Spawn
        NetworkObject networkObject = shield.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Le prefab du shield doit avoir un NetworkObject.");
        }
    }
}
