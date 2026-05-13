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

    private void Start()
    {
        WheelCollider = GetComponent<WheelCollider>();
    }

    void FixedUpdate()
    {
        if (wheelModel != null)
        {
            WheelCollider.GetWorldPose(out position, out rotation);
            wheelModel.transform.position = position;
            wheelModel.transform.rotation = rotation;
        }
    }
}