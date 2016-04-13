using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    //TEMPORARY FUNCTIONALITY
    int lives = 3;              //Amount of lives player has
    int bombs = 0;              //Number of bombs player has dropped

    //PLAYER DEFAULT STATS
    const float POWER_UP_TIME = 5.0f;
    const float DEFAULT_SPEED = 6f;
    const int DEFAULT_EXPLOSION_MULTIPLIER = 1;
    const int DEFAULT_BOMB_AMOUNT = 1;

    public int player = -1;

    public float speed = DEFAULT_SPEED;                                     //Speed player moves at
    public int explosionMultiplier = DEFAULT_EXPLOSION_MULTIPLIER;          //For increasing explosion size
    public int maxBombsAllowed = DEFAULT_BOMB_AMOUNT;                       //Max amount of bombs player can place 
    public GameObject spawnLocation;                                        //Location player set to spawn at
    public GameController gcScript;                                         //For repeated access of game controller script

    public int getBombs()
    {
        return bombs;
    }
    public void addBomb()
    {
        bombs++;
    }
    public void removeBomb()
    {
        bombs--;
    }

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

    void OnParticleCollision(GameObject obj)
    {
        if (obj.tag == "Explosion")
        {
            Damage();
            Debug.Log("Died to Player: " + obj.transform.parent.GetComponent<ExplosionScript>().player);
        }
    }

    void OnTriggerEnter2D(Collider2D obj)
    {
        if (obj.tag == "BuffSpeed")
            ActivatePowerUp("PowerUp_Sprint");
        if (obj.tag == "BuffExplosion")
            ActivatePowerUp("PowerUp_Explosion");
    }

    //Activates power up with given buff name
    void ActivatePowerUp(string buff)
    {
        StopCoroutine(buff);                    //Prevents it from returning to default speed due to previous call
        StartCoroutine(buff);                   //in the scenario the player pick ups another speed buff
    }

    //For temporary activation of increased player speed
    IEnumerator PowerUp_Sprint()
    {
        speed = 12f;
        yield return new WaitForSeconds(POWER_UP_TIME);
        speed = DEFAULT_SPEED;
    }

    IEnumerator PowerUp_Explosion()
    {
        explosionMultiplier++;
        yield return new WaitForSeconds(POWER_UP_TIME);
        explosionMultiplier = 1;
    }
}
