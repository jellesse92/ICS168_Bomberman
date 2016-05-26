using UnityEngine;
using System.Collections;

public class BlockScript : MonoBehaviour {

    const float EXP_TIME = 3.0f;

    bool isPickUp = false;
    string tag = "";
    Sprite sprite;
    public Sprite[] pickUpSprite;

    void Start()
    {
        ResetProperties();
    }

    void OnParticleCollision(GameObject obj){
        if (obj.tag == "Explosion" && !isPickUp)
        {
            MakePickUp();
        }
        else if (isPickUp)
            WaitToDisable();
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

    public void SetTag(int n)
    {
        switch (n)
        {
            case 0:
                tag = "BuffExplosion";
                sprite = pickUpSprite[1];
                break;
            case 1:
                tag = "BuffLimit";
                sprite = pickUpSprite[2];
                break;
            case 2:
            case 3:
                tag = "BuffSpeed";
                sprite = pickUpSprite[3];
                break;
            default:
                break;
        }
    }

    void MakePickUp()
    {
        if (tag == "")
            StartCoroutine("WaitToDisable");
        else
        {
            transform.tag = tag;
            gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            isPickUp = true;
        }
    }

    void ResetProperties()
    {
        transform.tag = "Destructable";
        gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = pickUpSprite[0];
        isPickUp = false;
        tag = "";
        sprite = pickUpSprite[0];
    }

    IEnumerator WaitToDisable()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        yield return new WaitForSeconds(EXP_TIME);
        gameObject.SetActive(false);
    }


}
