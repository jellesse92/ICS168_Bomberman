using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class _GameController : MonoBehaviour {

    const int RANDOM_PICK_RANGE = 4;

    //Player data
    public GameObject[] players;    //Tracks player game objects
    int controlledPlayer = -1;      //Keeps track of what player current client is
    bool playersJoined = false;     //Checks if player joined game for if host

    //Game Elements
    List<GameObject> boxes;
    string pickAssignment;          //Pick up value assignment

    //Timer Variables
    public Text timerText;          //Text for timer
    float timeRemaining = 120.0f;   //Starting time

    //Networking Variables
    GameObject networkObject;
    bool isHost = false;

    ApplicationManager appManageScript;

    // Use this for initialization
    void OnApplicationQuit()
    {
        networkObject.GetComponent<Host>().disconnect();
    }

	void Start () {
        networkObject = GameObject.Find("Network_Controller");
        appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
        isHost = networkObject.GetComponent<Host>().isHost;
        boxes = new List<GameObject>();

        if (isHost)
        {
            networkObject.GetComponent<Server>().CreateGame();
            AssignPickUps();
        }  
        else
        {
            string joinup = "J:" + appManageScript.username + ":" + networkObject.GetComponent<Client>().address + ":" + networkObject.GetComponent<Client>().port.ToString();
            Debug.Log("JOINING GAME... " + joinup);
            string resp = appManageScript.GetServerResponse(joinup);
            networkObject.GetComponent<Client>().JoinGame();
        }
    }

    void Update()
    {
        RunTimer();
    }

    void FixedUpdate()
    {
        if(GetActivePlayers() == 1)
        {
            if(controlledPlayer != -1)
            {
                if (isHost && playersJoined && players[controlledPlayer].activeSelf)
                    gameObject.GetComponent<GameEndUIController>().ActivateWinScreen();
                else if (!isHost && players[controlledPlayer].activeSelf)
                {
                    gameObject.GetComponent<GameEndUIController>().ActivateWinScreen();
                }
            }

                
        }
        
    }

    //Activates given player number
    public void ActivatePlayer(int num)
    {
        players[num].SetActive(true);
        StartCoroutine(players[num].GetComponent<PlayerController>().RespawnWait());
    }

    //Deactivates disconnected player
    public void DeactivatePlayer(int num)
    {
        players[num].SetActive(false);
    }

    //Sets which player is controlled by client
    public void SetPlayer(int n)
    {
        ActivatePlayer(n);
        players[n].GetComponent<PlayerController>().clientControlled = true;
        controlledPlayer = n;
    }

    //Updates uncontrolled player positions
    public void UpdatePlayerPosition(int player, float x, float y)
    {
        if(players[player].activeSelf)
            players[player].GetComponent<PlayerController>().UpdatePosition(x, y);
    }

    //Update Bomb Placement for uncontrolled player
    public void UpdateBombPlace(int player)
    {
        players[player].GetComponent<BombDrop>().DropBomb();
    }

	public void UpdateScores(int player, int killer)
	{
        if(killer != player)
		    players [killer].GetComponent<PlayerController> ().score++;
        players[killer].GetComponent<PlayerController>().UpdateScoreDisplay();
    }

    public Vector2 GetPlayerPos(int player)
    {
        if (player != -1)
        {
            return new Vector2(players[player].transform.position.x, players[player].transform.position.y);
        }
        return new Vector2(0f, 0f);
    }

    public int ControlledPlayer()
    {
        return controlledPlayer;
    }

    public void ReportDeath(int victim, int killer)
    {
        if (isHost)
            UpdateScores(victim - 1, killer - 1);        
    }

    public bool isHosting()
    {
        return isHost;
    }

    public int  GetActivePlayers()
    {
        int active = 0;
        foreach (GameObject p in players)
        {
            if (p.activeSelf)
                active++;
        }
        return active;
    }

    public void ActivateGameOver()
    {
        int finalScore = players[controlledPlayer].GetComponent<PlayerController>().score;
        bool foundTie = false;

        Time.timeScale = 0f;
        

        for(int i = 0; i < 4; i++)
        {
            if(i != controlledPlayer)
            {
                if(players[i].GetComponent<PlayerController>().score > finalScore)
                {
                    gameObject.GetComponent<GameEndUIController>().ActivateLoseScreen();
                    return;
                }
                if (players[i].GetComponent<PlayerController>().score == finalScore)
                    foundTie = true;
            }
        }

        if (!foundTie)
            gameObject.GetComponent<GameEndUIController>().ActivateWinScreen();
        else
            gameObject.GetComponent<GameEndUIController>().ActivateLoseScreen();
    }

    public void PlayerJoined()
    {
        playersJoined = true;
    }

    void RunTimer()
    {
        if(timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = "0" + (int)(timeRemaining / 60) + ":" + ((int)(timeRemaining % 60)).ToString("D2");
        }
        else
        {
            timerText.text = "00:00";
            if (isHost)
            {
                ActivateGameOver();
                networkObject.GetComponent<Server>().SendGameEnd();
            }
        }
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    public void SetTimeRemaining(float t)
    {
        timeRemaining = t;
    }

    public void AssignPickUps()
    {
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        foreach (Transform child in map.transform)
        {
            if(child.tag == "Destructable")
            {
                int random = (int)Random.Range(0, RANDOM_PICK_RANGE);
                child.GetComponent<BlockScript>().SetTag(random);
                pickAssignment += random.ToString();
            }
        }
    }

    public void SetPickUps(string set)
    {
        int i = 0;
        GameObject map = GameObject.FindGameObjectWithTag("Map");

        foreach (Transform child in map.transform)
        {
            if (child.tag == "Destructable")
            {
                int assign;
                int.TryParse(set[i].ToString(), out assign);
                child.GetComponent<BlockScript>().SetTag(assign);
                i++;
            }
        }
    }

    public string GetPickUps()
    {
        return pickAssignment;
    }

}
