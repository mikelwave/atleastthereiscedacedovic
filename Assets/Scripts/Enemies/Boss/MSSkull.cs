using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MSSkull : MonoBehaviour
{
    Gravity grav;
    public Sprite[] sprites;
    public AudioClip[] sounds;
    SpriteRenderer render;
    Collider2D col;
    Rigidbody2D rb;
    GameData data;
    Coroutine dis;
    [Header("Skull Block")]
    public bool skullBlock = false;
    public bool spawnTiles = true;
    public TileBase skullTile;
    public Tilemap map;
    Sprite startSprite;
    bool isWarning = false;
    public GameObject toSpawn;
    IEnumerator skullSpawn()
    {
        isWarning = true;
        render.gameObject.SetActive(true);
        if(skullBlock)
        {
            render.drawMode = SpriteDrawMode.Simple;
            render.transform.localScale = Vector3.one;
        }
        render.size = Vector2.one;
        render.sortingOrder = 16;
        render.sortingLayerName = "UI";
        if(!spawnTiles||skullBlock&&spawnTiles)
        for(int i = 0;i<6;i++)
        {
            if(i%2==0)data.playUnlistedSoundPoint(sounds[0],new Vector3(transform.position.x,transform.position.y,transform.position.z));
            render.sprite = sprites[(int)i%2];
            yield return new WaitForSeconds(0.1f);
        }
        isWarning = false;
        data.playUnlistedSoundPoint(sounds[1],new Vector3(transform.position.x,transform.position.y,transform.position.z));
        transform.position+=Vector3.up*8;
        if(toSpawn!=null)
        {
            toSpawn.transform.localPosition = Vector3.zero;
            toSpawn.transform.SetParent(null);
            toSpawn.SetActive(true);
            Destroy(this.gameObject);
        }
        rb.velocity = new Vector2(0,-grav.maxVelocities.y);
        if(!skullBlock)
        {
            rb.angularVelocity = Random.Range(-1000,1000);
            render.sprite = sprites[Random.Range(2,sprites.Length-1)];
        }
        else
        {
            render.sprite = startSprite;
            render.drawMode = SpriteDrawMode.Tiled;
            render.transform.localScale = Vector3.one;
            render.size = new Vector2(1,2);
        }
        render.sortingOrder = 6;
        render.sortingLayerName = "Default";
        col.enabled = true;
        grav.enabled = true;
    }
    public void kill()
    {
        if(!isWarning)
        dis = StartCoroutine(disable());
        else gameObject.SetActive(false);
    }
    IEnumerator disable()
    {
        data.playUnlistedSoundPoint(sounds[Random.Range(2,4)],transform.position);
        Transform g = transform.GetChild(1);
        g.eulerAngles = Vector3.zero;
        g.localPosition = Vector3.zero;
        //g.position = new Vector3(g.position.x,Mathf.Floor(g.position.y),g.position.z);
        render.gameObject.SetActive(false);
        g.gameObject.SetActive(true);
        col.enabled = false;
        grav.enabled = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        if(skullBlock&&spawnTiles)
        {
            Vector3Int pos = new Vector3Int(Mathf.FloorToInt(transform.position.x),Mathf.FloorToInt(transform.position.y),0);
            map.SetTile(pos,skullTile);
            map.SetTile(pos-Vector3Int.up,skullTile);
        }
        render.sprite = null;
        yield return new WaitUntil(()=>!g.gameObject.activeInHierarchy);
        transform.eulerAngles = Vector3.zero;
        g.localEulerAngles = Vector3.zero;
        g.localPosition = Vector3.zero;
        gameObject.SetActive(false);
        render.sortingOrder = 16;
        render.sortingLayerName = "UI";
        dis = null;
        if(skullBlock&&!spawnTiles)
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(data==null)
        {
            data = GameObject.Find("_GM").GetComponent<GameData>();
            rb = GetComponent<Rigidbody2D>();
            grav = GetComponent<Gravity>();
            col = transform.GetChild(0).GetComponent<Collider2D>();
            render = transform.GetChild(0).GetComponent<SpriteRenderer>();
            grav.enabled = false;
            col.enabled = false;
        }
    }
    void OnEnable()
    {
        if(data==null)Start();
        transform.position = new Vector3(Mathf.Floor(transform.position.x)+0.5f,Mathf.Floor(transform.position.y)+0.51f,transform.position.z);
        if(startSprite==null)startSprite = render.sprite;
        StartCoroutine(skullSpawn());
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(!skullBlock)
        {
            if(other.name=="PlayerCollider")
            other.transform.parent.GetComponent<PlayerScript>().Damage(true,false);
            if(other.tag=="Ground"||other.name=="PlayerCollider")
            {
                if(dis==null)
                dis = StartCoroutine(disable());
            }
        }
    }
    void OnCollisionStay2D(Collision2D other)
    {
        if(skullBlock&&other.gameObject.tag=="Ground")
        {
            if(dis==null)
                dis = StartCoroutine(disable());

        }
    }
}
