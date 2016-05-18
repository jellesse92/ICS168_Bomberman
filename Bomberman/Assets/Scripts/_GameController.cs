using UnityEngine;
using System.Collections;

public class _GameController : MonoBehaviour {

    public GameObject[] players;    //Tracks player game objects
    int controlledPlayer = -1;                                  //Keeps track of what player current client is
    bool playersJoined = false;


    GameObject networkObject;
    bool isHost = false;

	// Use this for initialization
	void Start () {
        networkObject = GameObject.Find("Network_Controller");
        isHost = networkObject.GetComponent<Host>().isHost;

        if (isHost)
        {
            networkObject.GetComponent<Server>().CreateGame();
        } else
        {
            networkObject.GetComponent<Client>().JoinGame();
        }
    }

    void FixedUpdate()
    {
        if(GetActivePlayers() == 1)
        {
            if (isHost && playersJoined && players[controlledPlayer].activeSelf)
                gameObject.GetComponent<GameEndUIController>().ActivateWinScreen();
            else if (!isHost && players[controlledPlayer].activeSelf)
            {
                gameObject.GetComponent<GameEndUIController>().ActivateWinScreen();
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
		//players [player].GetComponent<PlayerController> ().lives--;
        if (player == controlledPlayer && players[player].GetComponent<PlayerController>().lives <= 0)
        {
            gameObject.GetComponent<GameEndUIController>().ActivateLoseScreen();
            DeactivatePlayer(player);
        }

        if(killer != player)
		    players [killer].GetComponent<PlayerController> ().score++;
        players[killer].GetComponent<PlayerController>().UpdateScoreDisplay();
    }

    public Vector2 GetPlayerPos(int player)
    {
        //Debug.Log(player);
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
        /*
        if (!isHost)
            networkObject.GetComponent<Client>().SendDeath(killer - 1);
        else
        */
        if (isHost)
        {
            UpdateScores(victim - 1, killer - 1);
            networkObject.GetComponent<Server>().SetScoreChanged();
        }
            
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

    public void ActivateLose(int p)
    {
        gameObject.GetComponent<GameEndUIController>().ActivateLoseScreen();
        DeactivatePlayer(p);
    }

    public void PlayerJoined()
    {
        playersJoined = true;
    }
}
