﻿using UnityEngine;
using System.Collections;

public class BombDrop : MonoBehaviour {
    public GameObject bombPrefab;
	public AudioClip dropNoise;
    GameObject networkObject;

    int bombs_placed = 0;

    void Start()
    {
        networkObject = GameObject.FindGameObjectWithTag("Network");
    }
    
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space) && GetComponent<PlayerController>().clientControlled) {
            DropBomb();
            if (GetComponent<PlayerController>().player == 1)   //Player is running the server
            {
                networkObject.GetComponent<Server>().SendBombEvent(0);
            }
            else                                                //Else player is a client
            {
                networkObject.GetComponent<Client>().SendBombDrop();
            }
        }
    }

    public void DropBomb()
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);


        if (GetComponent<PlayerController>().BombLimitReached())
        {
            GameObject bomb = (GameObject)Instantiate(bombPrefab, pos, Quaternion.identity);
            bomb.GetComponent<Bomb>().player = GetComponent<PlayerController>().player;
            bomb.GetComponent<Bomb>().multiplier = GetComponent<PlayerController>().GetExplosionMulti();
            GetComponent<PlayerController>().addBomb();
			AudioSource.PlayClipAtPoint(dropNoise, transform.position);
        }
    }
   
}
