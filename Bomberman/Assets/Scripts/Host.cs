using UnityEngine;
using System.Collections;

public class Host : MonoBehaviour {
    Client cl;
    Server sv;
    public bool isHost = false;
	// Use this for initialization
	void Start () {
        cl = GameObject.Find("Network_Controller").GetComponent<Client>();
        sv = GameObject.Find("Network_Controller").GetComponent<Server>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void setHost()
    {
        isHost = true;
    }
    
    public void disconnect()
    {
        if (isHost)
        {
            sv.Disconnect();
        }
        else
        {
            cl.Disconnect();
        }

        
        Application.LoadLevel(0);

    }
}
