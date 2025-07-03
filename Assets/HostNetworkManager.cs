using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

public class HostNetworkManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI ipDisplayText;
    public Button copyIpButton;
    public Button letsGoButton;
    public TextMeshProUGUI connectedPlayersText;
    
    [Header("Network Settings")]
    public int port = 7777;
    
    private string hostIP;
    private NetworkManager networkManager;
    private List<string> connectedPlayerNames = new List<string>();
    private bool gameStarted = false;
    
    void Start()
    {
        // Start hosting immediately when panel opens
        StartHostingImmediately();
        
        copyIpButton.onClick.AddListener(CopyIPToClipboard);
        letsGoButton.onClick.AddListener(StartGame);
    }
    
    void StartHostingImmediately()
    {
        hostIP = GetLocalIPAddress();
        ipDisplayText.text = $"Share with friends:\n{hostIP}:{port}";
        
        // Clean up any existing NetworkManagers
        NetworkManager[] existingManagers = FindObjectsByType<NetworkManager>(FindObjectsSortMode.None);
        for (int i = 0; i < existingManagers.Length; i++)
        {
            DestroyImmediate(existingManagers[i].gameObject);
        }
        
        // Create fresh NetworkManager
        GameObject nmGO = new GameObject("HostNetworkManager");
        networkManager = nmGO.AddComponent<NetworkManager>();
        
        // Find KCP transport (what Mirror Basic example uses)
        System.Type transportType = FindKcpTransport();
        
        if (transportType != null)
        {
            Debug.Log($"HOST: Found transport: {transportType.FullName}");
            
            var transport = nmGO.AddComponent(transportType) as Transport;
            
            // Set port using reflection
            var portField = transportType.GetField("port");
            if (portField != null)
            {
                portField.SetValue(transport, (ushort)port);
                Debug.Log($"HOST: Set port to {port}");
            }
            
            // IMPORTANT: Set transport as active
            Transport.active = transport;
            
            // Assign transport to NetworkManager
            networkManager.transport = transport;
            
            // Handle Player Prefab requirement (for now, continue without)
            if (networkManager.playerPrefab == null)
            {
                Debug.LogWarning("HOST: No Player Prefab assigned - will create empty player objects");
            }
            
            Debug.Log($"HOST: Transport activated: {transport.GetType().Name}");
            
            // Start hosting
            networkManager.StartHost();
            
            Debug.Log($"HOST: Started hosting on {hostIP}:{port}");
            UpdateConnectedPlayersDisplay();
            
            // Check for connections periodically
            InvokeRepeating(nameof(CheckConnections), 1f, 1f);
        }
        else
        {
            Debug.LogError("HOST: KCP Transport not found!");
            ipDisplayText.text = "Error: KCP Transport not found";
        }
    }
    
    System.Type FindKcpTransport()
    {
        try
        {
            // Search for KCP transport (what Mirror Basic example uses)
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
            
            // Look for KcpTransport
            foreach (var type in allTypes)
            {
                if (type.Name == "KcpTransport")
                {
                    Debug.Log($"HOST: Found KcpTransport: {type.FullName}");
                    return type;
                }
            }
            
            // If KCP not found, try TelepathyTransport as fallback
            foreach (var type in allTypes)
            {
                if (type.Name == "TelepathyTransport")
                {
                    Debug.Log($"HOST: Using TelepathyTransport fallback: {type.FullName}");
                    return type;
                }
            }
            
            Debug.LogError("HOST: No suitable transport found (tried KCP and Telepathy)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"HOST: Transport search failed: {e.Message}");
        }
        
        return null;
    }
    
    void CheckConnections()
    {
        if (NetworkServer.active)
        {
            int currentConnections = NetworkServer.connections.Count;
            
            // Debug: Show all current connections
            if (currentConnections > 0 && connectedPlayerNames.Count != currentConnections)
            {
                Debug.Log($"HOST: NetworkServer has {currentConnections} connections:");
                foreach (var conn in NetworkServer.connections.Values)
                {
                    Debug.Log($"HOST:   Connection ID: {conn.connectionId}, Address: {conn.address}");
                }
            }
            
            // Reject new connections if game has started
            if (gameStarted)
            {
                foreach (var conn in NetworkServer.connections.Values)
                {
                    bool isNewConnection = !connectedPlayerNames.Any(name => name.Contains(conn.connectionId.ToString()));
                    if (isNewConnection)
                    {
                        Debug.Log($"HOST: Rejecting new connection {conn.connectionId} - game already started");
                        conn.Disconnect();
                        return;
                    }
                }
            }
            
            // Update player list if connection count changed
            if (connectedPlayerNames.Count != currentConnections)
            {
                Debug.Log($"HOST: Connection count changed: {connectedPlayerNames.Count} -> {currentConnections}");
                connectedPlayerNames.Clear();
                foreach (var conn in NetworkServer.connections.Values)
                {
                    connectedPlayerNames.Add($"Player {conn.connectionId}");
                }
                UpdateConnectedPlayersDisplay();
            }
        }
    }
    
    void UpdateConnectedPlayersDisplay()
    {
        if (connectedPlayersText != null)
        {
            string playerList = connectedPlayerNames.Count > 0 
                ? string.Join("\n", connectedPlayerNames) 
                : "Waiting for players...";
            connectedPlayersText.text = $"Connected Players:\n{playerList}";
        }
    }
    
    void StartGame()
    {
        gameStarted = true;
        letsGoButton.GetComponentInChildren<TextMeshProUGUI>().text = "Game Started!";
        letsGoButton.interactable = false;
        
        Debug.Log("HOST: Game started - lobby closed to new players");
    }
    
    string GetLocalIPAddress()
    {
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }
        catch
        {
            return "127.0.0.1";
        }
    }
    
    void CopyIPToClipboard()
    {
        string fullAddress = $"{hostIP}:{port}";
        GUIUtility.systemCopyBuffer = fullAddress;
        
        Debug.Log($"HOST: Copied to clipboard: {fullAddress}");
        StartCoroutine(ShowCopyFeedback());
    }
    
    System.Collections.IEnumerator ShowCopyFeedback()
    {
        string originalText = copyIpButton.GetComponentInChildren<TextMeshProUGUI>().text;
        copyIpButton.GetComponentInChildren<TextMeshProUGUI>().text = "Copied!";
        
        yield return new WaitForSeconds(1.5f);
        
        copyIpButton.GetComponentInChildren<TextMeshProUGUI>().text = originalText;
    }
}