using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class LoginMenuScript : MonoBehaviour {

    public GameObject[] gameSelectButtons;

    ApplicationManager appManageScript;

	// Use this for initialization
	void Start () {
        appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Deactivates all b
    public void FindGames()
    {
        string serverRes = "";
        foreach (GameObject button in gameSelectButtons)
            button.SetActive(false);
        serverRes = appManageScript.GetServerResponse("5:");
        if(serverRes != "NONE")
        {
            string[] games;

            Debug.Log("WHOLE!: " + serverRes);
            
            games = serverRes.Split(new[] { ' '});
            if (games.Length > 0)
                Debug.Log(games[0]);
            if (games.Length > 1)
                Debug.Log(games[1]);
            
            //string[] temp = games[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            //Debug.Log("NAME: " + temp[0]);





            //temp = serverRes.Split(new[] { ':'}, StringSplitOptions.RemoveEmptyEntries);


        }

    }
}
