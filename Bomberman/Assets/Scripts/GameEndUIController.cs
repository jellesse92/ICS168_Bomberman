using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameEndUIController : MonoBehaviour {

    public GameObject GameEndPanel;
    public Text GameEndText;
    public GameObject LoseScreen;

    public void ActivateLoseScreen()
    {
        GameEndPanel.SetActive(true);
        GameEndText.text = "You raging now?";
        LoseScreen.SetActive(true);
    }

    public void DeactivatePanel()
    {
        GameEndPanel.SetActive(false);
    }

    public void ReturnToMain()
    {
        if (!gameObject.GetComponent<_GameController>().isHosting())
            Application.LoadLevel(0);
        else
            GameEndText.text = "You're the host. You're not going anywhere that easily.";
    }
}
