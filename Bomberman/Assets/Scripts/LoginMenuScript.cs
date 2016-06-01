using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class LoginMenuScript : MonoBehaviour {

    [Serializable]
    class serverInfo
    {
        public string ipAddress;
        public string portNumber;
    }


    public GameObject[] gameSelectButtons;
    serverInfo[] gameServerInfo;

    ApplicationManager appManageScript;
    Client clientScript;

	// Use this for initialization
	void Start () {
        appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
        clientScript = GameObject.Find("Network_Controller").GetComponent<Client>();
        gameServerInfo = new serverInfo[10];
	}

    //Deactivates all b
    public void FindGames()
    {
        string serverRes = "";

        foreach (GameObject button in gameSelectButtons)
            button.SetActive(false);

        serverRes = appManageScript.GetServerResponse("5:");

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
                    gameServerInfo[i].ipAddress = gameInfo[1];
                    gameServerInfo[i].portNumber = gameInfo[2];
                }
            }
        }
    }

    public void SetServerInfo(int index)
    {
        int p = 8888;
        int.TryParse(gameServerInfo[index].portNumber, out p);
        clientScript.SetServer(gameServerInfo[index].ipAddress, p);
    }
}
