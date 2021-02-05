﻿using System.Collections;
using System.Collections.Generic;
using GEAR.Localization;
using Mirror;
using TMPro;
using UnityEngine;

public class NetworkStatusStation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScreen;

    [SerializeField] private Light statusLight;
    
    private ListServer _ls;
    
    // Start is called before the first frame update
    void Start()
    {
        _ls = FindObjectOfType<ListServer>();
        InvokeRepeating(nameof(UpdateNetworkInformation), 0, 1);
    }

    private void UpdateNetworkInformation()
    {
        if (!MaroonNetworkManager.Instance.IsActive())
        {
            string msg = LanguageManager.Instance.GetString("ScreenHostJoin");
            DisplayMessage(msg);
            return;
        }
        
        bool clientStarted = MaroonNetworkManager.Instance.mode == NetworkManagerMode.ClientOnly;
        bool hostStarted = NetworkServer.active;
        bool listServerConnected = _ls.GetListServerStatus();
        bool portsMapped = MaroonNetworkManager.Instance.PortsMapped;
        
        if (clientStarted)
        {
            DisplayOnlineInformation();
            statusLight.color = Color.green;
            statusLight.gameObject.SetActive(true);
            MaroonNetworkManager.Instance.ConnectionStatus = ConnectionState.ClientOnline;
            return;
        }

        if (!hostStarted)
        {
            statusLight.gameObject.SetActive(false);
            if (listServerConnected)
            {
                MaroonNetworkManager.Instance.ConnectionStatus = ConnectionState.Offline;
                string msg = LanguageManager.Instance.GetString("ScreenHostJoin");
                DisplayMessage(msg);
            }
            else
            {
                MaroonNetworkManager.Instance.ConnectionStatus = ConnectionState.OfflineNoConnectionToListServer;
                string msg = LanguageManager.Instance.GetString("ScreenNoListServerJoin");
                DisplayMessage(msg);
            }
        }
        else
        {
            statusLight.gameObject.SetActive(true);
            if (listServerConnected && portsMapped)
            {
                MaroonNetworkManager.Instance.ConnectionStatus = ConnectionState.HostOnline;
                DisplayOnlineInformation();
                statusLight.color = Color.green;
            }
            else if (portsMapped) //List server not connected
            {
                MaroonNetworkManager.Instance.ConnectionStatus = ConnectionState.HostOnlineNoConnectionToListServer;
                string msg = LanguageManager.Instance.GetString("ScreenNoListServerHost");
                DisplayMessage(msg);
                statusLight.color = Color.red;
            }
            else //Ports not mapped
            {
                MaroonNetworkManager.Instance.ConnectionStatus = ConnectionState.HostOnlinePortsNotMapped;
                string msg = LanguageManager.Instance.GetString("ScreenNoPortsMapped");
                DisplayMessage(msg);
                statusLight.color = Color.yellow;
            }
        }
    }

    private void DisplayOnlineInformation()
    {
        string information = "";
        information += LanguageManager.Instance.GetString("OnlineInformation1") + " <color=#800000>" + MaroonNetworkManager.Instance.PlayerName + "</color>!\n";
        information += LanguageManager.Instance.GetString("OnlineInformation2") + " <color=#800000>" + MaroonNetworkManager.Instance.ServerName + "</color>.\n";
        if (MaroonNetworkManager.Instance.IsInControl)
        {
            information += LanguageManager.Instance.GetString("OnlineInformation3");
        }
        else
        {
            information += "<color=#800000>" + MaroonNetworkManager.Instance.ClientInControl + "</color> " + LanguageManager.Instance.GetString("OnlineInformation4");
        }
        DisplayMessage(information);
    }
    
    private void DisplayMessage(string message)
    {
        textScreen.text = message;
    }
}