using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameEndUIController : MonoBehaviour {

    public GameObject GameEndPanel;
    public Text GameEndText;
    public Text GameEndSubText;
    public GameObject LoseScreen;
    public GameObject DisconnectScreen;
    public GameObject WinScreen;

    public void ActivateLoseScreen()
    {
        GameEndPanel.SetActive(true);
        GameEndText.text = "GAME OVER";
        GameEndSubText.text = "You raging now?";
        LoseScreen.SetActive(true);
    }

    public void ActivateDisconnectScreen()
    {
        GameEndPanel.SetActive(true);
        GameEndText.text = "DISCONNECTED";
        GameEndSubText.text = "You raging now?";
        DisconnectScreen.SetActive(true);
    }

    public void ActivateWinScreen()
    {
        GameEndPanel.SetActive(true);
        GameEndText.text = "YOU WIN";
        GameEndSubText.text = "You're the last ham standing!";
        WinScreen.SetActive(true);
    }

    public void DeactivatePanel()
    {
        GameEndPanel.SetActive(false);
    }

    public void ReturnToMain()
    {
		if (!gameObject.GetComponent<_GameController> ().isHosting () || gameObject.GetComponent<_GameController> ().GetActivePlayers () <= 1) {
			GameObject.Find("Network_Controller").GetComponent<Host> ().disconnect ();
			Application.LoadLevel (2);
		}
		
        else
            GameEndSubText.text = "You're the host. You're not going anywhere that easily.";
    }
}
