using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class HostNetworkManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI ipDisplayText;
    public Button copyIpButton;
    
    [Header("Network Settings")]
    public int port = 7777;
    
    private string hostIP;
    
    void Start()
    {
        // Show IP immediately when panel opens
        DisplayHostIP();
        
        copyIpButton.onClick.AddListener(CopyIPToClipboard);
    }
    
    void DisplayHostIP()
    {
        // Get local IP address
        hostIP = GetLocalIPAddress();
        
        // Display IP and port immediately
        ipDisplayText.text = $"Share with friends:\n{hostIP}:{port}";
        
        Debug.Log($"Host IP displayed: {hostIP}:{port}");
    }
    
    string GetLocalIPAddress()
    {
        try
        {
            // Get network interfaces with more detail
            var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            
            foreach (var ni in networkInterfaces)
            {
                // Skip non-operational or loopback interfaces
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up || 
                    ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    continue;
                    
                var ipProps = ni.GetIPProperties();
                
                // Prefer interfaces with gateways (real network connections)
                if (ipProps.GatewayAddresses.Count > 0)
                {
                    foreach (var addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return addr.Address.ToString();
                        }
                    }
                }
            }
            
            // Fallback to original method if no gateway-enabled interface found
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting IP address: {e.Message}");
        }
        
        return "127.0.0.1";
    }
    
    void CopyIPToClipboard()
    {
        string fullAddress = $"{hostIP}:{port}";
        GUIUtility.systemCopyBuffer = fullAddress;
        
        Debug.Log($"Copied to clipboard: {fullAddress}");
        
        // Show feedback to user
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