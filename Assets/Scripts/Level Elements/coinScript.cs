using System.Collections;
using UnityEngine;

public class coinScript : MonoBehaviour {
	bool visible = false;
	public bool activatedBySwitch = false;
	public bool switchActive = false;
	public int switchID = 0;
	public GameObject coinPickup;
	public bool collected = false;
	public bool destroyObject = true;
	public bool setParentToItems = true;
	public bool makeOpaqueOnCollect = false;
	SpriteRenderer render;
	// Use this for initialization
	void Start () {
		if(makeOpaqueOnCollect)
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		if(setParentToItems)
		transform.parent = GameObject.Find("Items").transform;
		if(!visible || activatedBySwitch)
		transform.GetChild(0).gameObject.SetActive(false);
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "ObjectActivator")
		{
		if(activatedBySwitch && switchActive ||!activatedBySwitch)
		transform.GetChild(0).gameObject.SetActive(true);
		visible = true;
		}
		if(other.name == "BlockParent(Clone)" && coinPickup!=null)
		{
			collected = true;
			GameData data = GameObject.Find("_GM").GetComponent<GameData>();
			data.addCoin(1,true);
			data.playSound(0,transform.position);
			GameObject obj = Instantiate(coinPickup,transform.position,Quaternion.identity);
			obj.SetActive(true);
			Destroy(gameObject);
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.name == "ObjectActivator" && !collected)
		{
		transform.GetChild(0).gameObject.SetActive(false);
		visible = false;
		}
	}
	IEnumerator die()
	{
		if(transform.name.Contains("Coin"))
		transform.SetParent(null);
		yield return new WaitForSeconds(1f);
		if(destroyObject)
		Destroy(gameObject);
		else gameObject.SetActive(false);
	}
	public void killCoin()
	{
		collected = true;
		if(makeOpaqueOnCollect)
		render.color = new Color(render.color.r,render.color.b,render.color.g,1);
		transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Player";
		transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 2;
		GetComponent<CircleCollider2D>().enabled = false;
		StartCoroutine(die());
	}
	public void setVisible(bool vis)
	{
		switchActive = vis;
		if(visible)
		{
		transform.GetChild(0).gameObject.SetActive(true);
		}
	}
}
