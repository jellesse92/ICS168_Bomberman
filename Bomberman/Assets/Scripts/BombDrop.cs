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

            if(bombs_placed < GetComponent<PlayerController>().maxBombsAllowed)
            {
                StartCoroutine("PlacedBomb");
                GameObject bomb = (GameObject)Instantiate(bombPrefab, pos, Quaternion.identity);
                bomb.GetComponent<Bomb>().player = GetComponent<PlayerController>().player;
            }

        }
    }

    IEnumerator PlacedBomb()
    {
        bombs_placed++;
        yield return new WaitForSeconds(3.0f);
        bombs_placed--;
    }
}
