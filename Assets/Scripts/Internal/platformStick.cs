using UnityEngine;

public class platformStick : MonoBehaviour
{
	public bool playerOnly = true;
    public Transform main;
    public float yPlatformOffset = 0;
	Transform enemies,items;
    void Start()
    {
        if(main==null)
		{
			main = transform;
			enemies = GameObject.Find("Enemies").transform;
			items = GameObject.Find("Items").transform;
		}
    }
    void OnCollisionEnter2D(Collision2D other)
	{
		Transform t = other.transform;
		bool inverted = (t.eulerAngles.z>=180||t.localScale.y==-1)?true:false;
		//Debug.Log(other.transform.name+" "+other.transform.position.y+" platform: "+(transform.position.y+yPlatformOffset+0.45f));
		if((!inverted&&t.position.y>=transform.position.y+yPlatformOffset+0.45f)
		||(inverted&&t.position.y<=transform.position.y-yPlatformOffset-0.45f))
		{
				bool canParent = false;
				if(dataShare.debug)
				Debug.Log(transform.name+" wants to parent: "+other.transform.name+" enabled: "+enabled);
				if(!this.enabled||other.gameObject.name.ToLower().Contains("map"))return;
				switch(other.transform.name)
				{
					default:
					if(!playerOnly)
					{
						if(!other.transform.name.ToLower().Contains("platform"))
						canParent = true;
					}
					break;

					case "Player_main": canParent = true; break;
					case "floatyLady_enemy": break;
				}
				if(canParent)
                other.transform.SetParent(transform);

                if(other.transform.name=="Player_main")
                other.transform.GetComponent<PlayerScript>().correctScaleParented(main.localScale.x,false);
		}
	}
	void OnCollisionExit2D(Collision2D other)
	{
		unparent(other.transform);
	}
	public void unparent(Transform other)
	{
		//Debug.Log(t.name);
		if(other.parent==null) return;
		if(other.parent==transform)
		{
			if(other.name=="Player_main")
			{
                other.GetComponent<PlayerScript>().correctScaleParented(main.localScale.x,true);
			    other.SetParent(null);
			}
			else if(other.tag == "Enemy")
			{
				other.SetParent(enemies);
			}
			else if(other.name.ToLower().Contains("item"))
			{
				other.SetParent(items);
			}
			else other.SetParent(null);
		}
		/*else
		{
			if(other.transform.parent!=null&&other.transform.parent==transform &&gameObject.activeInHierarchy&&other.transform.parent.name!="Player_main"
			||other.transform.parent==null&&other.transform.parent==transform &&gameObject.activeInHierarchy)
            {
                if(other.transform.name=="Player_main")
                other.transform.GetComponent<PlayerScript>().correctScaleParented(main.localScale.x,true);
			    other.transform.SetParent(null);
            }
		}*/
	}
}
