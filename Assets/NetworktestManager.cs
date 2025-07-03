using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public class NetworkTestManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI ipText;
    public Button startServerButton;
    public Button stopServerButton;
    public TMP_InputField connectIpField;
    public Button connectButton;
    public TextMeshProUGUI statusText;
    
    [Header("Network Settings")]
    public int port = 7777;
    
    private string localIP;
    
    void Start()
    {
        // Get IP address
        localIP = GetLocalIP();
        
        // Debug IP detection
        Debug.Log($"Raw IP detected: '{localIP}'");
        Debug.Log($"Port setting: {port}");
        
        // Display IP
        ipText.text = $"Server IP: {localIP}:{port}";
        
        // Setup buttons
        startServerButton.onClick.AddListener(TestMirrorAvailability);
        stopServerButton.onClick.AddListener(StopServer);
        connectButton.onClick.AddListener(TestConnect);
        
        // Initial UI state
        stopServerButton.interactable = false;
        statusText.text = "Ready";
        
        // Change button text for testing
        startServerButton.GetComponentInChildren<TextMeshProUGUI>().text = "Test Mirror";
        connectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Test Connect";
        
        // Test basic socket connectivity
        TestBasicSocket();
        
        Debug.Log($"Network Test initialized. Local IP: {localIP}:{port}");
    }
    
    string GetLocalIP()
    {
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                string ip = endPoint.Address.ToString();
                Debug.Log($"IP detection method: Google DNS route - Result: {ip}");
                return ip;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Primary IP detection failed: {e.Message}");
            
            // Fallback method
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        Debug.Log($"IP detection method: Hostname lookup - Result: {ip}");
                        return ip.ToString();
                    }
                }
            }
            catch (System.Exception e2)
            {
                Debug.LogError($"Fallback IP detection also failed: {e2.Message}");
            }
            
            return "127.0.0.1";
        }
    }
    
    void TestBasicSocket()
    {
        try
        {
            using (Socket testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                Debug.Log("✓ Basic socket creation works");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ Basic socket test failed: {e.Message}");
            statusText.text = "Socket creation failed";
        }
    }
    
    void TestMirrorAvailability()
    {
        Debug.Log("Testing Mirror availability...");
        statusText.text = "Testing Mirror...";
        
        try
        {
            // Try to find Mirror's NetworkManager type
            System.Type nmType = System.Type.GetType("Mirror.NetworkManager, Mirror");
            if (nmType != null)
            {
                Debug.Log("✓ Mirror.NetworkManager found");
                
                // Check for different transport types
                CheckTransportAvailability();
            }
            else
            {
                Debug.LogWarning("✗ Mirror.NetworkManager NOT found");
                statusText.text = "Mirror not installed";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Mirror test failed: {e.Message}");
            statusText.text = $"Mirror test error: {e.Message}";
        }
    }
    
    void CheckTransportAvailability()
    {
        string[] transportNames = {
            // Try different possible namespaces/assemblies
            "Mirror.TelepathyTransport, Mirror",
            "Mirror.TelepathyTransport, Assembly-CSharp",
            "Mirror.TelepathyTransport",
            "TelepathyTransport",
            "Mirror.KcpTransport, Mirror", 
            "Mirror.KcpTransport, Assembly-CSharp",
            "Mirror.KcpTransport",
            "KcpTransport",
            "Mirror.SimpleWebTransport, Mirror",
            "Mirror.SimpleWebTransport, Assembly-CSharp", 
            "Mirror.SimpleWebTransport",
            "SimpleWebTransport"
        };
        
        System.Type foundTransport = null;
        string foundTransportName = "";
        
        foreach (string transportName in transportNames)
        {
            try
            {
                System.Type transportType = System.Type.GetType(transportName);
                if (transportType != null)
                {
                    Debug.Log($"✓ {transportName} found");
                    if (foundTransport == null)
                    {
                        foundTransport = transportType;
                        foundTransportName = transportName;
                    }
                }
                else
                {
                    Debug.Log($"✗ {transportName} NOT found");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"✗ {transportName} check failed: {e.Message}");
            }
        }
        
        // Also try to find all types that inherit from Transport
        try
        {
            Debug.Log("Searching for all Transport types in loaded assemblies...");
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
            
            foreach (var type in allTypes)
            {
                if (type.Name.Contains("Transport") && !type.IsAbstract)
                {
                    Debug.Log($"Found transport-like type: {type.FullName}");
                    if (foundTransport == null && type.Name.Contains("Telepathy"))
                    {
                        foundTransport = type;
                        foundTransportName = type.FullName;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Assembly search failed: {e.Message}");
        }
        
        if (foundTransport != null)
        {
            Debug.Log($"Will use: {foundTransportName}");
            statusText.text = $"Mirror OK - Using {foundTransportName}";
            
            // Try to create components with the found transport
            TryCreateMirrorComponents(foundTransport);
        }
        else
        {
            Debug.LogWarning("No usable transports found");
            statusText.text = "Mirror installed but no transports available";
            
            // Still try to create NetworkManager without transport
            TryCreateMirrorComponents(null);
        }
    }
    
    void TryCreateMirrorComponents(System.Type transportType = null)
    {
        try
        {
            // Try to create NetworkManager using reflection
            System.Type nmType = System.Type.GetType("Mirror.NetworkManager, Mirror");
            GameObject nmGO = new GameObject("TestNetworkManager");
            var networkManager = nmGO.AddComponent(nmType);
            
            Debug.Log("✓ Successfully created Mirror NetworkManager");
            
            if (transportType != null)
            {
                var transport = nmGO.AddComponent(transportType);
                Debug.Log($"✓ Successfully created {transportType.Name}");
                
                // Try to set the transport reference
                var transportField = nmType.GetField("transport");
                if (transportField != null)
                {
                    transportField.SetValue(networkManager, transport);
                    Debug.Log("✓ Transport assigned to NetworkManager");
                    statusText.text = "Mirror fully functional!";
                }
                else
                {
                    Debug.LogWarning("Could not assign transport to NetworkManager");
                    statusText.text = "Mirror components created but not linked";
                }
            }
            else
            {
                statusText.text = "NetworkManager created but no transport";
            }
            
            // Clean up immediately
            DestroyImmediate(nmGO);
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create Mirror components: {e.Message}");
            statusText.text = $"Mirror creation failed: {e.Message}";
        }
    }
    
    void TestConnect()
    {
        string targetIP = connectIpField.text.Trim();
        
        if (string.IsNullOrEmpty(targetIP))
        {
            statusText.text = "Enter IP to test";
            return;
        }
        
        Debug.Log($"Testing basic connection to {targetIP}:{port}");
        statusText.text = $"Testing connection to {targetIP}...";
        
        // Simple socket connection test
        StartCoroutine(TestSocketConnection(targetIP));
    }
    
    System.Collections.IEnumerator TestSocketConnection(string targetIP)
    {
        Socket testSocket = null;
        System.IAsyncResult result = null;
        
        try
        {
            testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            testSocket.ReceiveTimeout = 3000;
            testSocket.SendTimeout = 3000;
            
            result = testSocket.BeginConnect(targetIP, port, null, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to start connection: {e.Message}");
            statusText.text = $"Connection start failed: {e.Message}";
            if (testSocket != null) testSocket.Close();
            yield break;
        }
        
        // Wait for connection (outside try-catch to allow yield)
        float elapsed = 0;
        while (!result.IsCompleted && elapsed < 3f)
        {
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Check result
        try
        {
            if (result.IsCompleted)
            {
                testSocket.EndConnect(result);
                if (testSocket.Connected)
                {
                    Debug.Log($"✓ Successfully connected to {targetIP}:{port}");
                    statusText.text = "Connection successful!";
                }
                else
                {
                    Debug.Log($"✗ Connection to {targetIP}:{port} failed");
                    statusText.text = "Connection failed";
                }
            }
            else
            {
                Debug.Log($"✗ Connection to {targetIP}:{port} timed out");
                statusText.text = "Connection timed out";
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"✗ Connection to {targetIP}:{port} failed: {e.Message}");
            statusText.text = $"Connection failed: {e.Message}";
        }
        finally
        {
            if (testSocket != null)
            {
                testSocket.Close();
            }
        }
    }
    
    void StopServer()
    {
        Debug.Log("Stop function called");
        statusText.text = "Stopped";
    }
}