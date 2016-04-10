using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    //TEMPORARY FUNCTIONALITY
    int lives = 3;              //Amount of lives player has

    public int player = -1;
    public float speed = 6;                 //Speed player moves at
    public GameObject spawnLocation;        //Location player set to spawn at
    public GameController gcScript;         //For repeated access of game controller script

    void Awake()
    {
        Respawn();
        gcScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    void FixedUpdate()
    {
        // Check Input Axes, then use velocity
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        GetComponent<Rigidbody2D>().velocity = new Vector2(h, v) * speed;

        // Set Animation Parameters
        GetComponent<Animator>().SetInteger("X", (int)h);
        GetComponent<Animator>().SetInteger("Y", (int)v);
    }

    //Respawns player at free spawn location
    void Respawn()
    {
        transform.position = new Vector2(spawnLocation.transform.position.x, spawnLocation.transform.position.y);
    }

    public void Damage()
    {
        Respawn();
        //TEMPORARY ASPECT
        lives--;
        if (lives < 0) { 
            gcScript.PlayerDied(0);
            gameObject.SetActive(false);
        }
    }
}
