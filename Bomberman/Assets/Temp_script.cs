using UnityEngine;
using System.Collections;

public class Temp_script : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        if (this.name == "server")
        {
            GameObject.Find("Network_Controller").GetComponent<Server>().CreateGame();
        } else
        {
            Debug.Log("Attempting to connect");
            GameObject.Find("Network_Controller").GetComponent<Client>().JoinGame("72.211.206.39", 7777);
        }
    }
}
