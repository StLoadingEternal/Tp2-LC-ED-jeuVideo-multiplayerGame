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

        if (newValue <= 0)
        {
            if (IsOwner)
                ShowDeathScreen();

            if (IsServer)
                StartCoroutine(DestroyCarAfterDelay());
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

    IEnumerator DestroyCarAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<NetworkObject>().Despawn();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Points: " + points.Value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        CarHealth otherCar = collision.gameObject.GetComponent<CarHealth>();
        if (otherCar == null) return;

        // Évite de se détecter soi-même.
        if (otherCar == this) return;

        // Empêche la collision d'être traitée deux fois.
        // Une seule des deux voitures applique les dégâts.
        if (NetworkObjectId > otherCar.NetworkObjectId) return;


        ContactPoint contact = collision.GetContact(0);

        // Convertit le point de contact dans l'espace local de chaque voiture.
        Vector3 localContactThisCar = transform.InverseTransformPoint(contact.point);
        Vector3 localContactOtherCar = otherCar.transform.InverseTransformPoint(contact.point);


        // Si Z est positif, le contact est devant la voiture.
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

            Debug.Log("Collision latérale !");
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