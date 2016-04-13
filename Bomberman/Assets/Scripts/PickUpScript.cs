using UnityEngine;
using System.Collections;

public class PickUpScript : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        Randomize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Randomize()
    {
        int ran = Random.Range(0, 3);

        switch (ran)
        {
            case 0:
                transform.tag = "BuffExplosion";
                break;
            case 1:
                transform.tag = "BuffLimit";
                break;
            default:
                transform.tag = "BuffSpeed";
                break;
        }
    }
}
