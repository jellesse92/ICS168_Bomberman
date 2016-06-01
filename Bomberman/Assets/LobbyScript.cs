using UnityEngine;
using System.Collections;

public class LobbyScript : MonoBehaviour {

    Server serverScript;

    bool isHost = false;

	// Use this for initialization
	void Start () {
        Time.timeScale = 0.0f;
        if (GameObject.FindGameObjectWithTag("Network").GetComponent<Host>().isHost)
        {
            isHost = true;
            serverScript = GameObject.FindGameObjectWithTag("Network").GetComponent<Server>();
        }
            
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public void PlayGame()
    {
        if (isHost)
        {
            serverScript.StartGame();
        }

    }
}
