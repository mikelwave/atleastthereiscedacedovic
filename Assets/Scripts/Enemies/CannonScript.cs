using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class CannonScript : MonoBehaviour {

	[Header("Cannon height: ")]
	public int h = 2;
	[Space]
	public Sprite[] cannonSprites = new Sprite[2];
	SpriteRenderer render;
	public GameObject projectile;
	public AudioClip shootSound;
	int framesTilFire = 180,startFrames = 180;
	bool active = false;
	Transform smoke;
	Transform Player;
	bool playerNear = false;
	bool invert = false;
	public Vector2 bounds = new Vector2(-1f,1f);
	public Transform main;
	List <CannonScript> cannonChildren;
	bool parentControlled = false;
	public LayerMask whatIsGround;
	bool checkForObstruct = true;
	public void toggleObstruct(bool t)
	{
		checkForObstruct = t;
	}
	// Use this for initialization
	void Start () {
		if(transform.eulerAngles.z!=0)
		{
			invert = true;
		}
		if(main==null)main = transform;
		render = GetComponent<SpriteRenderer>();
		smoke = transform.GetChild(0);
		Player = GameObject.Find("Player_main").transform;
		if(Application.isPlaying)
		{
			if(transform!=transform.root&&transform.parent.GetComponent<CannonScript>()==null)
			{
				cannonChildren = new List<CannonScript>();
				for(int i = 1; i<transform.childCount;i++)
				{
					CannonScript c = transform.GetChild(i).GetComponent<CannonScript>();
					if(c!=null)
					{
						c.main = transform;
						cannonChildren.Add(c);
					}
				}
			}
			else parentControlled = true;
			if(GetComponent<platformStick>()==null)
			gameObject.AddComponent<platformStick>();

			platformStick pS = GetComponent<platformStick>();
			pS.yPlatformOffset=h-1;
			pS.main = main;

			bounds+= new Vector2(transform.position.x,transform.position.x);
			transform.position = new Vector3(transform.position.x,transform.position.y,-1);
			
			if(render.size.y<2f)
			{
				render.sprite = cannonSprites[0];
				
			}
			else if(render.size.y>=2f)
			{
				render.sprite = cannonSprites[1];
			}
			BoxCollider2D box = GetComponent<BoxCollider2D>();
			box.size = new Vector2(0.95f,h-0.017f);
			box.offset = new Vector2(0,box.size.y/2+0.0155f);
			smoke.transform.localPosition = new Vector3(smoke.transform.localPosition.x,smoke.transform.localPosition.y+h-1,smoke.transform.localPosition.z);
		}
	}
	public void hardMode()
	{
		startFrames = 120;
		framesTilFire = startFrames;
	}
	void fireAll()
	{
		Fire();
		for(int i = 0; i<cannonChildren.Count;i++)
		{
			cannonChildren[i].Fire();
		}
	}
	// Update is called once per frame
	void Update () {
		if(Application.isPlaying)
		{
			if(Player.position.x>=bounds.x&&Player.position.x<bounds.y&&!playerNear)
			{
				playerNear = true;
			}
			else if(Player.position.x<bounds.x||Player.position.x>bounds.y)
			{
				if(playerNear)
					playerNear = false;
			}
			if(!playerNear&&active&&Time.timeScale!=0)
			{
				if(framesTilFire>0&&!parentControlled)
				framesTilFire--;
				if(framesTilFire==0)
				{
					if(checkForObstruct)
					{
						RaycastHit2D ray = Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y+((h-0.5f)*(invert ? -1 : 1))),Vector2.left*transform.localScale.x*(invert ? -1 : 1),1,whatIsGround);
						Debug.DrawLine(transform.position,new Vector3(transform.position.x,transform.position.y+((h-0.5f)*(invert ? -1 : 1)),transform.position.z)+(Vector3.left*transform.localScale.x*(invert ? -1 : 1)),Color.red,2f);
						if(ray.collider==null)
						{
							fireAll();
						}
					}
					else fireAll();
					framesTilFire = startFrames;
				}
			}
		}
		if(!Application.isPlaying)
		{
			if(h==0)
				h=1;
			render.size = new Vector2(1,Mathf.Abs(h));
			if(render.size.y<=1f && render.sprite!=cannonSprites[0])
			{
				render.sprite = cannonSprites[0];
			}
			else if(render.size.y>1f && render.sprite!=cannonSprites[1])
			{
				render.sprite = cannonSprites[1];
			}
		}
	}
	public void setXPosition()
	{
		transform.position = new Vector3((float)System.Math.Ceiling(transform.position.x/0.2f)*0.2f,transform.position.y,transform.position.z);
	}
	void Fire()
	{
		GetComponent<Animator>().SetTrigger("shoot");
		GetComponent<AudioSource>().PlayOneShot(shootSound,1f);
		Vector3 pos = transform.position;
		GameObject obj;
		if(!invert)
		obj = Instantiate(projectile,new Vector3(pos.x,pos.y-0.5f+h,0),Quaternion.identity);
		else obj = Instantiate(projectile,new Vector3(pos.x,pos.y+0.5f-h,0),Quaternion.identity);

		if(main==null)
		main = transform;
		if(main.localScale.x==-1&&!invert)
		{
			//print(main.localScale.x+" main name: "+main.name);
			obj.transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z+180f);
		}
		if(main.localScale.x==1&&invert)
		{
			//print(main.localScale.x+" main name: "+main.name);
			obj.transform.eulerAngles +=new Vector3(0,0,180);
		}
		//print(obj.transform.eulerAngles.z);
		//obj.transform.SetParent(transform.parent);
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&!active)
		{
			active = true;
			if(framesTilFire>90)
			framesTilFire = 90;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator"&&active)
		{
			active = false;
		}
	}
}
