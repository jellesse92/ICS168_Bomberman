using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour {
	public AudioClip backgroundMusic;
	// Use this for initialization
	void Start () {
	
		AudioSource.PlayClipAtPoint (backgroundMusic, transform.position);
	}
	
	// Update is called once per frame
	void Update () {

	}
}
