using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RearWallSpawner : NetworkBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject rearWallPrefab;

    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 60f;

    [Header("Wall")]
    [SerializeField] private float wallLifeTime = 10f;
    [SerializeField] private float spawnDistanceBehind = 5f;
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("UI")]
    [SerializeField] private Image cooldownIcon;

    private float lastSpawnTime = -999f;

    private void Update()
    {
        if (!IsOwner) return;

        UpdateCooldownIcon();

        if (Input.GetKeyDown(KeyCode.F))
        {
            TrySpawnRearWall();
        }
    }

    private void TrySpawnRearWall()
    {
        if (Time.time - lastSpawnTime < cooldownDuration)
        {
            return;
        }

        lastSpawnTime = Time.time;
        SpawnRearWallServerRpc();
    }

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

    [ServerRpc]
    private void SpawnRearWallServerRpc()
    {
        if (rearWallPrefab == null)
        {
            Debug.LogWarning("Prefab du mur manquant.");
            return;
        }

        Vector3 spawnPosition = transform.position - transform.forward * spawnDistanceBehind;
        spawnPosition.y += spawnHeight;

        Quaternion spawnRotation = transform.rotation;

        GameObject wall = Instantiate(
            rearWallPrefab,
            spawnPosition,
            spawnRotation
        );

        NetworkTimedDestroy timedDestroy = wall.GetComponent<NetworkTimedDestroy>();
        if (timedDestroy != null)
        {
            timedDestroy.SetLifeTime(wallLifeTime);
        }

        NetworkObject networkObject = wall.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Le prefab du mur doit avoir un NetworkObject.");
        }
    }
}
