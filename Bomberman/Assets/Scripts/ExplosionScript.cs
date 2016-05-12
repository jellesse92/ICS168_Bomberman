using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {

    public int player = -1;
    public GameObject[] ExplosionParticles;
    public int multiplier = 1;
	public AudioClip explosionSound;
    

	public void Resize(float life)
    {
        for (int i = 0; i < 4; i++)
            ExplosionParticles[i].GetComponent<ParticleSystem>().startLifetime = life;
		AudioSource.PlayClipAtPoint (explosionSound, transform.position);
    }

    void OnDestroy()
    {
        PlayerController[] players = GameObject.FindObjectsOfType<PlayerController>();
        foreach (PlayerController gplayer in players)
        {
            if (gplayer.player == player)
            {
                gplayer.removeBomb();
            }
        }
    }
}
