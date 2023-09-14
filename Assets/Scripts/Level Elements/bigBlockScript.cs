using System.Collections;
using UnityEngine;

public class bigBlockScript : MonoBehaviour {
	public GameObject powerup;
	public Sprite[] blocksprites;
	SpriteRenderer render;
	GameData data;
	GameObject power;
	GameObject storedItem;
	Animation anim;
	bool activated = false;
	public int groundLayer = 9;
	// Use this for initialization
	void Awake () {
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		anim = transform.GetChild(0).GetComponent<Animation>();
	}
	public void storeItem(GameObject obj)
	{
		storedItem = obj;
	}
	IEnumerator die(float time)
	{
		BoxCollider2D col = transform.GetChild(0).GetComponent<BoxCollider2D>();
		string name = transform.GetChild(0).name;
		transform.GetChild(0).name = "BlockParent(Clone)";
		if(col!=null)
		col.enabled = false;
		yield return 0;
		gameObject.layer = 14;
		transform.GetChild(0).gameObject.layer = 14;
		if(col!=null)
		{
		col.isTrigger = true;
		col.enabled = true;
		}
		yield return new WaitForSeconds(0.15f);
		render.sprite = blocksprites[1];
		yield return new WaitForSeconds(time-0.15f);
		if(col!=null)
		col.isTrigger = false;
		transform.GetChild(0).name = name;
		gameObject.layer = groundLayer;
		transform.GetChild(0).gameObject.layer = groundLayer;
	}
	public void activate()
	{
		if(!activated)
		{
			activated = true;
			render.sprite = blocksprites[0];
			anim.Play();
			power = Instantiate(powerup,new Vector3(transform.position.x,transform.position.y+1.5f,transform.position.z),Quaternion.identity);
			power.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = storedItem.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
			power.transform.GetChild(0).GetComponent<killOnEnd>().prefab = storedItem;
			power.SetActive(true);
			data.GetComponent<AudioSource>().PlayOneShot(data.sounds[42]);
			StartCoroutine(die(0.3f));
		}
	}
}
