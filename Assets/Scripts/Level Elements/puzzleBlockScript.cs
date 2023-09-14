using System.Collections;
using UnityEngine;

public class puzzleBlockScript : MonoBehaviour
{
    public int sequenceID = 1,sequenceIDInvert = 1;
    [Header ("Post animation dead blocks. Left = Correct, Right = Incorrect")]
    public Vector2Int deadblocks = new Vector2Int(2,3);
    public bool goDead = true;
    bool hittable = true;
    Animation anim;
    [Space]
    public Sprite[] sprites;
    Sprite startSprite;
    SpriteRenderer render;
    Coroutine hit;
    puzzleMasterScript pMaster;
    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animation>();
        render = transform.GetChild(0).GetComponent<SpriteRenderer>();
        pMaster = transform.parent.GetComponent<puzzleMasterScript>();
        startSprite = render.sprite;
    }

    public void blockHit(bool inverted)
    {
        if(hittable)
        {
            int usedID = sequenceID;
            if(goDead)hittable = false;
            if(!inverted)
            anim.Play("Block_BounceNoEventanim");
            else
            {
                anim.Play("Block_BounceNoEventInvert");
                usedID = sequenceIDInvert;
            }
            
            if(hit!=null)StopCoroutine(hit);

            if(pMaster.addToSequence(usedID.ToString()))
            {
                if(goDead)
                hit = StartCoroutine(blockGlow(deadblocks.x));
                else hit = StartCoroutine(blockGlowRevert(deadblocks.y));
            }
            else
            {
                if(goDead)
                hit = StartCoroutine(blockGlow(deadblocks.y));
                else hit = StartCoroutine(blockGlowRevert(deadblocks.y));
            }
        }
        else
        {
            pMaster.hitSound();
        }
    }
    public void disableBlock(bool changeSprite)
    {
        if(hittable)
        {
            if(changeSprite)
            render.sprite = sprites[deadblocks.y];
            hittable = false;
        }
    }
    public void resetBlock()
    {
        hittable = true;
        render.sprite = startSprite;
    }
    IEnumerator blockGlow(int spriteID)
    {
        render.sprite = sprites[0];
        yield return new WaitForSeconds(0.1f);
        render.sprite = sprites[spriteID];
    }
    IEnumerator blockGlowRevert(int spriteID)
    {
        render.sprite = sprites[0];
        yield return new WaitForSeconds(0.1f);
        if(!pMaster.failed)
        render.sprite = startSprite;
        else render.sprite = sprites[spriteID];
    }
}
