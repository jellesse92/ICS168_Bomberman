using UnityEngine;
using System.Collections;

public class PickUpScript : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        Randomize();
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<PlayerController>().PickUp(gameObject.tag);
            gameObject.SetActive(false);
        }
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
