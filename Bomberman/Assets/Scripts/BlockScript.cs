using UnityEngine;
using System.Collections;

public class BlockScript : MonoBehaviour {

    const float EXP_TIME = 3.0f;

    bool isPickUp = false;
    public Sprite[] pickUpSprite;

    void OnParticleCollision(GameObject obj){
        if(obj.tag == "Explosion" && !isPickUp){
            Randomize();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player" && isPickUp)
        {
            col.gameObject.GetComponent<PlayerController>().PickUp(gameObject.tag);
            ResetProperties();
            gameObject.SetActive(false);
        }
    }

    void Randomize()
    {
        int ran = Random.Range(0, 4);

        switch (ran)
        {
            case 0:
                MakePickUp("BuffExplosion", pickUpSprite[1]);
                break;
            case 1:
                MakePickUp("BuffLimit", pickUpSprite[2]);
                break;
            case 2:
            case 3:
                MakePickUp("BuffSpeed", pickUpSprite[3]);
                break;
            default:
                StartCoroutine(WaitToDisable());
                break;
        }
    }

    void MakePickUp(string name, Sprite spr)
    {
        transform.tag = name;
        gameObject.GetComponent<SpriteRenderer>().sprite = spr;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        isPickUp = true;
    }

    void ResetProperties()
    {
        transform.tag = "Destructable";
        gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = pickUpSprite[0];
        isPickUp = false;
    }

    IEnumerator WaitToDisable()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        yield return new WaitForSeconds(EXP_TIME);
        gameObject.SetActive(false);
    }


}
