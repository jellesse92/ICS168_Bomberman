using UnityEngine;
using System.Collections;

public class Host : MonoBehaviour {

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
}
