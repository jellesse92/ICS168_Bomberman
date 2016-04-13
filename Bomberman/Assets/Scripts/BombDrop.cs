using UnityEngine;
using System.Collections;

public class BombDrop : MonoBehaviour {
    public GameObject bombPrefab;

    int bombs_placed = 0;
    
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Vector2 pos = transform.position;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);

            
            if (GetComponent<PlayerController>().BombLimitReached())
            {
                GameObject bomb = (GameObject)Instantiate(bombPrefab, pos, Quaternion.identity);
                bomb.GetComponent<Bomb>().player = GetComponent<PlayerController>().player;
                bomb.GetComponent<Bomb>().multiplier = GetComponent<PlayerController>().GetExplosionMulti();
                GetComponent<PlayerController>().addBomb();
            }
        }
    }
   
}
