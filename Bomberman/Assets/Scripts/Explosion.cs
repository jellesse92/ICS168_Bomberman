using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {
    public int player = -1;

    void OnTriggerEnter2D(Collider2D co) {
        if (co.gameObject.tag == "Player")
            co.gameObject.GetComponent<PlayerController>().Damage();
        else if (!co.gameObject.isStatic) { 
            Destroy(co.gameObject);
            if(co.gameObject.tag == "Monster") {
                Debug.Log("Player " + player + " Scored!");
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().PlayerScored(player);
            }
        }
    }
}
