using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

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

    private ApplicationManager appManageScript;

    void Start()
    {
        appManageScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
    }

	public void OnPlay()
	{
        string serverMsg = "4:" + username + ":" + appManageScript.address + ":" + appManageScript.port.ToString();
        if(appManageScript.GetServerResponse(serverMsg) == "SUCCESS")
            Application.LoadLevel (1);
            
	}

	public void OnEnable()
	{
		m_OpenParameterId = Animator.StringToHash (k_OpenTransitionName);

		if (initiallyOpen == null)
			return;

		OpenPanel(initiallyOpen);
	}

	public void OpenPanel (Animator anim)
	{
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

	public void CreateAccount()
	{
		username = inputUsername.text;
		password = inputPassword.text;
		repassword = inputRepassword.text;

        string serverMsg = "0:" + username + ":" + password;

		if ((password != "") && (repassword != "") && (password == repassword) &&
            appManageScript.GetServerResponse(serverMsg) == "SUCCESS")
		{
			//Connect to server. Ask if okay.
			loggedIn = true;
			Debug.Log (loggedIn);
			CloseCurrent();
		} 
		else 
		{
			// Give user an Error Message explaining to them how to properly create an account.
		}
	}


    
	public void LogIn()
	{
		password = loginUsername.text;
		username = loginPassword.text;

        string serverMsg = "1:" + username + ":" + password;

        if (password != "" && appManageScript.GetServerResponse(serverMsg) == "SUCCESS") 
		{
			//Connect to server. Ask if okay.
			loggedIn = true;
			Debug.Log (loggedIn);
			CloseCurrent ();
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
