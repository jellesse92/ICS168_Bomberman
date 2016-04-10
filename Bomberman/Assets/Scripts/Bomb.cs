using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {
    // Explosion Prefab
    public int player = -1;
    public GameObject explosionPrefab;

    void OnDestroy() {
        GameObject explosion = (GameObject)Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<Explosion>().player = player;
    }
}
