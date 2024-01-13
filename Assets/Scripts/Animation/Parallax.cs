using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Parallax : MonoBehaviour {
	
	public bool AutoSyncWithCamera = true;
	dataShare dataS;
	public List<Transform> backgrounds;    
	private float [] parallaxScales;   
	public float smoothing = 1f;
	public bool parallaxX = true;
	public bool parallaxY = true;
	
	private Transform cam;             
	public Vector3 previousCamPos;
	bool work = false; 
	public Vector3 startCamPos = Vector3.zero;
	public bool specialparallax = false;
	IEnumerator setValue()
	{
		if(specialparallax)yield return 0;

		previousCamPos = startCamPos;
	}
	
	void Awake () 
	{
		if(Application.isPlaying)
		{
			cam = Camera.main.transform; 
		}

		if(!Application.isPlaying&&startCamPos==Vector3.zero)
		{
			if(transform.name!="SubAreaParallax")
			startCamPos = GameObject.Find("Main Camera").transform.position;
			else
			{
				Vector3 pos = GameObject.Find("Main Camera").transform.position;
				//startCamPos = new Vector3(pos.x,transform.position.y,pos.z);
				startCamPos = pos;
			}
		}
	}

	void Start ()	
	{
		if(!Application.isPlaying&&startCamPos==Vector3.zero)
		{
			if(transform.name!="SubAreaParallax")
			startCamPos = GameObject.Find("Main Camera").transform.position;
			else
			{
				Vector3 pos = GameObject.Find("Main Camera").transform.position;
				//startCamPos = new Vector3(pos.x,transform.position.y,pos.z);
				startCamPos = pos;
			}
		}
		if(Application.isPlaying)
		{
			if(AutoSyncWithCamera)
			{
				startCamPos = GameObject.Find("Main Camera").transform.position;
			}
			if(backgrounds.Count==0)
			{
				for (int i = 0; i<transform.childCount;i++)
				{
		 		 backgrounds.Add(transform.GetChild(i).transform);
				}
			}
			if(Application.isPlaying&&backgrounds.Count==0)
			Destroy(gameObject);
			parallaxScales = new float[backgrounds.Count]; 
		
			for (int i = 0; i < backgrounds.Count; i++)  
			{
				parallaxScales[i] = backgrounds[i].position.z * -1;
			}
				StartCoroutine(setValue());	
		}
	}
	void OnEnable()
	{
		if(Application.isPlaying)
		{
			work = true;
		}
	}
	void OnDisable()
	{
		if(Application.isPlaying)
			work = false;
	}
	void LateUpdate () 
	{
		if(work&&Application.isPlaying)
		{
			for (int i = 0; i < backgrounds.Count; i++)		
			{
				if(backgrounds[i]!=null)
				{
				Vector2 parallax;
				float xValue = 0, yValue = 0;
				if(parallaxX) xValue = (previousCamPos.x - cam.position.x) * parallaxScales[i];
				if(parallaxY) yValue = (previousCamPos.y - cam.position.y) * parallaxScales[i];

				parallax = new Vector2(xValue,yValue);

				Vector3 backgroundTargetPos = new Vector3 (backgrounds[i].position.x + parallax.x, backgrounds[i].position.y + parallax.y, backgrounds[i].position.z);
				
				
				backgrounds[i].position = Vector3.Lerp (backgrounds[i].position, backgroundTargetPos, smoothing * 0.16f);
				}
			}
			previousCamPos = cam.position; 
		}
	}
}