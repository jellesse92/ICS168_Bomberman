using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {
    // Explosion Prefab
    public int player = -1;
    int released = 1;
    public GameObject explosionPrefab;

    void Start()
    {
        released = 0;
        GetComponent<Collider2D>().enabled = false;
    }
    void OnTriggerExit2D(Collider2D gco)
    {
        released = 1;
        if (gco.gameObject.GetComponent<PlayerController>().player == player)
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }

    void OnCollisionExit2D(Collision2D gco)
    {
        if(released == 0)
        {
            released = 1;
            //
        }
    }

    void OnDestroy() {
        GameObject explosion = (GameObject)Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<Explosion>().player = player;
    }
}
