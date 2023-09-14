using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ConveyorBeltScript : MonoBehaviour
{
    [Range(1,100)]
    public int beltLength = 3;
    [Range(-12,12)]
    public int beltSpeed = 1;
    public Sprite[] spritesSingle, spritesMultiple,arrowsSingle,arrowsMultiple;
    SimpleAnim2 anim2,arranim2;
    MGCameraController cam;
    List<Gravity> affectedObjects;
    bool active = false;
    SpriteRenderer render, renderArrow;
    void printer()
	{
		print(gameObject.name+" "+transform.GetInstanceID()+" "+transform.position+"\nBelt Speed: "+beltSpeed);
	}
    void Awake()
    {
        if(Application.isPlaying)
        {
            if(dataShare.debug)
            printer();
            
            anim2 = GetComponent<SimpleAnim2>();
            arranim2 = transform.GetChild(0).GetComponent<SimpleAnim2>();
            render = GetComponent<SpriteRenderer>();
            renderArrow = transform.GetChild(0).GetComponent<SpriteRenderer>();
            anim2.waitBetweenFrames = 0.15f/Mathf.Abs(beltSpeed);
            cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            box.size = new Vector2(beltLength,0.53f);
            box.offset = new Vector2(0,0.235f);
            box.enabled = true;
            anim2.sprites = new List<Sprite>();
            affectedObjects = new List<Gravity>();
            
            if(beltLength==1)
            {
                for(int i = 0;i<spritesSingle.Length;i++)
                {
                    anim2.sprites.Add(spritesSingle[i]);
                }
                for(int i = 0;i<arrowsSingle.Length;i++)
                {
                    arranim2.sprites.Add(arrowsSingle[i]);
                }
            }
            else
            {
                for(int i = 0;i<spritesMultiple.Length;i++)
                {
                    anim2.sprites.Add(spritesMultiple[i]);
                }
                for(int i = 0;i<arrowsSingle.Length;i++)
                {
                    arranim2.sprites.Add(arrowsMultiple[i]);
                }
                arranim2.waitBetweenFrames = anim2.waitBetweenFrames*3f;
            }
            if(!active) toggle(false);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name=="ObjectActivator"&&!active)
        toggle(true);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.name=="ObjectActivator"&&active)
        toggle(false);
    }
    void toggle(bool activated)
    {
        active = activated;
        anim2.enabled = activated;
        arranim2.enabled = activated;
        render.enabled = activated;
        renderArrow.enabled = activated;
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        for(int i = 0; i<other.contacts.Length;i++)
		{
            if(Mathf.Floor(other.contacts[i].point.y+0.1f)>=transform.position.y+0.5f)
            {
                //print("Affecting "+other.transform.name);

                Gravity grav = other.gameObject.GetComponent<Gravity>();
                if(grav!=null&&!other.transform.name.Contains("Burger"))
                {
                    if(other.transform.name=="Player_main")cam.overWriteLockScroll = true;
                    if(!affectedObjects.Contains(grav))
                    affectedObjects.Add(grav);
                }
                break;
            }
        }
    }
    void OnCollisionExit2D(Collision2D other)
    {
        Gravity gravExit = other.gameObject.GetComponent<Gravity>();
        //print("Not Affecting "+other.transform.name);
        for(int i = affectedObjects.Count-1; i>=0;i--)
		{
            if(affectedObjects[i]==null)
            {
                affectedObjects.Remove(affectedObjects[i]);
                continue;
            }
            if(affectedObjects[i]==gravExit)
            {
                if(other.transform.name=="Player_main")
                {
                    if(cam.overWriteLockScroll)
                    cam.overWriteLockScroll = false;
                    other.gameObject.GetComponent<PlayerScript>().convertPosSpeedToVelocity();
                }
                //print("Not affecting "+other.transform.name);

                affectedObjects.Remove(affectedObjects[i]);
                break;
            }
        }
    }
    void FixedUpdate()
    {
        if(active&&affectedObjects.Count!=0)
        {
            float y = transform.position.y;
            for(int i = affectedObjects.Count-1; i>=0;i--)
            {
                if(affectedObjects[i]==null
                ||Mathf.Abs(affectedObjects[i].transform.position.y-y)>1.4f
                ||affectedObjects[i].transform.parent!=null && affectedObjects[i].transform.parent.name=="Player_main")
                {
                    affectedObjects.Remove(affectedObjects[i]);
                    continue;
                }
                
                //push here
                if(affectedObjects[i].transform.childCount==0||affectedObjects[i].transform.GetChild(0).gameObject.activeInHierarchy)
                affectedObjects[i].transform.position+=new Vector3(beltSpeed*Time.deltaTime,0,0);
            }
        }
    }
    #if UNITY_EDITOR
    int lastLength = 0;
    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        renderArrow = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if(lastLength!=beltLength)
        {
            lastLength = beltLength;
            if(beltLength==1)
            {
                render.sprite = spritesSingle[0];
                renderArrow.sprite = arrowsSingle[arrowsSingle.Length-1];
            }
            else
            {
                render.sprite = spritesMultiple[0];
                renderArrow.sprite = arrowsMultiple[arrowsMultiple.Length-1];
            }
            render.size = new Vector2(beltLength,1);
            renderArrow.size = new Vector2(beltLength,0.25f);
        }
        if(beltSpeed == 0)beltSpeed = 1;
        if(beltSpeed<=-1&&!renderArrow.flipX)
        {
            renderArrow.flipX = true;
        }
        else if(beltSpeed>=1&&renderArrow.flipX)
        {
            renderArrow.flipX = false;
        }
    }
    #endif
}
