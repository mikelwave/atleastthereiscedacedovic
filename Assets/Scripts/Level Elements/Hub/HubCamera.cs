using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System;
using UnityEngine.Events;

public class HubCamera : MonoBehaviour 
{
	#region main
	Transform Player;
	public float fadeAnim = 0;
	public float fadeSpeedMultiplier = 1;
	public float damping = 0.1f;
	ContrastStretch contrast;
	ScreenOverlay overlay;
	private Vector3 velocity = Vector3.zero;
	bool fadeScreenIn = false;
	public bool forHub = true;
	public float shakeTimer;
	public float shakeAmount;
	public bool constantShake = false;
	public bool workInStoppedTime = false;
	Vector3 shakeOffset;
	GamepadEffects gamepadEffects;
	public bool startWithFade = false;
	public bool staticCamera = false;

	public BoxCollider2D Bounds;

	private Vector3
		_min,
		_max;
	void Awake()
	{
		gamepadEffects = GameObject.Find("Rebindable Manager").GetComponent<GamepadEffects>();
		if(forHub)
		{
			GameObject g = GameObject.Find("Player_main");
			if(g!=null)
			Player =g.transform;
			if(!staticCamera)
			Bounds = GameObject.Find("WorldHub").GetComponent<BoxCollider2D>();
		}
		contrast = GetComponent<ContrastStretch>();
		overlay = GetComponent<ScreenOverlay>();
		if(Bounds!=null)
		{
			_min = Bounds.bounds.min;
			_max = Bounds.bounds.max;
		}
		if(startWithFade)
		fadeScreen(false);
	}

	void Update()
	{
		if(Player!=null&&Bounds!=null)
		{
			var x = Player.position.x;
			var cameraHalfWidth = GetComponent<Camera>().orthographicSize * ((float) Screen.width / Screen.height);

			x = Mathf.Clamp(x, _min.x + cameraHalfWidth, _max.x - cameraHalfWidth);
			Vector3 targetPoint = new Vector3(x, transform.position.y, transform.position.z);
			transform.position = Vector3.SmoothDamp(transform.position,targetPoint, ref velocity, damping, Mathf.Infinity, Time.unscaledDeltaTime);
		}
		if (shakeTimer > 0||constantShake)
		{
			transform.position = new Vector3 (transform.position.x - shakeOffset.x, transform.position.y - shakeOffset.y, transform.position.z);
			//Debug.Log(transform.position);
			shakeOffset = UnityEngine.Random.insideUnitCircle * shakeAmount;
			transform.position = new Vector3 (transform.position.x + shakeOffset.x, transform.position.y + shakeOffset.y, transform.position.z);
			if(!constantShake||shakeTimer>0)
			shakeTimer -= Time.deltaTime;
		}
		if(shakeTimer<=0&&shakeOffset!=Vector3.zero&&!constantShake)
		{
			transform.position = new Vector3 (transform.position.x - shakeOffset.x, transform.position.y - shakeOffset.y, transform.position.z);
			shakeOffset = Vector3.zero;
			gamepadEffects.setRumble(Vector2.zero);
		}

		if(fadeScreenIn && fadeAnim < 1f)
		{
			fadeAnim+=0.02f*fadeSpeedMultiplier;
			overlay.intensity+=0.02f*fadeSpeedMultiplier;
			contrast.limitMinimum += 0.005f*fadeSpeedMultiplier;
		}
		if(!fadeScreenIn && fadeAnim > 0.0f)
		{
			fadeAnim-=0.02f*fadeSpeedMultiplier;
			overlay.intensity-=0.02f*fadeSpeedMultiplier;
			contrast.limitMinimum -= 0.005f*fadeSpeedMultiplier;
		}
	}
	public void fadeScreen(bool fadeIn)
	{
		//print("Fading: "+fadeIn);
		if(fadeIn)
		{
			LoadEventTriggered();
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
	public void setInstantFade(bool fadeIn)
	{
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
	public void shakeCamera(float shakePwr, float shakeDur){
		shakeAmount = shakePwr;
		float rumblePower = Mathf.Clamp(shakePwr*3,0.0f,1.0f);
		gamepadEffects.setRumble(new Vector2(rumblePower,rumblePower));
		shakeTimer = shakeDur;
	}
	#endregion
    [Serializable]
    public class LoadEvent : UnityEvent { }
 
    [SerializeField]
    private LoadEvent loadEvent = new LoadEvent();
    public LoadEvent onLoadEvent { get { return loadEvent; } set { loadEvent = value; } }
 
    public void LoadEventTriggered()
    {
        onLoadEvent.Invoke();
    }
}