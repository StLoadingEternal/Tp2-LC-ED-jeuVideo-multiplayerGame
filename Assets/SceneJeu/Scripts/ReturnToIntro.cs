using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToIntro : MonoBehaviour
{
    [SerializeField] private string introSceneName = "SceneIntroduction";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToIntro();
        }
    }

    public void GoToIntro()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(introSceneName);
    }
}
