using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class CarHealth : NetworkBehaviour
{

    [Header("Points")]
    public int startingPoints = 100;
    public int collisionDamage = 10;

    private NetworkVariable<int> points = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private TextMeshProUGUI scoreText;
    private bool isEliminated = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null)
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        }

        if (IsServer)
            points.Value = startingPoints;

        points.OnValueChanged += OnPointsChanged;
        UpdateScoreUI();
    }

    void OnPointsChanged(int oldValue, int newValue)
    {
        UpdateScoreUI();

        // La mort est gérée seulement par le serveur.
        // Le bool isEliminated évite de lancer plusieurs fois la coroutine
        // si les points restent à 0 ou si plusieurs collisions arrivent en même temps.
        if (newValue <= 0 && IsServer && !isEliminated)
        {
            isEliminated = true;
            StartCoroutine(HandlePlayerDeathAfterDelay());
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Points: " + points.Value;
    }


    IEnumerator HandlePlayerDeathAfterDelay()
    {
        // Client propriétaire de cette voiture.
        ulong deadClientId = OwnerClientId;

        // Affiche le DeathText seulement au joueur mort.
        ShowDeathScreenClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { deadClientId }
            }
        });

        // Laisse le joueur voir le message de mort.
        yield return new WaitForSeconds(2f);

        // Retire la voiture du réseau pour tous les joueurs.
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }

        // Si le joueur mort est un client normal, on le déconnecte.
        // On ne déconnecte pas le host, sinon la partie risque de fermer pour tout le monde.
        if (deadClientId != NetworkManager.ServerClientId)
        {
            NetworkManager.Singleton.DisconnectClient(deadClientId);
        }
        else
        {
            Debug.Log("Le host est mort : son bolide est retiré, mais la partie reste active.");
        }
    }

    void ShowDeathScreen()
    {
        // Cherche dans tous les objets incluant les inactifs
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var text in allTexts)
        {
            if (text.gameObject.name == "DeathText")
            {
                text.gameObject.SetActive(true);
                break;
            }
        }
    }

    [ClientRpc]
    void ShowDeathScreenClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Ce RPC est envoyé seulement au joueur mort.
        ShowDeathScreen();
    }

   
    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        CarHealth otherCar = collision.gameObject.GetComponentInParent<CarHealth>();
        if (otherCar == null) return;

        // Évite de se détecter soi-même.
        if (otherCar == this) return;

        // Empêche la collision d'être traitée deux fois.
        if (NetworkObjectId > otherCar.NetworkObjectId) return;

        ContactPoint contact = collision.GetContact(0);

        Vector3 localContactThisCar = transform.InverseTransformPoint(contact.point);
        Vector3 localContactOtherCar = otherCar.transform.InverseTransformPoint(contact.point);

        bool thisCarFrontHit = localContactThisCar.z > 0f;
        bool otherCarFrontHit = localContactOtherCar.z > 0f;

        bool validDamageCollision = false;

        if (thisCarFrontHit && otherCarFrontHit)
        {
            TakeDamage(collisionDamage);
            otherCar.TakeDamage(collisionDamage);

            validDamageCollision = true;

            Debug.Log("Collision frontale !");
        }
        else if (thisCarFrontHit)
        {
            otherCar.TakeDamage(collisionDamage);

            validDamageCollision = true;

            Debug.Log("Collision latérale : cette voiture attaque !");
        }
        else if (otherCarFrontHit)
        {
            TakeDamage(collisionDamage);

            validDamageCollision = true;

            Debug.Log("Collision latérale : autre voiture attaque !");
        }

        //Reaction des spectateurs seulement si la collision est avec une autre voiture
        if (validDamageCollision)
        {
            SpectatorManager manager = FindFirstObjectByType<SpectatorManager>();

            if (manager != null)
            {
                manager.TriggerCollisionReaction();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        points.Value = Mathf.Max(0, points.Value - damage);
    }

    public override void OnDestroy()
    {
        points.OnValueChanged -= OnPointsChanged;
    }

    public int GetPoints()
    {
        return points.Value;
    }
}