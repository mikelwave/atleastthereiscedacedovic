using System.Collections;
using UnityEngine;

public class switchScript : MonoBehaviour {
	public Sprite pressed;
	Sprite normal;
	GameData data;
	SpriteRenderer render;
	Collider2D col;
	// Use this for initialization
	void Start () {
		data = GameObject.Find("_GM").GetComponent<GameData>();
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		normal = render.sprite;
		col = transform.GetChild(0).GetComponent<Collider2D>();
		transform.GetChild(0).GetComponent<Collider2D>().isTrigger = true;
	}
	public void Activate(bool music)
	{
		//Debug.Log("Activated");
		data.S_Switch(true,0,music);
		data.playSoundStatic(102);
		gameObject.layer = 10;
		if(reActivateCor!=null)StopCoroutine(reActivateCor);
		reActivateCor = StartCoroutine(reActivate());
	}
	Coroutine reActivateCor;
	IEnumerator reActivate()
	{
		col.enabled = false;
		render.sprite = pressed;
		yield return new WaitUntil(()=>data.redSwitchFrames<=0);
		col.enabled = true;
		render.sprite = normal;
		reActivateCor = null;
	}
}
