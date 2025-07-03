using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Linq;

public class ClientNetworkManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public Button attemptCallButton;
    public TextMeshProUGUI statusText;
    public Button letsGoButton;
    
    [Header("Network Settings")]
    private string targetIP;
    private int targetPort;
    private NetworkManager networkManager;
    
    void Start()
    {
        // Set default port
        portInputField.text = "7777";
        statusText.text = "waiting";
        
        // Set up button listeners
        attemptCallButton.onClick.AddListener(AttemptConnection);
        letsGoButton.onClick.AddListener(JoinGame);
        
        // Initially hide Let's Go until connected
        letsGoButton.gameObject.SetActive(false);
    }
    
    void AttemptConnection()
    {
        targetIP = ipInputField.text.Trim();
        string portText = portInputField.text.Trim();
        
        // Basic validation
        if (string.IsNullOrEmpty(targetIP))
        {
            statusText.text = "Please enter host IP";
            return;
        }
        
        if (!int.TryParse(portText, out targetPort))
        {
            statusText.text = "Invalid port number";
            return;
        }
        
        statusText.text = "Connecting...";
        
        // Start connection process with proper cleanup
        StartCoroutine(ConnectWithCleanup());
    }
    
    System.Collections.IEnumerator ConnectWithCleanup()
    {
        // Check if host is already running in this instance
        if (NetworkServer.active)
        {
            Debug.Log("CLIENT: Host already running in this instance - cannot create separate client");
            statusText.text = "Host already running - build separate client";
            yield break;
        }
        
        // Clean up any existing NetworkManagers
        NetworkManager[] existingManagers = FindObjectsByType<NetworkManager>(FindObjectsSortMode.None);
        for (int i = 0; i < existingManagers.Length; i++)
        {
            if (existingManagers[i] != null)
            {
                Debug.Log($"CLIENT: Destroying existing NetworkManager: {existingManagers[i].name}");
                DestroyImmediate(existingManagers[i].gameObject);
            }
        }
        
        // Wait a frame to ensure cleanup
        yield return null;
        
        // Create fresh NetworkManager for client
        GameObject nmGO = new GameObject("ClientNetworkManager");
        networkManager = nmGO.AddComponent<NetworkManager>();
        
        // Find KCP transport (same as host uses)
        System.Type transportType = FindKcpTransport();
        
        if (transportType != null)
        {
            Debug.Log($"CLIENT: Found transport: {transportType.FullName}");
            
            var transport = nmGO.AddComponent(transportType) as Transport;
            
            // Set port using reflection
            var portField = transportType.GetField("port");
            if (portField != null)
            {
                portField.SetValue(transport, (ushort)targetPort);
                Debug.Log($"CLIENT: Set port to {targetPort}");
            }
            
            // IMPORTANT: Set transport as active
            Transport.active = transport;
            
            // Assign transport and set connection details
            networkManager.transport = transport;
            networkManager.networkAddress = targetIP;
            
            Debug.Log($"CLIENT: Transport activated: {transport.GetType().Name}");
            
            // Start client connection
            networkManager.StartClient();
            
            Debug.Log($"CLIENT: Attempting to connect to {targetIP}:{targetPort}");
            
            // Check connection status
            StartCoroutine(CheckConnectionStatus());
        }
        else
        {
            statusText.text = "Transport not found";
            Debug.LogError("CLIENT: No suitable transport found!");
        }
    }
    
    System.Type FindKcpTransport()
    {
        try
        {
            // Search for KCP transport (same as host)
            var allTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => {
                    try 
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return new System.Type[0];
                    }
                });
            
            // Look for KcpTransport first (Mirror's standard)
            foreach (var type in allTypes)
            {
                if (type.Name == "KcpTransport")
                {
                    Debug.Log($"CLIENT: Found KcpTransport: {type.FullName}");
                    return type;
                }
            }
            
            // If KCP not found, try TelepathyTransport as fallback
            foreach (var type in allTypes)
            {
                if (type.Name == "TelepathyTransport")
                {
                    Debug.Log($"CLIENT: Using TelepathyTransport fallback: {type.FullName}");
                    return type;
                }
            }
            
            Debug.LogError("CLIENT: No suitable transport found (tried KCP and Telepathy)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CLIENT: Transport search failed: {e.Message}");
        }
        
        return null;
    }
    
    System.Collections.IEnumerator CheckConnectionStatus()
    {
        float timeout = 10f;
        float elapsed = 0f;
        
        // Store the NetworkManager we just created for this connection
        var ourNetworkManager = networkManager;
        
        while (elapsed < timeout)
        {
            // Check if OUR specific NetworkManager has an active client connection
            // AND that we're not confusing it with the host's local client
            if (ourNetworkManager != null && 
                ourNetworkManager.mode == NetworkManagerMode.ClientOnly &&  // Ensure we're in client-only mode
                NetworkClient.active &&  // Client is active
                NetworkClient.ready)     // Client is ready (better than isConnected)
            {
                statusText.text = "Connected!";
                letsGoButton.gameObject.SetActive(true);
                Debug.Log($"CLIENT: Successfully connected to {targetIP}:{targetPort}");
                yield break;
            }
            
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        
        statusText.text = "Connection failed";
        Debug.Log("CLIENT: Connection attempt timed out or failed");
    }
    
    void JoinGame()
    {
        statusText.text = "Joining game...";
        Debug.Log("CLIENT: Player ready to join game");
    }
}