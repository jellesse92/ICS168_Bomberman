using UnityEngine;
using System.Collections;

public class _GameController : MonoBehaviour {

    public GameObject[] players;    //Tracks player game objects
    int controlledPlayer = -1;                                  //Keeps track of what player current client is


	// Use this for initialization
	void Start () {
        
        if ( GameObject.Find("Network_Controller").GetComponent<Host>().isHost  == true)
        {
            GameObject.Find("Network_Controller").GetComponent<Server>().CreateGame();
        } else
        {
            GameObject.Find("Network_Controller").GetComponent<Client>().JoinGame();
        }
    }

    //Activates given player number
    public void ActivatePlayer(int num)
    {
        players[num].SetActive(true);
    }

    //Deactivates disconnected player
    public void DeactivatePlayer(int num)
    {
        players[num].SetActive(false);
    }

    //Sets which player is controlled by client
    public void SetPlayer(int n)
    {
        Debug.Log("Setting Player: " + n);
        ActivatePlayer(n);
        players[n].GetComponent<PlayerController>().clientControlled = true;

        controlledPlayer = n;
    }

    //Updates uncontrolled player positions
    public void UpdatePlayerPosition(int player, float x, float y)
    {
        Debug.Log(player);
        players[player].transform.position = new Vector2(x, y);
    }

    //Update Bomb Placement for uncontrolled player
    public void UpdateBombPlace(int player)
    {
        players[player].GetComponent<BombDrop>().DropBomb();
    }

	public void UpdateScores(int player, int killer)
	{
		players [player].GetComponent<PlayerController> ().lives--;
		players [killer].GetComponent<PlayerController> ().score++;
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
}
