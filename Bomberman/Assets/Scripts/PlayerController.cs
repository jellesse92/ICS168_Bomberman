using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    //LATENCY EFFECT
    const float ALLOWED_DISTANCE = 1.5f;        //Radius from original position allowed before lag effect displays
    Vector2 lastPosition;                       //Last position recorded for non-client controlled player
    Vector2 newPosition;                        //Position to move towards 

    //TEMPORARY FUNCTIONALITY
    public int lives = 3;              //Amount of lives player has
    public int score = 0;       //Player's score 
    int bombs = 0;              //Number of bombs player has dropped

    //PLAYER DEFAULT STATS
    const float POWER_UP_TIME = 5.0f;
    const float DEFAULT_SPEED = 6f;
    const float INVINC_TIME = 3f;
    const int DEFAULT_EXPLOSION_MULTIPLIER = 0;
    const int DEFAULT_BOMB_AMOUNT = 1;

    //GUI and Sound
	public AudioClip playerHitSound;
    public Text scoreText;

    //PLAYER INFORMATION
    public bool clientControlled = false; 
    public int player = -1;

    //PLAYER STATS
    float speed = DEFAULT_SPEED;                                            //Speed player moves at
    bool invincible = false;                                                //Player invincibility
    int explosionMultiplier = DEFAULT_EXPLOSION_MULTIPLIER;                 //For increasing explosion size
    int maxBombsAllowed = DEFAULT_BOMB_AMOUNT;                              //Max amount of bombs player can place 
    public GameObject spawnLocation;                                        //Location player set to spawn at



    //EXTERNAL SCRIPTS
    _GameController gcScript;                                                //For repeated access of game controller script

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
        invincible = false;
        Respawn();
        lastPosition = new Vector2(transform.position.x, transform.position.y);
        transform.GetChild(0).gameObject.SetActive(false);
        gcScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<_GameController>();
        UpdateScoreDisplay();
    }

    void FixedUpdate()
    {
        if (clientControlled)
        {
            // Check Input Axes, then use velocity
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            GetComponent<Rigidbody2D>().velocity = new Vector2(h, v) * speed;

            // Set Animation Parameters
            GetComponent<Animator>().SetInteger("X", (int)h);
            GetComponent<Animator>().SetInteger("Y", (int)v);
        }
    }

    //Respawns player at free spawn location
    void Respawn()
    {
        transform.position = new Vector2(spawnLocation.transform.position.x, spawnLocation.transform.position.y);
    }

    public void Damage()
    {
		AudioSource.PlayClipAtPoint (playerHitSound, transform.position);
        StartCoroutine("StartInvincibility");
        Respawn();

        //Restore when activating mode with Lives
        /*
        //TEMPORARY ASPECT
        lives--;
        if (lives < 0) { 
            gcScript.PlayerDied(0);
            gameObject.SetActive(false);
        }
        */
    }

    void OnParticleCollision(GameObject obj)
    {
        if (!invincible && obj.tag == "Explosion" && obj.transform.parent.GetComponent<ExplosionScript>().player != -1)
        {
            Damage();
            gcScript.ReportDeath(obj.transform.parent.GetComponent<ExplosionScript>().player);
            Debug.Log("Died to Player: " + obj.transform.parent.GetComponent<ExplosionScript>().player);
        }
    }

    public void PickUp(string name)
    {
        if (name == "BuffSpeed")
            ActivatePowerUp("PowerUp_Sprint");
        if (name == "BuffExplosion")
            ActivatePowerUp("PowerUp_Explosion");
        if (name == "BuffLimit")
            ActivatePowerUp("PowerUp_Limit");
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

    IEnumerator PowerUp_Limit()
    {
        maxBombsAllowed++;
        yield return new WaitForSeconds(POWER_UP_TIME);
        maxBombsAllowed = DEFAULT_BOMB_AMOUNT;
    }

    public bool BombLimitReached()
    {
        return getBombs() < maxBombsAllowed;
    }

    public int GetExplosionMulti()
    {
        return explosionMultiplier;
    }

    public void UpdateScoreDisplay()
    {
        scoreText.text = score.ToString();
    }

    IEnumerator StartInvincibility()
    {
        invincible = true;
        yield return new WaitForSeconds(INVINC_TIME);
        invincible = false;
    }

    public void UpdatePosition(float x, float y)
    {
        
        newPosition = new Vector2(x, y);
        if(Vector2.Distance(newPosition, lastPosition) > ALLOWED_DISTANCE)
        {
            StopCoroutine("ActivateLagParticle");
            StartCoroutine("ActivateLagParticle");
        }
        lastPosition = new Vector2(x, y);
        transform.position = lastPosition;
        
    }

    IEnumerator MoveToNewPosition()
    {
        transform.position = Vector2.Lerp(lastPosition, newPosition, .5f);
        yield return new WaitForSeconds(.1f);
        transform.position = Vector2.Lerp(lastPosition, newPosition, 1f);

    }

    IEnumerator ActivateLagParticle()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void ActivateLoseScreen()
    {

    }

    

}
