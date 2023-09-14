/*
Copyright (c) Michal "MGZone" Stejskal
Do not distribute!
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class SimpleAnim2 : MonoBehaviour {
#region main
public SpriteRenderer[] render;
public List <Sprite> sprites;
public float waitBetweenFrames = 0.5f;
public int animationChance = 0;
public float randomizedIntroTime = 0f;
public bool looping = true;
public bool destroyOnEnd = false;
public bool disableOnEnd = false;
public bool disableMain = false;
public bool playOnAwake = true,unParentOnAwake = false;
int currentFrame;
int Frames;
public bool isPlaying = false;
int rolled = 1;
Coroutine cor;
IEnumerator playAnim()
{
	if(!isPlaying)
	isPlaying = true;
	yield return new WaitForSeconds(UnityEngine.Random.Range(0,randomizedIntroTime));
		while(currentFrame < Frames)
		{
			yield return new WaitForSeconds(waitBetweenFrames);
			currentFrame++;
			if(currentFrame >= Frames && looping)
			{
				currentFrame = 0;
				rolled = UnityEngine.Random.Range(0,animationChance+1);
				while(rolled != 0)
				{
					rolled = UnityEngine.Random.Range(0,animationChance+1);
					yield return new WaitForSeconds(waitBetweenFrames);
				}
				rolled = 1;
			}
		}
	isPlaying = false;
	if(disableOnEnd)
	{
		if(!disableMain)
		gameObject.SetActive(false);
		else transform.parent.gameObject.SetActive(false);
		EventTriggered();
	}
	else if(destroyOnEnd)
		Destroy(gameObject);
}
	void LateUpdate()
	{
		if(currentFrame<sprites.Count&&isPlaying)
		{
			for(int i = 0; i<render.Length;i++)
			{
				if(render[i]!=null&&render[i].sprite != sprites[currentFrame])
					render[i].sprite = sprites[currentFrame];
			}
		}
	}
	// Use this for initialization
	public void Start()
	{
		if(render.Length == 0||render[0]==null)
		{
			render = new SpriteRenderer[1];
			render[0] = GetComponent<SpriteRenderer>();
		}
		Frames = sprites.Count;
		currentFrame = 0;
		if(disableOnEnd)destroyOnEnd = false;
		
		if(playOnAwake)
		{
			if(cor != null)
			StopCoroutine(cor);
		cor = StartCoroutine(playAnim());
		}
		if(unParentOnAwake)
		{
			transform.SetParent(null);
		}
	}
	void OnEnable ()
	{
		if(playOnAwake||isPlaying)
		{
		if(cor != null)
		StopCoroutine(cor);
		cor = StartCoroutine(playAnim());
		}
	}

	public void StartPlaying()
	{
		if(cor != null)
		StopCoroutine(cor);
	currentFrame = 0;
	if(gameObject.activeInHierarchy)
	cor = StartCoroutine(playAnim());	
	}
	public void StopPlaying()
	{
		isPlaying = false;
		StopCoroutine(cor);
	}
	void OnDisable()
	{
	if(cor != null)
		StopCoroutine(cor);	
	currentFrame = 0;
	}
	#endregion
     [Serializable]
     public class animEndEvent : UnityEvent { }
 
     [SerializeField]
     private animEndEvent animEnd = new animEndEvent();
     
     public animEndEvent onAnimEndEvent { get { return animEnd; } set { animEnd = value; } }
 
     public void EventTriggered()
     {
        onAnimEndEvent.Invoke();
     }
}