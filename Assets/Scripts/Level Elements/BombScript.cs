using UnityEngine;

public class BombScript : MonoBehaviour
{
    public GameObject ExplosionObj;
    GameData data;
    MGCameraController camControl;
    Rigidbody2D rb;
    public LayerMask whatIsGround;
    public bool plusExplosion = false;
    Vector2[] points;
    bool spawnGrounded = false;
    bool exploding = false;
    public float yOffset = 0;
    public bool canExplodeOnGroundTouch = true;
    // Start is called before the first frame update
    void Start()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
        camControl = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
        rb = GetComponent<Rigidbody2D>();
        if(plusExplosion)
        {
            Vector3Int posInt = new Vector3Int(Mathf.RoundToInt((transform.position.x-0.5f)),Mathf.RoundToInt((transform.position.y-0.5f)),Mathf.RoundToInt(transform.position.z));
			Vector3 spawnPos = new Vector3(posInt.x+0.5f,posInt.y+0.5f,posInt.z);
            points = new Vector2[13];
            points[0] = spawnPos;
            points[1] = spawnPos+new Vector3(1,0,0);
            points[2] = spawnPos+new Vector3(2,0,0);
            points[3] = spawnPos+new Vector3(3,0,0);
            points[4] = spawnPos-new Vector3(1,0,0);
            points[5] = spawnPos-new Vector3(2,0,0);
            points[6] = spawnPos-new Vector3(3,0,0);
            points[7] = spawnPos+new Vector3(0,1,0);
            points[8] = spawnPos+new Vector3(0,2,0);
            points[9] = spawnPos+new Vector3(0,3,0);
            points[10]= spawnPos-new Vector3(0,1,0);
            points[11]= spawnPos-new Vector3(0,2,0);
            points[12]= spawnPos-new Vector3(0,3,0);

            data.addVectorPoints(points);
        }
        else
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position,-transform.up,0.6f,whatIsGround);
            if(ray.collider!=null)
            {
                spawnGrounded = true;
            }
            else
            {
                gameObject.layer = 28;
                transform.GetChild(0).gameObject.layer = 12;
                GetComponent<Collider2D>().isTrigger=true;
            }
        }
    }
    void instantiateExplosion(Vector3 point)
	{
		if(data.explodeTile(point,true))
		{
			GameObject obj;
			obj = Instantiate(ExplosionObj,point,Quaternion.identity);
			obj.name="ScreenNuke";
			obj.SetActive(true);
		}
        else if(data.explodeTile(point+new Vector3(0,yOffset,0),true))
		{
			GameObject obj;
			obj = Instantiate(ExplosionObj,point+new Vector3(0,yOffset,0),Quaternion.identity);
			obj.name="ScreenNuke";
			obj.SetActive(true);
		}
	}
    void instantiateExplosionBlank(Vector3 point)
	{
			GameObject obj;
			obj = Instantiate(ExplosionObj,point+new Vector3(0,yOffset,0),Quaternion.identity);
			obj.name="ScreenNuke";
			obj.SetActive(true);
	}
    // Update is called once per frame
    public void touchGround(bool blank)
    {
        exploding= true;
        Vector3Int posInt = new Vector3Int(Mathf.RoundToInt((transform.position.x-0.5f)),Mathf.RoundToInt((transform.position.y-0.5f)),Mathf.RoundToInt(transform.position.z));
		Vector3 spawnPos = new Vector3(posInt.x+0.5f,posInt.y+0.5f-yOffset,posInt.z);
		//print("Middle: "+posInt);
        if(rb!=null)
        {
		    GetComponent<Gravity>().enabled = false;
            rb.velocity = Vector2.zero;
        }
        if(!blank)
        instantiateExplosion(spawnPos);
        else instantiateExplosionBlank(spawnPos);

        data.playSound(68,transform.position);
		camControl.shakeCamera(0.1f,0.3f);
        Destroy(gameObject);
    }
    void OnTriggerStay2D(Collider2D other)
	{
        if(other.name.Contains("deathZone"))Destroy(gameObject);
        if(canExplodeOnGroundTouch)
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position,-transform.up,0.6f,whatIsGround);
            if(ray.collider!=null&&!spawnGrounded||!spawnGrounded&&other.gameObject.name=="PlayerCollider")
            {
                string s = "";
                if(ray.collider!=null)
                    s = ray.collider.transform.tag;
                if(!exploding)
                {
                    if(s=="Player"||s=="semiSolid")
                    {
                        if(s=="Player")ray.collider.transform.parent.GetComponent<PlayerScript>().Damage(true,false);
                        exploding = true;
                        touchGround(true);
                    }
                    else
                    {
                        if(!exploding)
                        touchGround(false);
                    }
                }
            }
        }
	}
    void OnTriggerExit2D(Collider2D other)
	{
        if(spawnGrounded&&!plusExplosion)
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position,-transform.up,0.6f,whatIsGround);
            if(ray.collider==null)
            {
                EnemyOffScreenDisabler eneoff = GetComponent<EnemyOffScreenDisabler>();
                eneoff.neverSleep = true;
                eneoff.canUnload = false;
                spawnGrounded = false;
                gameObject.layer = 28;
                transform.GetChild(0).gameObject.layer = 12;
                GetComponent<Collider2D>().isTrigger=true;
            }
        }
	}
}
