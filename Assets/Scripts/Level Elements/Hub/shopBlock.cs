using System.Collections;
using UnityEngine;

public class shopBlock : MonoBehaviour
{
    public int id = 0; //0 = life, 1 = floppy
    public int price = 100;
    bool locked = false;
    GameData data;
    Animation anim;
    Transform flopObj;
    public Sprite AltLife,emptySprite;
    SpriteRenderer r;
    // Start is called before the first frame update
    void St()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
        anim = transform.GetChild(0).GetComponent<Animation>();
        r = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if(transform.childCount>2)
        {
            flopObj = transform.GetChild(2);
        }
        if(id==0&&data.mode==1)
        {
            r.sprite = AltLife;
        }
    }
    void OnEnable()
    {
        if(data==null)
        St();
        StartCoroutine(Iset());
    }
    IEnumerator Iset()
    {
        yield return 0;
        checkEmpty();
    }
    void checkEmpty()
    {
        switch(id)
        {
            default:
            if(data.lives>=99)
            {
                r.sprite = emptySprite;
                locked = true;
            }
            break;
            case 1:
            if(data.floppies>=99)
            {
                r.sprite = emptySprite;
                locked = true;
            }
            break;
        }
    }
    public void shopBlockHit()
    {
        anim.Play("Block_BounceNoEventanim");
        //has enough to buy
        if(data.coins>=price&&!locked)
        {
            data.playSound(95,transform.position);
            data.addCoin(-price,false);
            data.saveCoin();
            purchaseItem();
        }
        else data.playSound(locked? 1:96,transform.position); //not enough money
    }
    void purchaseItem()
    {
        switch(id)
        {
            default: //life
            data.addLives(1);
            Vector3 tr = transform.position;
            if(data.mode!=1)
            data.ScorePopUp(new Vector3(tr.x,tr.y,tr.z),"1up",new Color32(133,251,124,255));
            else data.ScorePopUp(new Vector3(tr.x,tr.y,tr.z),"ALT1up",new Color32(133,251,124,255));
            data.saveLives();
            break;
            case 1: //floppy
            data.addFloppy(1,false);
            data.playSoundStatic(34);
            if(flopObj!=null)flopObj.gameObject.SetActive(true);
            data.saveFloppies();
            break;
        }
        checkEmpty();
    }
}
