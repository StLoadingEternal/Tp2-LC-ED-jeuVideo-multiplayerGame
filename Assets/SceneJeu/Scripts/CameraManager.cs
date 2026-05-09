using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    public Camera overviewCam;

    private Camera intCam;
    private Camera extCam;
    private Camera[] cameras;
    private int idx = 0;

    void Update()
    {
        // Cherche les caméras si pas encore trouvées
        if (intCam == null || extCam == null)
        {
            FindCarCameras();
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            idx = (idx + 1) % cameras.Length;
            Activate(idx);
        }
    }

    void FindCarCameras()
    {
        // Cherche la voiture du joueur local
        var cars = FindObjectsByType<RacingCarControl>(FindObjectsSortMode.None);
        foreach (var car in cars)
        {
            NetworkObject netObj = car.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                intCam = car.GetComponentsInChildren<Camera>(true)[0];
                extCam = car.GetComponentsInChildren<Camera>(true)[1];

                cameras = new Camera[] { overviewCam, intCam, extCam };
                Activate(0);
                Debug.Log("Caméras trouvées !");
                break;
            }
        }
    }

    void Activate(int i)
    {
        for (int j = 0; j < cameras.Length; j++)
            cameras[j].gameObject.SetActive(j == i);
    }
}