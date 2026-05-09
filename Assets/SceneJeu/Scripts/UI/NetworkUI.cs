using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI joinCodeDisplay;
    public Button hostButton;
    public Button clientButton;

    async void Start()
    {
        // Initialiser Unity Services
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    async void StartHost()
{
    try
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        joinCodeDisplay.text = "Join Code: " + joinCode;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        NetworkManager.Singleton.StartHost();

        // Cacher tout sauf le join code
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
        // joinCodeDisplay reste visible !
    }
    catch (System.Exception e)
    {
        Debug.LogError("Erreur host : " + e.Message);
    }
}

async void StartClient()
{
    try
    {
        string joinCode = joinCodeInput.text.Trim();

        JoinAllocation joinAllocation = await RelayService.Instance
            .JoinAllocationAsync(joinCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        NetworkManager.Singleton.StartClient();

        // Cacher tout
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
        joinCodeDisplay.gameObject.SetActive(false);
    }
    catch (System.Exception e)
    {
        Debug.LogError("Erreur client : " + e.Message);
    }
}
}