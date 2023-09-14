using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class MGCameraController : MonoBehaviour {

	[Header("Camera controls")]
	[Space]
	public float focusPoint = 3.8f;
	public float turnPoint = 1.9f;
	public float verticalOffset = 2.8f;
	public float damping = 0.1f;
	[Header("Autoscroll")]
	public Vector2 autoscrollDir = Vector2.zero; 
	[Space]
	[Header("target variables")]
	[Space]
	public Transform target;
	PlayerScript playerControl;
	Rigidbody2D targetRigid;
	float targetXVelo;
	private Vector3 velocity = Vector3.zero;
	[HideInInspector]
	public Vector3 targetPoint;
	public bool lockscroll = false;
	public bool lockCamera = false;
	public bool overWriteLockScroll = false;
	public bool alwaysFollowVertically = false;
	Transform pointLeft;
	Transform pointRight;
	Transform pointUp;
	Transform pointDown;
	public bool lockLeft = true,lockRight = true,lockDown = true,lockUp = true;
	public bool hardLockLeft = false,hardLockRight = false,hardLockDown = false,hardLockUp = false;
	bool goingLeft = false;
	public bool scrollingRight = true;
	Vector3 point;
	Vector3 delta;
	Vector3 destination;
	public float shakeTimer;
	public float shakeAmount;
	public bool constantShake = false;
	public bool workInStoppedTime = false;
	Vector3 shakeOffset;
	public float fadeAnim = 0;
	bool fadeScreenIn = false;
	public ContrastStretch contrast;
	public ScreenOverlay overlay;
	GamepadEffects gamepadEffects;
	public bool nukeEvent = false;
	public bool startWithFade = true;
	public bool invert = false;
	public Collider2D usedBounds;
	public bool obeyBounds = true;
	bool snapToCurrentPoint = false;
	bool verticalShakeOnly = false;
	public bool easeShake = false;
	public float fadeAdditive = 0.02f;
	float startFadeAdditive = 0;
	public float nukeLength = 0.1f;
	bool shaking = false;
	public Texture2D[] overlayTextures = new Texture2D[2];
	[HideInInspector] 
	public bool flash = false;
	[Header ("For explicit bounds")]
	public bool explicitStayInsideBounds = false;
	private Vector3
		_min,
		_max;

	public void AssignTarget(Transform targ)
	{
		target = targ;
		targetRigid = targ.GetComponent<Rigidbody2D>();
		//print("New Camera Target: "+targ);
	}
	public void setLockDown(bool toSet)
	{
		lockDown = toSet;
		hardLockDown = toSet;
	}
	public void cameraPointFreeze()
	{
		snapToCurrentPoint = true;
		velocity = Vector2.zero;
	}
	public void lockCam(bool l)
	{
		lockCamera = l;
	}
	public void setDamp(float newDamp)
	{
		damping = newDamp;
	}
	public void LeftLock()
	{
		lockLeft = true;
		hardLockLeft = true;
	}
	public void unlockLeft()
	{
		lockLeft = false;
		hardLockLeft = false;
	}
	public void resetVelocity(bool resetX)
	{
		if(resetX)
		{
			velocity = new Vector2(0,velocity.y);
		}
		else velocity = new Vector2(velocity.x,0);
	}
	public void hardResetVelocity(bool disableOnEnd)
	{
		targetXVelo = 0;
		velocity = new Vector2(0,0);
		delta = new Vector3(0,0,0);
		targetPoint = transform.position;
		destination = targetPoint;
		this.enabled = !disableOnEnd;
	}
	public void flipVertOffset(bool flip)
	{
		if(!flip) verticalOffset = -Mathf.Abs(verticalOffset);
		else verticalOffset = Mathf.Abs(verticalOffset);
		invert = !flip;
	}
	void OnEnable()
	{
		destination = transform.position;
	}
	// Use this for initialization
	void Start ()
	{
		if(target==null)
			target = GameObject.Find("Player_main").transform;
		playerControl = target.GetComponent<PlayerScript>();
		targetRigid = target.GetComponent<Rigidbody2D>();
		startFadeAdditive = fadeAdditive;
		if(contrast==null)
		contrast = GetComponent<ContrastStretch>();
		if(overlay==null)
		{
			overlay = GetComponent<ScreenOverlay>();
			overlayTextures[0] = overlay.texture;
		}

		Vector2 camSize = new Vector2(2f * Camera.main.orthographicSize * Camera.main.aspect, 2f * Camera.main.orthographicSize);

		GameObject pointLeftObj = new GameObject("pointLeft");
		GameObject pointRightObj = new GameObject("pointRight");
		GameObject pointUpObj = new GameObject("pointUp");
		GameObject pointDownObj = new GameObject("pointDown");

		pointLeftObj.transform.parent = transform;
		pointRightObj.transform.parent = transform;
		pointUpObj.transform.parent = transform;
		pointDownObj.transform.parent = transform;

		pointLeft = pointLeftObj.transform;
		pointRight = pointRightObj.transform;
		pointUp = pointUpObj.transform;
		pointDown = pointDownObj.transform;

		pointLeftObj.layer = gameObject.layer;
		pointRightObj.layer = gameObject.layer;
		pointUpObj.layer = gameObject.layer;
		pointDownObj.layer = gameObject.layer;

		pointLeft.localPosition = new Vector2(-camSize.x/2,0);
		pointRight.localPosition = new Vector2(camSize.x/2,0);
		pointUp.localPosition = new Vector2(0,camSize.y/2);
		pointDown.localPosition = new Vector2(0,-camSize.y/2);

		pointLeftObj.AddComponent<BoxCollider2D>();
		pointRightObj.AddComponent<BoxCollider2D>();
		pointUpObj.AddComponent<BoxCollider2D>();
		pointDownObj.AddComponent<BoxCollider2D>();

		pointLeft.GetComponent<BoxCollider2D>().offset = new Vector2(-0.535f,0);
		pointRight.GetComponent<BoxCollider2D>().offset = new Vector2(0.535f,0);
		pointUp.GetComponent<BoxCollider2D>().offset = new Vector2(0,0.535f);
		pointDown.GetComponent<BoxCollider2D>().offset = new Vector2(0,-0.535f);

		pointLeft.GetComponent<BoxCollider2D>().isTrigger = true;
		pointRight.GetComponent<BoxCollider2D>().isTrigger = true;
		pointUp.GetComponent<BoxCollider2D>().isTrigger = true;
		pointDown.GetComponent<BoxCollider2D>().isTrigger = true;
		gamepadEffects = GameObject.Find("Rebindable Manager").GetComponent<GamepadEffects>();
		if(startWithFade)
		fadeScreen(false);
		setAutoScrollLimits(true);
	}
	public void setAutoScrollLimits(bool toggleBlocker)
	{
		if(this!=null)
		if(autoscrollDir==Vector2.zero&&transform.GetChild(0).childCount!=0&&toggleBlocker)
		{
			transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
		}
		if(autoscrollDir!=Vector2.zero)
			{
				if(transform.GetChild(0).childCount!=0 && transform.GetChild(0).GetChild(0)!=null&&toggleBlocker)
				{
					transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
				}
				if(autoscrollDir.x>0)
				{
					transform.GetChild(3).GetComponent<BoxCollider2D>().enabled = false;
					lockLeft = true;
					//Debug.Log("Autoscroll right");
				}
				if(autoscrollDir.x<0)
				{
					transform.GetChild(4).GetComponent<BoxCollider2D>().enabled = false;
					lockRight = true;
					//Debug.Log("Autoscroll left");
				}
				if(autoscrollDir.y<0)
				{
					transform.GetChild(5).GetComponent<BoxCollider2D>().enabled = false;
					lockUp = true;
					//Debug.Log("Autoscroll down");
				}
				if(autoscrollDir.y>0)
				{
					transform.GetChild(6).GetComponent<BoxCollider2D>().enabled = false;
					lockDown = true;
					//Debug.Log("Autoscroll up");
				}
			}
	}
	public void assignValues()
	{
		point = Camera.main.WorldToViewportPoint(targetPoint);
		delta = targetPoint - Camera.main.ViewportToWorldPoint(new Vector3(0.5f,0.5f,point.z));
		destination = transform.position + delta;
		
	}
	void FixedUpdate ()
	{
		if(!workInStoppedTime)
		{
		targetXVelo = targetRigid.velocity.x;
		//If target isn't past the right red point and is moving right.
		//If target isn't past the left red point and is moving left.
		if(target.transform.position.x < transform.position.x+focusPoint && target.localScale.x == 1.0f && targetXVelo >= 0f && !lockLeft && !scrollingRight && playerControl.transform.parent==null
		|| target.transform.position.x > transform.position.x-focusPoint && target.localScale.x == -1.0f && targetXVelo < 0f && !lockRight && scrollingRight && playerControl.transform.parent==null)
		{
			lockscroll = true;
		}
		else
		{
			//If target faces right while going past the right red point
			if(target.transform.position.x > transform.position.x+focusPoint && targetXVelo > 0f && !scrollingRight)
			{
			scrollingRight = true;
			}
			//If target faces left while going past the left red point
			if(target.transform.position.x < transform.position.x-focusPoint && targetXVelo < 0f && scrollingRight)
			{
			scrollingRight = false;
			}
		
			if(target.transform.position.x > transform.position.x-turnPoint && targetXVelo > 0f && scrollingRight && !lockRight
			||target.transform.position.x < transform.position.x+turnPoint && targetXVelo < 0f && !scrollingRight && !lockLeft
			||playerControl.transform.parent!=null)
			lockscroll = false;
		}
		//Normal behaviour
		if(!lockscroll || lockscroll && overWriteLockScroll)
		{
			if(targetXVelo > 0f && goingLeft)goingLeft = false;
			else if(targetXVelo < 0f && !goingLeft)goingLeft = true;
			if(!invert)
			{
				if(playerControl.grounded
				||!playerControl.grounded && lockDown && target.transform.position.y>transform.position.y+verticalOffset && !lockUp //follow up while locked down
				||!playerControl.grounded && !lockDown && target.transform.position.y < playerControl.platHeight //if player below last platform point & not locked down
				||!playerControl.grounded && !lockDown && transform.position.y < playerControl.platHeight+verticalOffset //if cam lower than player height with cam's offset
				||alwaysFollowVertically&&target.transform.position.y>transform.position.y+verticalOffset)
		 		{
			 		if( goingLeft && !lockLeft
					|| !goingLeft && !lockRight)
					 {
						if(!alwaysFollowVertically)
					 	targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),target.position.y+verticalOffset,target.position.z);
						else targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),target.position.y,target.position.z);
					 }
					else if (goingLeft && lockLeft
					||      !goingLeft && lockRight)
						targetPoint = new Vector3(transform.position.x,target.position.y+verticalOffset,target.position.z);
				}
				else
		 		{
			 		if(goingLeft && !lockLeft
			 		||!goingLeft && !lockRight)
					targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),transform.position.y,target.position.z);
			 		else if(goingLeft && lockLeft
					||     !goingLeft && lockRight)
					targetPoint = new Vector3(transform.position.x,transform.position.y,target.position.z);
		 		}
			}
			else
			{
				if(playerControl.grounded
				||!playerControl.grounded && lockDown && target.transform.position.y>transform.position.y+verticalOffset && !lockUp
				||!playerControl.grounded && !lockDown && target.transform.position.y > playerControl.platHeight
				||!playerControl.grounded && !lockDown && transform.position.y < playerControl.platHeight+verticalOffset
				||alwaysFollowVertically&&target.transform.position.y<transform.position.y+verticalOffset)
		 		{
			 		if( goingLeft && !lockLeft
					|| !goingLeft && !lockRight)
					 {
						if(!alwaysFollowVertically)
					 	targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),target.position.y+verticalOffset,target.position.z);
						else targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),target.position.y,target.position.z);
					 }
					else if (goingLeft && lockLeft
					||      !goingLeft && lockRight)
						targetPoint = new Vector3(transform.position.x,target.position.y+verticalOffset,target.position.z);
				}
				else
		 		{
			 		if(goingLeft && !lockLeft
			 		||!goingLeft && !lockRight)
					targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),transform.position.y,target.position.z);
			 		else if(goingLeft && lockLeft
					||     !goingLeft && lockRight)
					targetPoint = new Vector3(transform.position.x,transform.position.y,target.position.z);
		 		}
			}
		}
		//If scroll is locked.
		else if(lockscroll && !overWriteLockScroll)
		{
			if(!invert)
			{
				if(playerControl.grounded
				||!playerControl.grounded && lockDown && target.transform.position.y>transform.position.y+verticalOffset && !lockUp
				||!playerControl.grounded && !lockDown && target.transform.position.y < playerControl.platHeight
				||!playerControl.grounded && !lockDown && transform.position.y < playerControl.platHeight+verticalOffset
				||alwaysFollowVertically&&target.transform.position.y>transform.position.y+verticalOffset)
				{
					if(!alwaysFollowVertically)
					targetPoint = new Vector3(transform.position.x,target.position.y+verticalOffset,target.position.z);
					else targetPoint = new Vector3(transform.position.x,target.position.y,target.position.z);
				}
				else targetPoint = new Vector3(transform.position.x,transform.position.y,target.position.z);
			}
			else
			{
				if(playerControl.grounded
				||!playerControl.grounded && lockDown && target.transform.position.y>transform.position.y+verticalOffset && !lockUp
				||!playerControl.grounded && !lockDown && target.transform.position.y > playerControl.platHeight
				||!playerControl.grounded && !lockDown && transform.position.y < playerControl.platHeight+verticalOffset
				||alwaysFollowVertically&&target.transform.position.y<transform.position.y+verticalOffset)
				{
					if(!alwaysFollowVertically)
					targetPoint = new Vector3(transform.position.x,target.position.y+verticalOffset,target.position.z);
					else targetPoint = new Vector3(transform.position.x,target.position.y,target.position.z);
				}
				else targetPoint = new Vector3(transform.position.x,transform.position.y,target.position.z);
			}
		}
		if(target != null &&!lockCamera)
		{
			if(!snapToCurrentPoint)
			{
				assignValues();
				if(transform.position.x < destination.x && lockRight
				|| transform.position.x > destination.x && lockLeft)
					destination = new Vector3(transform.position.x,destination.y,destination.z);
				if(transform.position.y > destination.y && lockDown
				|| transform.position.y < destination.y && lockUp)
				destination = new Vector3(destination.x,transform.position.y,destination.z);
				
				//transform.position = Vector3.SmoothDamp(transform.position,destination, ref velocity, damping)+new Vector3(autoscrollDir.x,autoscrollDir.y,0);
				Vector3 pos = transform.position;
				float yDiff = 0;
				float dampSub = 0;

				if(target==playerControl.transform&&autoscrollDir.x==0)
				{
					yDiff = Mathf.Clamp(Mathf.Abs(playerControl.transform.position.y-pos.y),0,4.5f);
					dampSub = Mathf.Clamp((damping*((yDiff/6))),0,0.6f);
					if(shakeTimer>0)dampSub/=2;
				}
				transform.position = new Vector3(Mathf.SmoothDamp(pos.x,destination.x,ref velocity.x,damping,Mathf.Infinity,Time.deltaTime)+autoscrollDir.x,
				Mathf.SmoothDamp(pos.y,destination.y,ref velocity.y,damping-dampSub,Mathf.Infinity,Time.deltaTime)+autoscrollDir.y,
				transform.position.z);
			}
			else
			{
				snapToCurrentPoint = false;
				targetPoint = transform.position;
				destination = new Vector3(transform.position.x,transform.position.y,destination.z);
				transform.position = destination;
			}
		}
		//Debug.Log("target has passed the right focus point");
		Debug.DrawLine(new Vector3(transform.position.x+focusPoint,20.0f,0.0f), new Vector3(transform.position.x+focusPoint,-5.0f,0.0f),Color.red);
		Debug.DrawLine(new Vector3(transform.position.x-focusPoint,20.0f,0.0f), new Vector3(transform.position.x-focusPoint,-5.0f,0.0f),Color.red);
		Debug.DrawLine(new Vector3(transform.position.x+turnPoint,20.0f,0.0f), new Vector3(transform.position.x+turnPoint,-5.0f,0.0f),Color.green);
		Debug.DrawLine(new Vector3(transform.position.x-turnPoint,20.0f,0.0f), new Vector3(transform.position.x-turnPoint,-5.0f,0.0f),Color.green);
			//autoScroll
			//if(autoscrollDir!=Vector2.zero)
			//{
				//transform.position+=new Vector3(autoscrollDir.x,autoscrollDir.y,0);
			//}
		}
	}
	IEnumerator nuke(float length)
	{
		if(transform.childCount<8||transform.GetChild(7).name!="ScreenNuke")
		{
			GameObject obj = Instantiate(transform.GetChild(0).gameObject,transform.position,Quaternion.identity);
			obj.transform.SetParent(transform);
			obj.transform.name = "ScreenNuke";
			obj.SetActive(true);
			yield return new WaitForSeconds(length);
			obj.SetActive(false);
		}
		else
		{
			GameObject obj = transform.GetChild(7).gameObject;
			obj.SetActive(true);
			yield return new WaitForSeconds(length);
			obj.SetActive(false);
		}
	}
	void Update () {
		//if(target.name!="Player_main")
		//print(target.name);
		if(explicitStayInsideBounds)
		{
			if(usedBounds!=null)
			{
				_min = usedBounds.bounds.min;
				_max = usedBounds.bounds.max;
				var x = transform.position.x;
				var cameraHalfWidth = GetComponent<Camera>().orthographicSize * ((float) Screen.width / Screen.height);

				x = Mathf.Clamp(x, _min.x + cameraHalfWidth, _max.x - cameraHalfWidth);
				transform.position = new Vector3(x,transform.position.y,transform.position.z);
			}
		}
		if(nukeEvent)
		{
			nukeEvent = false;
			StartCoroutine(nuke(nukeLength));
		}
		if(fadeScreenIn && fadeAnim < 1f)
		{
			fadeAnim+=fadeAdditive;
			overlay.intensity+=fadeAdditive;
			contrast.limitMinimum += 0.01f;
			if(flash&&fadeAnim>=1f)
			{
				flash = false;
				fadeScreenIn = false;
			}
		}
		if(!fadeScreenIn && fadeAnim > 0.0f)
		{
			fadeAnim-=fadeAdditive;
			overlay.intensity-=fadeAdditive;
			contrast.limitMinimum -= 0.01f;
			if(fadeAnim<=0)
			{
				fadeAdditive = startFadeAdditive;
				overlay.texture = overlayTextures[0];
			}
		}
		if (shakeTimer > 0||constantShake)
		{
			if(Time.timeScale!=0)
			{
				if(easeShake)shakeAmount=Mathf.Clamp(shakeAmount-shakeAmount/10f,0,shakeAmount);
				if(!verticalShakeOnly)
				transform.position = new Vector3 (transform.position.x - shakeOffset.x, transform.position.y - shakeOffset.y, transform.position.z);
				else transform.position = new Vector3 (transform.position.x, transform.position.y - shakeOffset.y, transform.position.z);
				//Debug.Log(transform.position);
				shakeOffset = Random.insideUnitCircle * shakeAmount;
				if(!verticalShakeOnly)
				transform.position = new Vector3 (transform.position.x + shakeOffset.x, transform.position.y + shakeOffset.y, transform.position.z);
				else transform.position = new Vector3 (transform.position.x, transform.position.y + shakeOffset.y, transform.position.z);
				if(!constantShake||shakeTimer>0)
				shakeTimer -= Time.deltaTime;
				if(shakeAmount<0.01f)shakeTimer=0;
			}
		}
		if(shakeTimer<=0&&shaking&&!constantShake)
		{
			shaking = false;
			if(easeShake)easeShake = false;
			shakeAmount = 0;
			if(!verticalShakeOnly)
			transform.position = new Vector3 (transform.position.x - shakeOffset.x, transform.position.y - shakeOffset.y, transform.position.z);
			else transform.position = new Vector3 (transform.position.x, transform.position.y - shakeOffset.y, transform.position.z);
			shakeOffset = Vector3.zero;
			verticalShakeOnly = false;
			gamepadEffects.setRumble(Vector2.zero);
		}
		if(workInStoppedTime && Application.isFocused)
		{
		targetXVelo = targetRigid.velocity.x;
		//If target isn't past the right red point and is moving right.
		//If target isn't past the left red point and is moving left.
		if(target.transform.position.x < transform.position.x+focusPoint && target.localScale.x == 1.0f && targetXVelo >= 0f && !lockLeft && !scrollingRight
		|| target.transform.position.x > transform.position.x-focusPoint && target.localScale.x == -1.0f && targetXVelo < 0f && !lockRight && scrollingRight)
		{
		lockscroll = true;
		}
		else
		{
			//If target faces right while going past the right red point
			if(target.transform.position.x > transform.position.x+focusPoint && targetXVelo > 0f && !scrollingRight)
			{
			scrollingRight = true;
			}
			//If target faces left while going past the left red point
			if(target.transform.position.x < transform.position.x-focusPoint && targetXVelo < 0f && scrollingRight)
			{
			scrollingRight = false;
			}
		
		if(target.transform.position.x > transform.position.x-turnPoint && targetXVelo > 0f && scrollingRight && !lockRight||
		   target.transform.position.x < transform.position.x+turnPoint && targetXVelo < 0f && !scrollingRight && !lockLeft)
		lockscroll = false;
		}
	//Normal behaviour
	if(!lockscroll || lockscroll && overWriteLockScroll)
	{
		if(targetXVelo > 0f && goingLeft)goingLeft = false;
		else if(targetXVelo < 0f && !goingLeft)goingLeft = true;
		
		if(playerControl.grounded
		||!playerControl.grounded && !lockDown && target.transform.position.y < playerControl.platHeight
		||!playerControl.grounded && !lockDown && transform.position.y < playerControl.platHeight+verticalOffset)
		 {
			 if( goingLeft && !lockLeft
			 || !goingLeft && !lockRight)
			 targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),target.position.y+verticalOffset,target.position.z);
			 else if (goingLeft && lockLeft
			 ||      !goingLeft && lockRight)
			 targetPoint = new Vector3(transform.position.x,target.position.y+verticalOffset,target.position.z);
		 }
		else
		 {
			 if(goingLeft && !lockLeft
			 ||!goingLeft && !lockRight)
			 targetPoint = new Vector3(target.position.x+(turnPoint*target.localScale.x),transform.position.y,target.position.z);
			 else if(goingLeft && lockLeft
			 ||     !goingLeft && lockRight)
			 targetPoint = new Vector3(transform.position.x,transform.position.y,target.position.z);
		 }
	}
	//If scroll is locked.
	else if(lockscroll && !overWriteLockScroll)
	{
		if(playerControl.grounded
		||!playerControl.grounded && !lockDown && target.transform.position.y < playerControl.platHeight
		||!playerControl.grounded && !lockDown && transform.position.y < playerControl.platHeight+verticalOffset)
								   targetPoint = new Vector3(transform.position.x,target.position.y+verticalOffset,target.position.z);
		else 					   targetPoint = new Vector3(transform.position.x,transform.position.y,target.position.z);
	}
		if(target != null &&!lockCamera)
		{
			assignValues();
			if(transform.position.x < destination.x && lockRight
			|| transform.position.x > destination.x && lockLeft)
				destination = new Vector3(transform.position.x,destination.y,destination.z);
	   		if(transform.position.y > destination.y && lockDown
	   		|| transform.position.y < destination.y && lockUp)
			   destination = new Vector3(destination.x,transform.position.y,destination.z);
			
			//Debug.Log(Time.unscaledDeltaTime);
			//transform.position = destination;
			Vector3 pos = transform.position;
			float yDiff = 0;
			float dampSub = 0;
			
			if(target==playerControl.transform)
			{
				yDiff = Mathf.Clamp(Mathf.Abs(playerControl.transform.position.y-pos.y),0,4.5f);
				dampSub = Mathf.Clamp((damping*((yDiff/6))),0,0.6f);
			}
			//transform.position = Vector3.SmoothDamp(transform.position,destination, ref velocity, damping,Mathf.Infinity,Time.unscaledDeltaTime);
			transform.position = new Vector3(Mathf.SmoothDamp(pos.x,destination.x,ref velocity.x,damping,Mathf.Infinity,Time.unscaledDeltaTime),
			Mathf.SmoothDamp(pos.y,destination.y,ref velocity.y,damping-dampSub,Mathf.Infinity,Time.unscaledDeltaTime),
			transform.position.z);
		}
		//Debug.Log("target has passed the right focus point");
		Debug.DrawLine(new Vector3(transform.position.x+focusPoint,20.0f,0.0f), new Vector3(transform.position.x+focusPoint,-5.0f,0.0f),Color.red);
		Debug.DrawLine(new Vector3(transform.position.x-focusPoint,20.0f,0.0f), new Vector3(transform.position.x-focusPoint,-5.0f,0.0f),Color.red);
		Debug.DrawLine(new Vector3(transform.position.x+turnPoint,20.0f,0.0f), new Vector3(transform.position.x+turnPoint,-5.0f,0.0f),Color.green);
		Debug.DrawLine(new Vector3(transform.position.x-turnPoint,20.0f,0.0f), new Vector3(transform.position.x-turnPoint,-5.0f,0.0f),Color.green);
	}
	}
	public void shakeCamera(float shakePwr, float shakeDur){
		shaking = true;
		verticalShakeOnly = false;
		shakeAmount = shakePwr;
		float rumblePower = Mathf.Clamp(shakePwr*3,0.0f,1.0f);
		if(!playerControl.dead)
		gamepadEffects.setRumble(new Vector2(rumblePower,rumblePower));
		else gamepadEffects.setRumble(Vector2.zero);
		shakeTimer = shakeDur;
	}
	public void shakeCameraVertically(float shakePwr, float shakeDur){
		shaking = true;
		verticalShakeOnly = true;
		shakeAmount = shakePwr;
		float rumblePower = Mathf.Clamp(shakePwr*3,0.0f,1.0f);
		if(!playerControl.dead)
		gamepadEffects.setRumble(new Vector2(rumblePower,0));
		else gamepadEffects.setRumble(Vector2.zero);
		shakeTimer = shakeDur;
	}
	public void fadeScreen(bool fadeIn)
	{
		if(fadeIn)
		{
			contrast.limitMinimum = 0;
			overlay.intensity = 0;
			fadeAnim = 0;
		}
		else
		{
			contrast.limitMinimum = 0.25f;
			overlay.intensity = 1;
			fadeAnim = 1;
		}
		fadeScreenIn = fadeIn;
	}
	public void flashWhite()
	{
		//print("Flash");
		contrast.limitMinimum = 0;
		overlay.intensity = 0;
		fadeAnim = 0;
		overlay.texture = overlayTextures[1];
		flash = true;
		fadeAdditive = 0.1f;
		fadeScreenIn = true;
	}
	public void setInstantFade(bool fadeIn)
	{
		//print("Fade instant: "+fadeIn);
		fadeScreenIn = fadeIn;
		if(fadeIn)
		{
			contrast.limitMinimum = 0.25f;
			overlay.intensity = 1;
			fadeAnim = 1;
		}
		else
		{
			contrast.limitMinimum = 0;
			overlay.intensity = 0;
			fadeAnim = 0;
		}
	}
	public void setPosition(Vector3 pos)
	{
		if(obeyBounds)
		{
		if(usedBounds!=null)
		{
			var cameraHalfWidth = GetComponent<Camera>().orthographicSize * ((float) Screen.width / Screen.height);
			Vector3 positionToSet = new Vector3(
			Mathf.Clamp(pos.x,usedBounds.bounds.min.x+cameraHalfWidth,usedBounds.bounds.max.x-cameraHalfWidth),
			Mathf.Clamp(pos.y,usedBounds.bounds.min.y+ GetComponent<Camera>().orthographicSize,usedBounds.bounds.max.y- GetComponent<Camera>().orthographicSize),
			transform.position.z);
			transform.position = positionToSet;
		}
		else Debug.LogError("No used bounds assigned.");
		}
		else
		{
			Vector3 positionToSet = new Vector3(pos.x,pos.y,transform.position.z);
			transform.position = positionToSet;
			//print("set cam pos");
		}
	}
}
