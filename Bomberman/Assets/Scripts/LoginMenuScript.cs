using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoginMenuScript : MonoBehaviour {

    public GameObject[] gameSelectButtons;
    string[] ipAddress = new string[8];
    string[] ports = new string[8];

    ApplicationManager appManageScript;
    Client clientScript;

	// Use this for initialization
	void Start () {
        appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
        clientScript = GameObject.Find("Network_Controller").GetComponent<Client>();
        
	}

    //Deactivates all b
    public void FindGames()
    {
        string serverRes = "";

        foreach (GameObject button in gameSelectButtons)
            button.SetActive(false);

        serverRes = appManageScript.GetServerResponse("5:");

        Debug.Log("Getting Server resp: " + serverRes);

        if(serverRes.Substring(0,4) != "NONE")
        {
            string[] gamesAvailable;

            gamesAvailable = serverRes.Trim().Split(new[] { ' '}, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < gamesAvailable.Length; i++)
            {
                if (i >= gameSelectButtons.Length || gameSelectButtons.Length <= 0)
                    return;

                string[] gameInfo = gamesAvailable[i].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                if(gameInfo.Length > 1)
                {
                    gameSelectButtons[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = gameInfo[0];
                    gameSelectButtons[i].SetActive(true);
                    ipAddress[i] = gameInfo[1];
                    ports[i] = gameInfo[2];
                }
            }
        }
    }

    public void SetServerInfo(int index)
    {
        int p = 8888;
        int.TryParse(ports[index].Trim(), out p);
        clientScript.SetServer(ipAddress[index].Trim(), p);
    }
}
