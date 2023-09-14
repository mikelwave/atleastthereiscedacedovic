using UnityEngine;

public class areaMusic : MonoBehaviour
{
    GameData data;
    public int musicID = 0;
    bool active = false;
    // Start is called before the first frame update
    void Start()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //print(other.name+" enter");
        if(other.name=="PlayerCollider")
        {
            toggleMusic(true);
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        //print(other.name+" exit");
        if(other.name=="PlayerCollider")
        {
            toggleMusic(false);
        }
    }
    void toggleMusic(bool on)
    {
        if(on) data.changeMusic(false,musicID,true,true,0.35f);
        else data.changeMusic(true,0,false,true,0.35f);
        active = on;
    }
    void OnDisable()
    {
        if(active)data.changeMusic(true,0,false,true,0.35f);
        active = false;
    }
}
