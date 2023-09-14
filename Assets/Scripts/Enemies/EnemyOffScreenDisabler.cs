using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EnemyOffScreenDisabler : MonoBehaviour {
	#region main
	MovementAI ai;
	Transform child;
	public bool visible = false;
	[HideInInspector]
	public bool canEnableAI = true;
	public bool neverSleep = false;
	public bool enableFlag = true;
	bool insideEnemy = false;
	public bool resetVeloOnDisable = true;
	public bool appearWithFrozenX = false;
	Collider2D col;
	Coroutine unLoadCor;
	public bool canUnload = true;
	public bool insideTrigger = false;
	bool firstUnload = false;
	public float spawnOffset = 0;
	public float despawnWait = 0;
	Rigidbody2D rb;
	// Use this for initialization
	void Start () {
		ai = GetComponent<MovementAI>();
		rb = GetComponent<Rigidbody2D>();
		if(transform.childCount>0)
		child = transform.GetChild(0);
		col = GetComponent<Collider2D>();
		if(!visible)
		toggle(false);
	}
	void OnDisable()
	{
		insideEnemy = false;
		if(child==null)
		Start();
	}
	public void testOffscreenSpawn()
	{
		if(!insideTrigger&&enableFlag)
		enableFlag = true;
	}
	IEnumerator unLoad()
	{
		yield return new WaitUntil(()=>!insideEnemy);
		yield return new WaitForSeconds(despawnWait);
		visible = false;
		toggle(false);
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag=="Enemy"&&other.name.Contains("Enemy")&&gameObject.activeInHierarchy)
		{
			//Debug.Log("Inside "+other.name);
			if(!other.transform.GetChild(0).gameObject.activeInHierarchy)
			insideEnemy = true;
			else insideEnemy = false;
		}
		if(other.name == "ObjectActivator")
		{
			insideTrigger = true;
			if(unLoadCor!=null)
			StopCoroutine(unLoadCor);
			if(enableFlag)
			{
				visible = true;
				toggle(true);
			}
		}
		else if(other.name == "ObjectActivator"&&!enableFlag)
		{
			insideTrigger = true;
			enableFlag = true;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.tag=="Enemy")
		{
			insideEnemy = false;
		}
		if(other.name == "ObjectActivator" && transform.parent != null && transform.parent.name != "Player_main"&&col.enabled&&canUnload&&!neverSleep)
		{
			if(!enableFlag)
			enableFlag = true;
			insideTrigger = false;
			if(gameObject.activeInHierarchy)
			{
				//print(other.name);
				if(unLoadCor!=null)
				StopCoroutine(unLoadCor);
				unLoadCor = StartCoroutine(unLoad());
			}
		}
	}
	public void toggle(bool active)
	{
		if(!active)
		{
			if(!firstUnload)
			{
				transform.position+=Vector3.right*spawnOffset;
				firstUnload = true;
			}
			if(resetVeloOnDisable)
			{
				rb.velocity = new Vector2(0,rb.velocity.y);
				rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
			}
			DisableEventTriggered();
		}
		else if (active)
		{
			if(!appearWithFrozenX)
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
			else rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
			EnableEventTriggered();
		}
		if(ai != null && canEnableAI)
		ai.enabled = active;
		if(child != null)
		child.gameObject.SetActive(active);
		else if(child == null)
		gameObject.GetComponent<SpriteRenderer>().enabled = active;
	}
	#endregion
	//my event
     [Serializable]
     public class DisableEvent : UnityEvent { }
 
     [SerializeField]
     private DisableEvent disableEvent = new DisableEvent();
     public DisableEvent onDisableEvent { get { return disableEvent; } set { disableEvent = value; } }
 
     public void DisableEventTriggered()
     {
         onDisableEvent.Invoke();
     }
	 [Serializable]
     public class EnableEvent : UnityEvent { }
 
     [SerializeField]
     private EnableEvent enableEvent = new EnableEvent();
     public EnableEvent onEnableEvent { get { return enableEvent; } set { enableEvent = value; } }
 
     public void EnableEventTriggered()
     {
         onEnableEvent.Invoke();
     }
}
