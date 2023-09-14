using UnityEngine;

public class itemShop : MonoBehaviour
{
    public int id = 0; //0 = cola, 1 = pepper, 4 = axe, 5 = burek, 7 = csgoknife, 8 = lknife
    public int price = 100;
    public int unlockMargin = 0;
    bool locked = false;
    GameData data;
    Animation anim;
    Vector3Int pos;
    public void shopBlockHit()
    {
        anim.Play("Block_BounceNoEventanim");
        //has enough to buy
        if(locked)
        {
            data.playSound(1,transform.position);
            return;
        }
        if(data.coins>=price)
        {
            data.playSound(95,transform.position);
            data.addCoin(-price,false);
            data.saveCoin();
            purchaseItem();
        }
        else data.playSound(96,transform.position); //not enough money
    }
    void purchaseItem()
    {
        data.shopPowerup(id,pos);
    }
    // Start is called before the first frame update
    void Start()
    {
        if(dataShare.totalCompletedLevels<unlockMargin)
            disable();
        
        data = GameObject.Find("_GM").GetComponent<GameData>();
        anim = transform.GetChild(0).GetComponent<Animation>();
        Vector3 p = transform.position;
        pos = new Vector3Int(Mathf.RoundToInt(p.x-0.5f),Mathf.RoundToInt(p.y),0);
    }
    void disable()
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        locked = true;
    }
}
