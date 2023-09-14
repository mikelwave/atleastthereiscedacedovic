using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class GravityShiftScript : MonoBehaviour {
	public bool upsideDown = false;
	Transform Player;
	Gravity PlayerGravity;
	PlayerScript pScript;
	public Sprite[] sprites = new Sprite[3];
	GameData data;
	SpriteRenderer render;
	ColorCorrectionRamp colorInvert;
	void rotatePlayer(bool toUpsideDown)
	{
		if(pScript.inverted==toUpsideDown||pScript.dead)
		return;

		data.playSound(51,transform.position);
		pScript.grounded = false;
		if(pScript.anim.GetBool("dive")&&pScript.anim.GetBool("gravity"))
		{
			pScript.anim.SetBool("dive",false);
			if(pScript.canInterruptDive)
				pScript.interruptDive = true;
		}
		pScript.diveCooldown = 0;
		pScript.gravityFlipCooldown = 3;
		float startRot = Player.eulerAngles.z;
		float targetRot = startRot+180;
		if(colorInvert!=null)
		{
			if(toUpsideDown)
			colorInvert.enabled = true;
			else colorInvert.enabled = false;
		}
		if(toUpsideDown)
		Player.position += Vector3.up;
		else Player.position -= Vector3.up;
		Player.eulerAngles=new Vector3(0,0,Mathf.Clamp(Mathf.Round(Player.eulerAngles.z)+targetRot,0,360));
		pScript.flipSprites(!toUpsideDown);
		Vector2 newGrav = PlayerGravity.pushForces;
		if(pScript.heldObject!=null)
		{
			pScript.flipHeldObject(toUpsideDown);
		}
		Vector2 pushForces = new Vector2(newGrav.x,Mathf.Abs(newGrav.y)*(toUpsideDown? 1:-1));
		PlayerGravity.pushForces = pushForces;
		//print("Push forces: "+PlayerGravity.pushForces);
		pScript.inverted = toUpsideDown;
		PlayerGravity.enabled = true;
	}
	// Use this for initialization
	void Start ()
	{
		colorInvert = GameObject.Find("Main Camera").GetComponent<ColorCorrectionRamp>();
		data = GameObject.Find("_GM").GetComponent<GameData>();
		render = transform.GetChild(0).GetComponent<SpriteRenderer>();
		if(Application.isPlaying)
		{
			Player = GameObject.Find("Player_main").transform;
			pScript = Player.GetComponent<PlayerScript>();
			PlayerGravity = Player.GetComponent<Gravity>();
		}
	}
	#if UNITY_EDITOR
	void Update()
	{
		if(!Application.isPlaying)
		{
			if(render==null)
			render = transform.GetChild(0).GetComponent<SpriteRenderer>();
			if(upsideDown&&render.sprite!=sprites[1]&&colorInvert==null)
			{
				render.sprite = sprites[1];
			}
			else if(!upsideDown&&render.sprite!=sprites[0])
			{
				render.sprite = sprites[0];
			}
			else if(upsideDown&&render.sprite!=sprites[2]&&colorInvert!=null)
			{
				render.sprite = sprites[2];
			}
		}
	}
	#endif
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name=="PlayerCollider"&&Time.timeScale!=0)
		{
			if(!upsideDown&&!pScript.inverted)
			{
				rotatePlayer(true);
				//print("playerflip invert");
			}
			else if(upsideDown&&pScript.inverted)
			{
				rotatePlayer(false);
				//print("playerflip normal");
			}
		}
	}
}
