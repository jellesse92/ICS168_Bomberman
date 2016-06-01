using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;

public class PanelManager : MonoBehaviour {

	public Animator initiallyOpen;
	public bool loggedIn = false;

	public InputField inputUsername;
	public InputField inputPassword;
	public InputField inputRepassword;

	public InputField loginUsername;
	public InputField loginPassword;

	private int m_OpenParameterId;
	private Animator m_Open;
	private GameObject m_PreviouslySelected;
	
	private string username = "";
	private string password = "";
	private string repassword = "";

	const string k_OpenTransitionName = "Open";
	const string k_ClosedStateName = "Closed";

    const string SALT_VALUE = ":123";

    private ApplicationManager appManageScript;
    private LoginMenuScript loginScript;
    GameObject playPanel;
    private Host serverInfo;

    void Start()
    {
        appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
        serverInfo = GameObject.Find("Network_Controller").GetComponent<Host>();
        loginScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<LoginMenuScript>();
        playPanel = GameObject.FindGameObjectWithTag("PlayButton");
        playPanel.transform.GetChild(0).GetComponent<Image>().material.color = Color.grey;
        playPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.grey;
        //playPanel.



    }

    /*
        PASSWORD ENCRYPTION
        Code from: https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5%28v=vs.110%29.aspx
    */

    static string GetMd5Hash(MD5 md5Hash, string input)
    {
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }

    /*
        PASSWORD ENCRYPTION END
    */

 

    public void OnPlay(int index = 0)
	{
        loginScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<LoginMenuScript>();
        if (serverInfo != null && serverInfo.isHost)
        {
            string serverMsg = "4:" + appManageScript.username + ":" + appManageScript.myip + ":" + serverInfo.serverport.ToString();
            string resp = appManageScript.GetServerResponse(serverMsg).Substring(0, 7);
            Debug.Log("Host Response: " + resp);
            if ( resp == "SUCCESS")
                Application.LoadLevel(1);
        } else
        {
            loginScript.SetServerInfo(index);
            Application.LoadLevel(1);
        }

        
            
	}

	public void OnEnable()
	{
		m_OpenParameterId = Animator.StringToHash (k_OpenTransitionName);

		if (initiallyOpen == null)
			return;

		OpenPanel(initiallyOpen);
	}

    public void OpenConnectGamePanel(Animator anim)
    {
        loginScript.FindGames();
        OpenPanel(anim);
    }

	public void OpenPanel (Animator anim)
	{
        if (anim.name == "Play" && appManageScript.logged_in == false)
        {
            return;
        }
		if (m_Open == anim)
			return;

		anim.gameObject.SetActive(true);
		var newPreviouslySelected = EventSystem.current.currentSelectedGameObject;

		anim.transform.SetAsLastSibling();

		CloseCurrent();

		m_PreviouslySelected = newPreviouslySelected;

		m_Open = anim;
		m_Open.SetBool(m_OpenParameterId, true);

		GameObject go = FindFirstEnabledSelectable(anim.gameObject);

		SetSelected(go);
	}

	static GameObject FindFirstEnabledSelectable (GameObject gameObject)
	{
		GameObject go = null;
		var selectables = gameObject.GetComponentsInChildren<Selectable> (true);
		foreach (var selectable in selectables) {
			if (selectable.IsActive () && selectable.IsInteractable ()) {
				go = selectable.gameObject;
				break;
			}
		}
		return go;
	}

	public void CloseCurrent()
	{
		if (m_Open == null)
			return;

		m_Open.SetBool(m_OpenParameterId, false);
		SetSelected(m_PreviouslySelected);
		StartCoroutine(DisablePanelDeleyed(m_Open));
		m_Open = null;
	}

	public void CreateAccount(GameObject error)
	{
		username = inputUsername.text;
		password = inputPassword.text;
		repassword = inputRepassword.text;
        string serverMsg = "";

        using (MD5 md5Hash = MD5.Create())
        {
            serverMsg = "0:" + username + ":" + GetMd5Hash(md5Hash, password + SALT_VALUE);
        }


        //serverMsg = "0:" + username + ":" + password;

		if ((password != "") && (repassword != "") && (password == repassword) &&
            appManageScript.GetServerResponse(serverMsg).Substring(0, 7) == "SUCCESS")
		{
			//Connect to server. Ask if okay.
			appManageScript.logged_in = true;
            appManageScript.username = username;
            playPanel.transform.GetChild(0).GetComponent<Image>().material.color = Color.white;
            playPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.white;
            error.SetActive(false);
			CloseCurrent();
		} 
		else 
		{
			error.SetActive(true);
		}
	}


    
	public void LogIn(GameObject error)
	{
		password = loginUsername.text;
		username = loginPassword.text;
        string serverMsg = "";

        using (MD5 md5Hash = MD5.Create())
        {
            serverMsg = "1:" + username + ":" + GetMd5Hash(md5Hash, password + SALT_VALUE);
        }

        if (password != "" && appManageScript.GetServerResponse(serverMsg).Substring(0, 7) == "SUCCESS") 
		{
            //Connect to server. Ask if okay.
            appManageScript.logged_in = true;
            appManageScript.username = username;
            playPanel.transform.GetChild(0).GetComponent<Image>().material.color = Color.white;
            playPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.white;
            //Debug.Log (loggedIn);
			error.SetActive(false);
			CloseCurrent ();
		}
		else 
		{
			error.SetActive(true);
		}

	}

	public void OpenIfLoggedIn (Animator anim)
	{
		Debug.Log (loggedIn);
		if (loggedIn)
		{
			OpenPanel(anim);
		}
	}



	IEnumerator DisablePanelDeleyed(Animator anim)
	{
		bool closedStateReached = false;
		bool wantToClose = true;
		while (!closedStateReached && wantToClose)
		{
			if (!anim.IsInTransition(0))
				closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

			wantToClose = !anim.GetBool(m_OpenParameterId);

			yield return new WaitForEndOfFrame();
		}

		if (wantToClose)
			anim.gameObject.SetActive(false);
	}

	private void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
	}
}
