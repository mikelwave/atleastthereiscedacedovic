using UnityEngine;
using UnityEngine.Tilemaps;

public class bridgeTile : MonoBehaviour
{
    Animator anim;
    GameData data;
    AudioSource asc;
    public Tile tile;
    void Awake()
    {
        if(data==null)
        {
            asc= GetComponent<AudioSource>();
            data = GameObject.Find("_GM").GetComponent<GameData>();
            anim = GetComponent<Animator>();
            gameObject.SetActive(false);
        }
    }
    public void spawn(Vector3Int pos,bool on)
    {
        transform.position = new Vector3(Mathf.RoundToInt(data.ssmap.transform.position.x)+pos.x+0.5f,Mathf.RoundToInt(data.ssmap.transform.position.y)+pos.y+0.5f,Mathf.RoundToInt(data.ssmap.transform.position.z));
        //print(pos);
        if(on&&data.ssmap.GetTile(pos)==null)
        {
            //print("1");
            //print("Bridge tile spawned at "+pos);
            gameObject.SetActive(true);
            //print("on");
            anim.SetTrigger("appear");
        }
        else if (!on)
        {
            //print("2");
            gameObject.SetActive(true);
            anim.SetTrigger("disappear");
        }
    }
    public void turnOff()
    {
        //print("off");
        gameObject.SetActive(false);
    }
    public void setTile(int placeDown)
    {
        Vector3Int pos = new Vector3Int(Mathf.RoundToInt(transform.position.x-0.5f)-Mathf.RoundToInt(data.ssmap.transform.position.x),
                                        Mathf.RoundToInt(transform.position.y-0.5f)-Mathf.RoundToInt(data.ssmap.transform.position.y),
                                        Mathf.RoundToInt(data.ssmap.transform.position.z));
        // 0 - null 
        if(placeDown == 0)
        {
            data.ssmap.SetTile(pos,null);
        }
        // 1 - bridge tile
        else if (placeDown == 1)
        {
            data.ssmap.SetTile(pos,tile);
            gameObject.SetActive(false);
        }
        // 2 - blank tile
        else
        {
            data.ssmap.SetTile(pos,data.replacementBlocks[5]);
        }
    }
    public void playSound()
    {
        if(asc.isPlaying)asc.Stop();
        asc.Play();
    }
}
