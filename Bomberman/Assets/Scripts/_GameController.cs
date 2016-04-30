using UnityEngine;
using System.Collections;

public class _GameController : MonoBehaviour {

    public GameObject[] players;                                //Tracks player game objects


	// Use this for initialization
	void Start () {

        //TESTING
        SetPlayer(0);
        ActivatePlayers(2);
        UpdatePlayerPosition(1, 5f, 0f);
        UpdateBombPlace(1);
	}

    //Activates given number of players for play
    void ActivatePlayers(int num)
    {
        for(int i = 0; i < num; i++)
            players[i].SetActive(true);
    }

    //Sets which player is controlled by client
    void SetPlayer(int n)
    {
        players[n].GetComponent<PlayerController>().clientControlled = true;
    }

    //Updates uncontrolled player positions
    void UpdatePlayerPosition(int player, float x, float y)
    {
        players[player].transform.position = new Vector2(x, y);
    }

    //Update Bomb Placement for uncontrolled player
    void UpdateBombPlace(int player)
    {
        players[player].GetComponent<BombDrop>().DropBomb();
    }
}
