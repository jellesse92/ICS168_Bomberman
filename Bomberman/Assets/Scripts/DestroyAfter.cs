using UnityEngine;
using System.Collections;

public class DestroyAfter : MonoBehaviour {
    // Time after which the bomb explodes
    public float time = 3;

    // Use this for initialization
    void Start () {
        Destroy(gameObject, time);
    }
}
