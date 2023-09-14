using UnityEngine;
using System;

[ExecuteInEditMode]
public class SausageAdderScript : MonoBehaviour {
	public Sprite[] goldSprites;
	public Sprite[] ghostSprites;
	bool isGhost = false;
	SpriteRenderer render;
	SimpleAnim2 anim2;
	[ExecuteInEditMode]
	void OnEnable () {
		transform.SetParent(GameObject.Find("SausagesHold").transform);
	}
	void Awake()
	{
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		anim2 = transform.GetChild(0).GetComponent<SimpleAnim2>();
	}
	public void turnIntoGhost()
	{
		anim2 = transform.GetChild(0).GetComponent<SimpleAnim2>();
		transform.name = "Ghost Sausage";
		isGhost = true;
		for(int i = 0; i<anim2.sprites.Count; i++)
		{
			anim2.sprites[i] = ghostSprites[i];
		}
	}
	void LateUpdate()
	{
		if(Application.isPlaying && !anim2.enabled && isGhost)
		{	
			if(render.sprite!=null)
			{
				string spriteName = render.sprite.name;
				var newSpriteInt = Array.FindIndex(goldSprites, item => item.name == spriteName);
				if(newSpriteInt<ghostSprites.Length)
					render.sprite = ghostSprites[newSpriteInt];
			}
		}
	}
}
