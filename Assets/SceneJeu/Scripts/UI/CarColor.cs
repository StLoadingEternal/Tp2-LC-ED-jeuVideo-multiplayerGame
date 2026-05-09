using Unity.Netcode;
using UnityEngine;

public class CarColor : NetworkBehaviour
{
    private NetworkVariable<Color> carColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Couleurs disponibles pour les 4 joueurs
    private Color[] availableColors = new Color[]
    {
        Color.yellow,
        Color.red,
        Color.blue,
        Color.green
    };

    // Le renderer du body de la voiture
    public Renderer bodyRenderer;

    public override void OnNetworkSpawn()
    {
        // Le serveur assigne la couleur selon le clientId
        if (IsServer)
        {
            int colorIndex = (int)(OwnerClientId % (ulong)availableColors.Length);
            carColor.Value = availableColors[colorIndex];
        }

        // Tous les clients appliquent la couleur
        carColor.OnValueChanged += OnColorChanged;
        ApplyColor(carColor.Value);
    }

    void OnColorChanged(Color oldColor, Color newColor)
    {
        ApplyColor(newColor);
    }

    void ApplyColor(Color color)
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = color;
        }
    }

    public override void OnDestroy()
    {
        carColor.OnValueChanged -= OnColorChanged;
    }

    public Color GetColor()
{
    return carColor.Value;
}
}