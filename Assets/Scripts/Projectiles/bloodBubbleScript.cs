using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class bloodBubbleScript : MonoBehaviour
{
    checkForSemiSolid checker;
    Rigidbody2D rb;
    public bool inverted = false;
    public bool ignoreSemiSolid = false;
    public LayerMask whatIsGround;
    Collider2D col;
    SimpleAnim2 anim2;
    SpriteRenderer render;
    ParticleSystem particle;
    Gravity grav;
    bool activated = false;
    public GameObject bloodSplatter;
    // Start is called before the first frame update
    void Start()
    {
        if(rb==null)
        {
            checker = transform.GetChild(0).GetComponent<checkForSemiSolid>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            anim2 = GetComponent<SimpleAnim2>();
            render = GetComponent<SpriteRenderer>();
            grav = GetComponent<Gravity>();
            Gravity parentGrav = transform.parent.GetComponent<Gravity>();
            grav.pushForces = parentGrav.pushForces;
            grav.savedPushForces = parentGrav.savedPushForces;
            transform.eulerAngles = transform.parent.eulerAngles;
            particle = transform.GetChild(1).GetComponent<ParticleSystem>();
            render.enabled = true;
            gameObject.SetActive(false);
        }
    }
    public void spawn(int xSide)
    {
        transform.parent = null;
        gameObject.SetActive(true);
        grav.enabled = true;
        int multiplier = 1;
        if(inverted)
        {
            multiplier=-1;
            transform.eulerAngles = new Vector3(0,0,180);
        }

        rb.velocity = new Vector2(xSide,5*multiplier);
    }
    // Update is called once per frame
    void Update()
    {
        if(!activated)
        {
            RaycastHit2D rayDown = Physics2D.Raycast(transform.position,-transform.up,0.1f,whatIsGround);
            if(rayDown.collider!=null&&rayDown.transform.GetComponent<Tilemap>()!=null)
            {
                activated = true;
                Tilemap map = rayDown.transform.GetComponent<Tilemap>();
                //print(map.name);
                StartCoroutine(createBlood(map));
            }
        }
        if(checker!=null)
        {
            if(!inverted)
            {
                if(rb.velocity.y>0&&!ignoreSemiSolid
                ||rb.velocity.y<0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
                {
                    ignoreSemiSolid = true;
                    //Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
                    whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
                    //Debug.Log(gameObject.name+" Ignoring collision "+"Checker inside: "+checker.insideSemiSolid+" RbY: "+rb.velocity.y);
                }
                else if(rb.velocity.y<=0&&ignoreSemiSolid&&checker.insideSemiSolid
                ||rb.velocity.y<=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
                {
                    ignoreSemiSolid = false;
                    //Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
                    whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
                    //Debug.Log(gameObject.name+" Not ignoring collision");
                }
            }
            else
            {
                if(rb.velocity.y<0&&!ignoreSemiSolid
                ||rb.velocity.y>0&&!ignoreSemiSolid&&!checker.insideSemiSolid)
                {
                    ignoreSemiSolid = true;
                    //Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),true);
                    whatIsGround ^= (1 << LayerMask.NameToLayer("semiSolidGround"));
                    //Debug.Log(gameObject.name+" Ignoring collision");
                }
                else if(rb.velocity.y>=0&&ignoreSemiSolid&&checker.insideSemiSolid
                ||rb.velocity.y>=0&&!transform.GetChild(0).gameObject.activeInHierarchy)
                {
                    ignoreSemiSolid = false;
                    //Physics2D.IgnoreCollision(semiSolid, GetComponent<Collider2D>(),false);
                    whatIsGround |= (1 << LayerMask.NameToLayer("semiSolidGround"));
                    //Debug.Log(gameObject.name+" Not ignoring collision");
                }
            }
        }
    }
    IEnumerator createBlood(Tilemap map)
    {
        col.enabled = false;
        anim2.enabled = false;
        render.enabled = false;
        particle.Play();
        Vector3Int posInt = new Vector3Int(Mathf.FloorToInt((transform.position.x)),Mathf.RoundToInt((transform.position.y)),Mathf.RoundToInt(transform.position.z));
        Vector3Int[] points = new Vector3Int[3];
        points[0] = posInt+new Vector3Int(0,-(int)transform.up.y,0);
        points[1] = posInt+new Vector3Int(1,-(int)transform.up.y,0);
        points[2] = posInt+new Vector3Int(-1,-(int)transform.up.y,0);

        for(int i = 0; i<points.Length;i++)
        {
            //print(points[i]);
            if(map.GetTile(points[i])!=null)
            {
                Instantiate(bloodSplatter,points[i]+new Vector3(0.5f,transform.up.y,0),Quaternion.identity);
                if(i==0)
                yield return new WaitForSeconds(0.5f);
            }
        }
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.name);
        /*if(!activated)
        {
            //get tilebase of what was hit, for semi solid only accept collision if velocity < 0 & semi solid checker !=null
            if(!ignoreSemiSolid&&other.name=="SemiSolidMap"&&checker.insideSemiSolid)
            {
                if(!inverted&&rb.velocity.y<0||inverted&&rb.velocity.y>0)
                {
                    activated = true;
                    Tilemap map = other.GetComponent<Tilemap>();
                    StartCoroutine(createBlood(map));
                }
            }
            else if(other.GetComponent<Tilemap>()!=null)
            {
                activated = true;
                Tilemap map = other.GetComponent<Tilemap>();
                StartCoroutine(createBlood(map));
            }
        }*/
    }
}
