using UnityEngine;
using System.Collections;

public class Host : MonoBehaviour {
    Client cl;
    Server sv;
	private ApplicationManager appManageScript;
    public bool isHost = false;
	// Use this for initialization
	void Start () {
		
       
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
        cl = GameObject.Find("Network_Controller").GetComponent<Client>();
        sv = GameObject.Find("Network_Controller").GetComponent<Server>();
		appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();

        //
        //Call Disconnect Message Here
        //

        if (sv.initialized)
        {
			Debug.Log (appManageScript.myip);
			appManageScript.GetServerResponse ("7:" + appManageScript.myip);
            sv.Disconnect();
           
        }
        else
        {
            cl.Disconnect();
        }

        Application.LoadLevel(0);

    }
}
