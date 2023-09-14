using UnityEngine;

public class PlayerTimePauser : MonoBehaviour {
	SpriteRenderer capeRender;
	public bool emitDust = false;
	PlayerScript pScript;
	Animator anim;
	void Start()
	{
		pScript = transform.parent.GetComponent<PlayerScript>();
		capeRender = transform.GetChild(2).GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();
	}
	public void setTime(float tim)
	{
		if(!anim.GetBool("Shrink"))
		{
			if(pScript.powerFrames==0)
			Time.timeScale = tim;
			if(tim != 0)
			anim.updateMode = AnimatorUpdateMode.Normal;
			else
			anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		else anim.updateMode = AnimatorUpdateMode.UnscaledTime;

		anim.SetBool("Grow",false);
	}
	public void shrinkPlayer(float tim)
	{
		Time.timeScale = tim;
		//print("shrinked "+Time.timeScale);
		transform.parent.GetComponent<playerSprite>().state = 0;
		anim.SetBool("Shrink",false);
		anim.updateMode = AnimatorUpdateMode.Normal;
		if(pScript.crouching)
		{
			pScript.crouching = false;
			anim.SetBool("crouch",pScript.crouching);
		}
	}
	public void disableRender()
	{
		pScript.render.enabled = false;
		capeRender.enabled = false;
	}
	public void setDrag(float dr){
		transform.parent.GetComponent<Rigidbody2D>().drag = dr;
	}
	public void killPlayer(float tim)
	{
		Time.timeScale = tim;
	}
	public void enableFakeWalk(float direction)
	{
		if(pScript.walkAfterGoal)
		{
			AxisSimulator axis = transform.parent.GetComponent<AxisSimulator>();
			if(direction!=0)
			axis.acceptFakeInputs = true;
			else axis.acceptFakeInputs = false;
			axis.artificialX = direction;
		}
	}
	public void playOutroTune()
	{
		GameObject.Find("_GM").GetComponent<GameData>().changeMusic(false,9,true,true,0.35f);
	}
	public void setCapeRenderOrder(int newOrder)
	{
		capeRender.sortingOrder = newOrder;
	}
}
