using UnityEngine;

public class dickBoneScript : MonoBehaviour {
	public GameObject flipped;
	public Vector2 flipVelo = new Vector2(1,10);
	Vector2 flipVeloUsed;
	public AudioClip deathSound;
	bool active = false;
	SimpleAnim2 anim2;
	GameData data;
	GameObject obj;
	SpriteRenderer render;
	void Start()
	{
		anim2 = transform.parent.GetComponent<SimpleAnim2>();
		render = transform.parent.GetComponent<SpriteRenderer>();
		render.enabled = anim2.enabled = active;
		data = GameObject.Find("_GM").GetComponent<GameData>();
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//print(other.name);
		if(other.name=="ObjectActivator")
		{
			active = true;
			render.enabled = anim2.enabled = active;
		}
		if(other.name=="ScreenNuke"&&active||other.name =="BlockParent(Clone)"||other.tag=="lKnife")
		{
			obj = Instantiate(flipped,transform.position,Quaternion.identity);
			obj.GetComponent<deadEnemyScript>().invertable = false;
			Vector3 tPos = transform.position;
			Vector3 pos;
			if(transform.parent.eulerAngles.z>=180||transform.localScale.y==-1)
			{
				pos = new Vector3(transform.position.x,transform.position.y-0.5f,transform.position.z);
			}
			else pos = new Vector3(transform.position.x,transform.position.y+0.5f,transform.position.z);
			Transform objTrans = obj.transform;
			objTrans.parent = null;
			objTrans.position = pos;
			objTrans.localScale = transform.parent.localScale;
			objTrans.eulerAngles = transform.parent.eulerAngles;
			obj.GetComponent<SpriteRenderer>().sprite = transform.parent.GetComponent<SpriteRenderer>().sprite;
			data.addScore(200);
			if(other.name!="ScreenNuke")
			{
				data.ScorePopUp(transform.position,"+200",new Color32(255,255,255,255));
				data.GetComponent<AudioSource>().PlayOneShot(deathSound);
			}
			Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
			obj.SetActive(true);
			int rando = Random.Range(0,2);
			if(rando==1)
				flipVeloUsed = new Vector2(-flipVelo.x,flipVelo.y);
			else flipVeloUsed = new Vector2(flipVelo.x,flipVelo.y);
			rb.velocity = flipVeloUsed;
			if(rando==1)
				rb.angularVelocity = 100;
			else rb.angularVelocity = -100;
			Destroy(transform.parent.gameObject);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name=="ObjectActivator")
		{
			active = false;
			render.enabled = anim2.enabled = active;
		}
	}
}
