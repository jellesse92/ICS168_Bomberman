using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject GameOverInterface;
    public GameObject EnemyList;

    public void FixedUpdate()
    {
        if(EnemyList.transform.childCount<=0 && !GameOverInterface.activeSelf)
        {
            GameOverInterface.SetActive(true);
            GameOverInterface.GetComponentInChildren<Text>().text = "You Won!";
        }
    }

	public void Reset()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    //EVENTUALLY ADDING FUNCTIONALITY TO ADD SCORE TO OPPOSING PLAYER WHO KILLED
    //Handles player death
    public void PlayerDied(int player)
    {
        //TEMPORARY
        GameOverInterface.SetActive(true);
        GameOverInterface.GetComponentInChildren<Text>().text = "Game Over";
    }

    //Increment player scores
    public void PlayerScored(int player)
    {

    }
}
