using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {

    public int player = -1;
    public GameObject[] ExplosionParticles;

	public void Resize(float life)
    {
        for (int i = 0; i < 4; i++)
            ExplosionParticles[i].GetComponent<ParticleSystem>().startLifetime = life;
    }
}
