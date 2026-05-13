using Unity.Netcode;
using UnityEngine;

public class WheelControl : NetworkBehaviour
{
    public Transform wheelModel;
    [HideInInspector] public WheelCollider WheelCollider;
    public bool steerable;
    public bool motorized;


    Vector3 position;
    Quaternion rotation;


    // Position de la roue synchronisée sur le réseau.
    // Seul le serveur a le droit d'écrire cette valeur, car le mouvement du véhicule
    // est géré en autorité serveur.
    private NetworkVariable<Vector3> networkWheelPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);

    // Rotation de la roue synchronisée sur le réseau.
    // Elle permet aux clients de voir les roues tourner et pivoter correctement.
    private NetworkVariable<Quaternion> networkWheelRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Server);

    private void Start()
    {
        WheelCollider = GetComponent<WheelCollider>();
    }

    void FixedUpdate()
    {

        if (wheelModel == null || WheelCollider == null) return;


        // Le serveur est responsable de la simulation physique.
        // Il lit la vraie position et rotation du WheelCollider,
        // puis il les envoie aux clients avec les NetworkVariables.
        if (IsServer)
        {
            WheelCollider.GetWorldPose(out position, out rotation);

            networkWheelPosition.Value = position;
            networkWheelRotation.Value = rotation;

            ApplyWheelPose(position, rotation);
        }
        else
        {
            // Les clients ne calculent pas la physique de la roue.
            // Ils appliquent simplement les valeurs reçues du serveur.
            ApplyWheelPose(networkWheelPosition.Value, networkWheelRotation.Value);
        }
    }


    // On applique la position et la rotation au mesh visuel de la roue.
    // Cela permet de voir la roue tourner et pivoter dans toutes les fenêtres.
    private void ApplyWheelPose(Vector3 pos, Quaternion rot)
    {
        wheelModel.position = pos;
        wheelModel.rotation = rot;
    }
}