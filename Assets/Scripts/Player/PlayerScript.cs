// PlayerScript
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(AxisSimulator))]
public class PlayerScript : MonoBehaviour
{
	[Serializable]
	public class DeathEvent : UnityEvent
	{
	}
	public int mode;
	[HideInInspector]
	public int dmgMode;
	public bool controllable;
	public float horizontalSpeed = 1f;
	[SerializeField] float walkSpeedGain = 0.07f; //Old: 0.07f
	[SerializeField] float runSpeedGain = 0.025f; //Old: 0.025f
	[SerializeField] float airSameDirAcceleration = 0.2f;  //Old: 0.04f; 
	[SerializeField] float slideSpeed = 0.06f; //Old: 0.06f
	[SerializeField] float passiveGain = 0.1f; //Old: 0.1f

	[HideInInspector]
	public AxisSimulator axis;
	public playerSprite pSprites;
	[HideInInspector]
	public Rigidbody2D rb;
	public Animator anim;
	public bool reachedGoal;
	public bool grounded;
	private bool touchingGround;
	public bool crouching;
	public float animatorSpeed = 1f;
	public Material poisonFlash;
	[HideInInspector]
	public Material mat;
	private bool slide;
	private bool releasedJump;
	private float jumpForce = 1f;
	public float jumpForceReduct = 1f;
	public float releaseJumpVelocity = 1f;
	public LayerMask whatIsGround;
	public LayerMask whatIsSolidGround;
	public LayerMask whatIsHittable;
	public LayerMask whatIsAllGround;
	public float startJumpPoint;
	public float currentJumpPoint;
	public float platHeight;
	public float maxHeight = 4f;
	private float maxHeightAdd;
	public Vector3 jumpHeights = new Vector3(2500f, 2500f, 3100f);
	public Vector3 jumpHeightsAlt = new Vector3(2500f, 2600f, 2800f);
	public bool running;
	private bool canRun = true;
	public bool dead;
	public bool midSpin;
	public SpriteRenderer render;
	private PolygonCollider2D pCol;
	private Vector2 colliderHeight = new Vector2(0.7f, 1.4f);
	private Vector2[] pathPoints;
	private bool frameCollision;
	private bool canDive;
	public Coroutine diveCor;
	private Coroutine shootCor;
	private Coroutine axeCor;
	public Coroutine knockbackCor;
	private Vector3 StartPoint;
	private bool midJump;
	public bool holdingObject;
	public Transform heldObject;
	private GameData data;
	private bool canAttack;
	private bool crouchBlock;
	private bool blinkingDuringInv = true;
	private Transform axeAura;
	public bool interruptDive;
	public bool canInterruptDive;
	private bool canHit;
	private bool capeDive;
	public bool midDive;
	private bool diveAfterJump = true;
	private float savedGravity;
	private float savedMax;
	public bool inCutscene;
	private Transform HalvaOverlay;
	public GameObject postDeathCanvas;
	public AudioClip[] stomp = new AudioClip[7];
	public int invFrames;
	private int stompFrames;
	public int powerFrames;
	private int powerUpWaitFrames;
	private int goalAnimFrames;
	public int diveCooldown;
	private WarpHitboxResize warpHitboxResize;
	public GameObject coin;
	[Space]
	private PolygonCollider2D ssCol;
	private CompositeCollider2D semiSolid;
	private Coroutine stickToGround;
	private bool stickingToGround;
	private bool ignoreSemiSolid;
	private bool semiSolidCooldown;
	private bool caped;
	public bool knockedBack;
	private GameObject cape;
	private checkForSemiSolid semiSolidChecker;
	private float capeGravity;
	public Gravity grav;
	public MenuScript pauseMenu;
	private ParticleSystem dust;
	private AudioSource dustSound;
	private PlayerTimePauser playerAnimScript;
	private bool semiSolidPrepare = true;
	[HideInInspector]
	public MGCameraController cam;
	private Vector2Int powerAnimStates;
	public bool eternal;
	public bool inverted;
	public bool bloodMode;
	private float scaleFix = 1f;
	public float bloodMaxHeight = 0.1f;
	public float bloodjumpForce = 10f;
	private bool sticking;
	public GameObject pointSound;
	private AudioSource sfxSound;
	private AudioSource aSource;
	public GameObject ImpactSprite;
	private bloodEffectScript[] ImpactSprites;
	private bloodEffectScript[] slideSprites;
	private SortingGroup sort;
	private Coroutine stick;
	private int jumpInputStoreFrames;
	public bool walkAfterGoal = true;
	public bool debugAutoJump;
	[HideInInspector]
	public int gravityFlipCooldown;
	private bool inPoison;
	private float lastPressedDir;
	private bool pressedOppositeDirInAir;
	private bool skipDeathAnim;
	private bool midDamage;
	private bool diveNudge;
	public string lavaSortLayerName;
	public int lavaSortLayerOrder;
	[HideInInspector]
	public bool reTrampoline;
	private float lastXPos;
	private float posDifference;
	private Coroutine poisonCor;
	private GameObject HUD;
	private bool sfxPaused;
	bool midDiscEat = false;
	private SpriteRenderer capeRender;
	[SerializeField]
	private DeathEvent deathEvent = new DeathEvent();

	public DeathEvent onDeathEvent
	{
		get
		{
			return deathEvent;
		}
		set
		{
			deathEvent = value;
		}
	}

	public void correctScaleParented(float x, bool exit)
	{
		scaleFix = x;
		if (scaleFix == 1f)
		{
			scaleFix = 1f;
			render.flipX = true;
		}
		else
		{
			Transform transform = base.transform;
			Vector3 localScale = base.transform.localScale;
			transform.localScale = new Vector3(0f - localScale.x, 1f, 1f);
			if(heldObject!=null)
			{
				Vector3 pos = heldObject.transform.localPosition;
				heldObject.transform.localPosition = new Vector3(-pos.x,pos.y,pos.z);
				pos = heldObject.transform.localScale;
			}
			if (!exit)
			{
				render.flipX = false;
			}
			else
			{
				render.flipX = true;
			}
		}
		if (inverted)
		{
			render.flipX = !render.flipX;
		}
		capeRender.flipX = render.flipX;
		HalvaOverlay.GetComponent<SpriteRenderer>().flipX = render.flipX;
	}

	public void convertPosSpeedToVelocity()
	{
		float num = Mathf.Clamp(0f - Mathf.Round(posDifference / 0.08f * 100f) / 100f, -1.2f, 1.3f);
		axis.axisPosX = Mathf.Clamp(axis.axisPosX + num, axis.axisRange.x, axis.axisRange.y);
	}

	private IEnumerator diveTime(float lengthOfDive, bool inDir)
	{
		diveCooldown = 30;
		capeDive = false;
		midDive = true;
		diveNudge = false;
		float diveLength = 0f;
		diveLength = (inDir ? 0.4f : lengthOfDive);
		if (pSprites.state == 3)
		{
			midSpin = true;
			stompFrames = 50;
			axeAura.gameObject.SetActive(value: true);
		}
		pathPoints[1] = new Vector2(0f, colliderHeight.x);
		pCol.SetPath(0, pathPoints);
		canRun = false;
		running = false;
		axis.acceptXInputs = false;
		savedGravity = grav.pushForces.y;
		savedMax = grav.maxVelocities.x;
		grav.maxVelocities = new Vector2(20f, grav.maxVelocities.y);
		grav.pushForces = new Vector2(grav.pushForces.x, 0f);
		anim.SetBool("gravity", value: false);
		interruptDive = false;
		while (diveLength > 0f)
		{
			Vector2 velocity = rb.velocity;
			if (velocity.x == 0f)
			{
				break;
			}
			diveLength -= Time.deltaTime;
			yield return 0;
		}
		diveLength = 0f;
		releasedJump = true;
		if (!inverted)
		{
			grav.pushForces = new Vector2(grav.pushForces.x, 0f - Mathf.Abs(savedGravity));
		}
		else
		{
			grav.pushForces = new Vector2(grav.pushForces.x, Mathf.Abs(savedGravity));
		}
		anim.SetBool("gravity", value: true);
		if (pSprites.state == 3)
		{
			midSpin = false;
			stompFrames = 5;
		}
		axeAura.gameObject.SetActive(value: false);
		while (!holdingObject && !grounded && anim.GetBool("dive"))
		{
			yield return 0;
			Vector2 velocity2 = rb.velocity;
			if (Mathf.Abs(velocity2.x) < 0.2f && rb.bodyType != RigidbodyType2D.Static)
			{
				Rigidbody2D rigidbody2D = rb;
				Vector3 localScale = base.transform.localScale;
				float x = 0.2f * localScale.x;
				Vector2 velocity3 = rb.velocity;
				rigidbody2D.velocity = new Vector2(x, velocity3.y);
			}
		}
		if (!bloodMode)
		{
			axis.axisAdder = passiveGain;
		}
		else
		{
			axis.axisAdder = 0.3f;
		}
		yield return 0;
		midDive = false;
		canInterruptDive = true;
		if (holdingObject)
		{
			interruptDive = true;
		}
		axis.acceptXInputs = false;
		while(rb.velocity.x!=0)
		{
			axis.acceptXInputs = false;
			if(!anim.GetBool("dive")||interruptDive)
			break;
			
			yield return 0;
		}
		/*yield return new WaitUntil(delegate
		{
			Vector2 velocity4 = rb.velocity;
			return velocity4.x == 0f || interruptDive || !anim.GetBool("dive");
		});*/
		interruptDive = false;
		canInterruptDive = false;
		grav.maxVelocities = new Vector2(savedMax, grav.maxVelocities.y);
		if(pSprites.state!=0)
		forceCrouch(true);
		slide = false;
		anim.SetBool("slide", slide);
		if (!reachedGoal)
		{
			if (!SuperInput.GetKey("Run")||crouching)
			{
				axis.setRange(1.2f);
			}
			else
			{
				axis.setRange(2.2f);
			}
		}
		else
		{
			axis.setRange(1f);
		}
		if (grounded)
		{
			axis.axisAdder = passiveGain;
		}
		else
		{
			axis.axisAdder = airSameDirAcceleration;
		}
		canRun = true;
		if (controllable)
		{
			axis.acceptXInputs = true;
		}
		if (!warpHitboxResize.midWarp)
		{
			data.sMeterWorks = true;
		}
		if (pSprites.state != 0 && !crouching)
		{
			pathPoints[1] = new Vector2(0f, colliderHeight.y);
			pCol.SetPath(0, pathPoints);
		}
		yield return 0;
		anim.SetBool("dive", value: false);
		grav.maxVelocities = new Vector2(savedMax, grav.maxVelocities.y);
		diveCor = null;
		if (holdingObject && !grounded)
		{
			holdingObject = false;
			transform.GetChild(0).localEulerAngles = Vector3.zero;
			if (SuperInput.GetKey("Jump"))
			{
				releasedJump = false;
				Vector2 vector = base.transform.up * jumpForce / jumpForceReduct;
				rb.AddForce(new Vector2(0f, vector.y), ForceMode2D.Force);
			}
			else
			{
				releasedJump = true;
				Vector2 vector2 = base.transform.up * jumpForce / 2f;
				rb.AddForce(new Vector2(0f, vector2.y), ForceMode2D.Force);
			}
		}
	}

	private IEnumerator jumpDur()
	{
		yield return new WaitForSeconds(0.05f);
		midJump = false;
		diveAfterJump = true;
	}

	private IEnumerator kickDur(float timer)
	{
		anim.SetBool("kick", value: true);
		yield return new WaitForSeconds(timer);
		anim.SetBool("kick", value: false);
	}

	private IEnumerator stickCor()
	{
		bool stickJump = false;
		if (SuperInput.GetKey("Jump"))
		{
			stickJump = true;
		}
		yield return new WaitForSeconds(0.1f);
		if (sticking)
		{
			Vector3 localScale = base.transform.localScale;
			if ((localScale.x != axis.horAxis && axis.horAxis != 0f && !dead) || SuperInput.GetKey("Jump") || stickJump)
			{
				unStick(jumpOff: true);
				stick = null;
				yield break;
			}
		}
		stick = null;
		unStick(jumpOff: false);
	}

	public IEnumerator idleLook()
	{
		anim.SetBool("idleEvent", value: true);
		yield return new WaitForSeconds(0.2f);
		anim.SetBool("idleEvent", value: false);
	}

	private IEnumerator ssCooldown()
	{
		semiSolidCooldown = true;
		yield return 0;
		yield return 0;
		semiSolidCooldown = false;
	}

	private IEnumerator prepareSemiSolidCollision()
	{
		yield return new WaitForSeconds(0.1f);
		semiSolidPrepare = false;
	}

	private IEnumerator shoot()
	{
		int stateSav = pSprites.state;
		yield return 0;
		if (!holdingObject)
		{
			int projType2 = 0;
			switch (stateSav)
			{
			default: yield break;
			case 2:
				projType2 = 0;
				break;
			case 5:
				projType2 = 1;
				break;
			case 6:
				projType2 = 2;
				break;
			}
			anim.SetBool("shoot", data.fire(projType2, base.transform.position, base.transform.localScale, inverted));
			yield return new WaitForSeconds(0.15f);
			anim.SetBool("shoot", value: false);
		}
	}

	private IEnumerator axeAttack()
	{
		yield return 0;
		if (!holdingObject)
		{
			midSpin = true;
			stompFrames = 50;
			axeAura.gameObject.SetActive(value: true);
			anim.speed = animatorSpeed;
			anim.SetBool("spinAttack", value: true);
			data.playSoundStatic(21);
			yield return new WaitForSeconds(0.28f);
			stompFrames = 5;
			axeAura.gameObject.SetActive(value: false);
			midSpin = false;
			anim.SetBool("spinAttack", value: false);
		}
	}

	private void spawnImpactKick()
	{
		float num = (!inverted) ? 1 : (-1);
		GameData gameData = data;
		Vector3 position = base.transform.position;
		Vector3 localScale = base.transform.localScale;
		gameData.spawnImpact(position + new Vector3(0.5f * localScale.x, 0.25f * num, 0f));
	}
	private IEnumerator ignoreCol(Transform objec)
	{
		string oldName = objec.name;
		GameObject o = objec.gameObject;
		Collider2D c = objec.GetComponent<Collider2D>();
		if(objec.name.EndsWith("NoCol"))
		{
			yield break;
			//oldName = oldName.Substring(0,oldName.Length-5);
			//objec.name = oldName;
		}
		objec.name+="NoCol";
		Physics2D.IgnoreCollision(pCol,c,true);
		while(o!=null)
		{
			float d = Vector3.Distance(transform.position,objec.position);
			if(d>=1f||d>0.5f&&!grounded&&((!inverted&&transform.position.y-0.5f>objec.position.y)||(inverted&&transform.position.y+0.5f<objec.position.y)))
			{
				//print(d);
				break;
			}
			//yield return new WaitForSeconds(0.1f);
			yield return 0;
		}
		//yield return new WaitUntil(()=>o==null||Vector3.Distance(transform.position,objec.position)>=1.5f);
		//print("Not ignoring "+oldName);
		if(o==null)yield break;
		objec.name=oldName;
		Physics2D.IgnoreCollision(pCol,c,false);
	}
	private IEnumerator throwBlock(Transform objec, holdableBlockScript block)
	{
		StartCoroutine(ignoreCol(objec));
		Transform objec2 = objec;
		if (block.explosive)
		{
			block.canExplode = true;
		}
		else
		{
			block.canShatter = true;
		}
		heldObject = objec2;
		holdingObject = true;
		flipHeldObject(inverted);
		yield return new WaitUntil(() => data.reachedGoal || (Time.timeScale!=0 && !SuperInput.GetKey("Run")) || objec2 == null || !objec2.gameObject.activeInHierarchy || objec2.parent != base.transform || dead || knockedBack || anim.GetBool("dive"));
		heldObject = null;
		float extraVelo = 0f;
		if (!SuperInput.GetKey("Run") && axis.verAxis != -1f)
		{
			extraVelo = 16f;
		}
		if(data.reachedGoal)
		{
			block.killBlock();
		}
		if (objec2 != null)
		{
			if (axis.verAxis == 0f || anim.GetBool("dive"))
			{
				spawnImpactKick();
				data.playSoundStatic(20);
				StartCoroutine(kickDur(0.25f));
			}
			else if (axis.verAxis == -1f)
			{
				block.togglePl(true);
				if (block.explosive)
				{
					block.canExplode = false;
					block.saveDropFrames = 5;
				}
				else
				{
					block.canShatter = false;
				}
			}
			if (!inverted)
			{
				float xScale = transform.localScale.x;
				Vector2 pPos = new Vector2(transform.position.x,transform.position.y),spawnOffset = new Vector2(-0.1f*xScale,0.5f);
				RaycastHit2D r = Physics2D.BoxCast(pPos+spawnOffset,Vector2.one/2,0,transform.right*xScale,0.9f,whatIsSolidGround);
				Vector3 ObPos = objec2.position;
				Debug.DrawLine(pPos+spawnOffset,r.centroid,Color.red,2f);
				if(r.collider!=null)
				objec2.position = new Vector3(r.centroid.x, ObPos.y, ObPos.z);
				else objec2.position = new Vector3(transform.position.x+(0.9f*xScale), ObPos.y, ObPos.z);
				if (!grounded)
				{
					Vector3 zero = Vector3.zero;
					Vector3 localScale = base.transform.localScale;
					block.blockParent(parent: false, zero, new Vector2(localScale.x * (6f + extraVelo), 15f), playerPickUp: true);
				}
				else if (axis.verAxis != -1f)
				{
					Vector3 zero2 = Vector3.zero;
					Vector3 localScale2 = base.transform.localScale;
					block.blockParent(parent: false, zero2, new Vector2(localScale2.x * (6f + extraVelo), 10f), playerPickUp: true);
				}
				else
				{
					Vector3 zero3 = Vector3.zero;
					Vector3 localScale3 = base.transform.localScale;
					block.blockParent(parent: false, zero3, new Vector2(localScale3.x * (2f + extraVelo), 0f), playerPickUp: true);
				}
			}
			else
			{
				float xScale = transform.localScale.x;
				Vector2 pPos = new Vector2(transform.position.x,transform.position.y),spawnOffset = new Vector2(-0.1f*xScale,-0.5f);
				RaycastHit2D r = Physics2D.BoxCast(pPos+spawnOffset,Vector2.one/2,0,-transform.right*xScale,0.9f,whatIsSolidGround);
				Vector3 ObPos = objec2.position;
				Debug.DrawLine(pPos+spawnOffset,r.centroid,Color.red,2f);
				if(r.collider!=null)
				objec2.position = new Vector3(r.centroid.x, ObPos.y, ObPos.z);
				else objec2.position = new Vector3(transform.position.x+(0.9f*xScale), ObPos.y, ObPos.z);
				/*Vector3 localPosition3 = objec2.localPosition;
				float y2 = localPosition3.y;
				Vector3 localPosition4 = objec2.localPosition;
				objec2.localPosition = new Vector3(-0.9f, y2, localPosition4.z);*/
				if (!grounded)
				{
					Vector3 zero4 = Vector3.zero;
					Vector3 localScale4 = base.transform.localScale;
					block.blockParent(parent: false, zero4, new Vector2(localScale4.x * (6f + extraVelo), -15f), playerPickUp: true);
				}
				else if (axis.verAxis != -1f)
				{
					Vector3 zero5 = Vector3.zero;
					Vector3 localScale5 = base.transform.localScale;
					block.blockParent(parent: false, zero5, new Vector2(localScale5.x * (6f + extraVelo), -10f), playerPickUp: true);
				}
				else
				{
					Vector3 zero6 = Vector3.zero;
					Vector3 localScale6 = base.transform.localScale;
					block.blockParent(parent: false, zero6, new Vector2(localScale6.x * (2f + extraVelo), 0f), playerPickUp: true);
				}
			}
		}
		holdingObject = false;
	}

	private IEnumerator throwHeldDisc(Transform objec)
	{
		Transform objec2 = objec;
		heldObject = objec2;
		holdingObject = true;
		flipHeldObject(inverted);
		yield return new WaitUntil(delegate
		{
			int result;
			if (!data.reachedGoal && (Time.timeScale==0||SuperInput.GetKey("Run")) && !(objec2 == null) 
			&& objec2.gameObject.activeInHierarchy && !(objec2.parent != base.transform) && !dead && !knockedBack && holdingObject)
			{
				if (axis.verAxis == 1f && anim.GetCurrentAnimatorStateInfo(0).IsName("Player_idle") && !warpHitboxResize.inWarpCollider)
				{
					Vector2 velocity2 = rb.velocity;
					if (Mathf.Abs(velocity2.x) <= 0.1f)
					{
						result = (base.enabled ? 1 : 0);
						goto IL_013c;
					}
				}
				result = 0;
			}
			else
			{
				result = 1;
			}
			goto IL_013c;
			IL_013c:
			return (byte)result != 0;
		});
		if(data.reachedGoal)
		{
			objec.GetComponent<EnemyCorpseSpawner>().spawnCorpse();
		}
		if (objec2 != null && objec2.parent == base.transform)
		{
			stompFrames = 5;
		}
		heldObject = null;
		if (objec2 != null)
		{
			shellScript shell = objec2.GetComponent<shellScript>();
			shell.hurtsPlayer = false;
			if (!grounded || (grounded && Mathf.Abs(axis.verAxis) != 1f))
			{
				shell.stompAllowWait = 30;
				spawnImpactKick();
				data.playSoundStatic(20);
				Vector3 position = base.transform.position;
				shell.discLaunch(position.x);
				StartCoroutine(kickDur(0.25f));
			}
			Vector2 velocity = rb.velocity;
			if (!warpHitboxResize.inWarpCollider && base.enabled)
			{
				if (axis.verAxis == 1f)
				{
					midDiscEat = true;
					data.playSoundStatic(35);
					if (HalvaOverlay.gameObject.activeInHierarchy)
					{
						shell.transform.GetComponent<EnemyCorpseSpawner>().invincibilityKills = false;
					}
					int value = shell.discValue;
					Transform t = objec2.transform.GetChild(0);
					if (shell.waitUntilReturn != null)
					{
						StopCoroutine(shell.waitUntilReturn);
					}
					UnityEngine.Object.Destroy(shell);
					if (value == 0)
					{
						t.gameObject.name = "Cola";
					}
					if (value == 1)
					{
						t.gameObject.name = "Pepper";
					}
					if (value == 2)
					{
						t.gameObject.name = "Axe";
					}
					anim.SetInteger("discEatState", value + 1);
					t.GetComponent<SimpleAnim2>().enabled = false;
					t.GetComponent<SpriteRenderer>().sprite = null;
					yield return 0;
					anim.SetInteger("discEatState", -1);
					yield return new WaitForSeconds(0.2f);
					if (!dead)
					{
						if(objec2!=null)
						{
							objec2.SetParent(null);
							Rigidbody2D rbs = objec2.GetComponent<Rigidbody2D>();
							rbs.bodyType = RigidbodyType2D.Dynamic;
							rbs.simulated = true;
						}
						if (warpHitboxResize.midWarp)
						{
							yield return 0;
							yield return new WaitUntil(() => !warpHitboxResize.midWarp);
							if (t != null)
							{
								t.position = base.transform.position;
							}
						}
					}
					else
					{
						UnityEngine.Object.Destroy(objec2.gameObject);
					}
					midDiscEat = false;
				}
				else
				{
					shell.kickable = false;
					shell.discParent(isParented: false, Vector3.zero);
				}
			}
		}
		holdingObject = false;
	}

	private IEnumerator poisonSequence()
	{
		int ID = data.playSoundStaticGetID(90);
		if (inPoison)
		{
			yield return new WaitForSeconds(0.1f);
			checkPoisonFlash(ID);
		}
		if (inPoison)
		{
			yield return new WaitForSeconds(0.1f);
		}
		render.material = mat;
		if (inPoison)
		{
			yield return new WaitForSeconds(0.3f);
			checkPoisonFlash(ID);
		}
		if (inPoison)
		{
			yield return new WaitForSeconds(0.1f);
		}
		render.material = mat;
		if (inPoison)
		{
			yield return new WaitForSeconds(0.3f);
			checkPoisonFlash(ID);
		}
		if (inPoison)
		{
			yield return new WaitForSeconds(0.1f);
		}
		render.material = mat;
		if (inPoison)
		{
			yield return new WaitForSeconds(0.3f);
		}
		if (inPoison && invFrames == 0 && !reachedGoal)
		{
			cam.easeShake = true;
			cam.shakeCamera(0.5f, 0.5f);
			data.playSoundStatic(91);
			Damage(ignoreStomp: true, ignoreFrames: false);
		}
		render.material = mat;
		poisonCor = null;
	}

	private void checkPoisonFlash(int ID)
	{
		if (inPoison)
		{
			render.material = poisonFlash;
			return;
		}
		render.material = mat;
		data.stopSoundStatic(ID);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		string tag = other.tag;
		string name = other.name;
		if (!grounded && name == "InvBlockMap")
		{
			if (!inverted)
			{
				Vector2 velocity = rb.velocity;
				if (velocity.y > 1.6f)
				{
					canHit = false;
					GameData gameData = data;
					Vector3 position = base.transform.position;
					float x = position.x;
					Vector3 position2 = base.transform.position;
					gameData.revealBlock(new Vector2(x, position2.y + 0.5f), base.transform.position, base.gameObject.name, inverted);
				}
			}
			else
			{
				Vector2 velocity2 = rb.velocity;
				if (velocity2.y < -1.6f)
				{
					canHit = false;
					GameData gameData2 = data;
					Vector3 position3 = base.transform.position;
					float x2 = position3.x;
					Vector3 position4 = base.transform.position;
					gameData2.revealBlock(new Vector2(x2, position4.y - 0.5f), base.transform.position, base.gameObject.name, inverted);
				}
			}
		}
		if (!dead && name == "GoalBounds" && controllable && reachedGoal&&(!grounded||inverted))
		{
			controllable = false;
			pauseMenu.enabled = false;
			axis.axisPosX /= 2f;
			StartCoroutine(data.goalAnimate(0, 0.5f));
			other.transform.parent.GetChild(0).GetComponent<GoalBall>().kicked = true;
			StartCoroutine(goalAnimation(kick: false));
		}
		switch (tag)
		{
		case "GroundHarm":
			if (!grounded)
			{
				break;
			}
			goto case "Harm";
		case "Harm":
		case "HarmTrigger":
		case "FlameHarm":
			if (Time.timeScale != 0f && invFrames == 0 && !dead && !inCutscene && !reachedGoal)
			{
				Damage(ignoreStomp: true, ignoreFrames: false);
			}
			break;
		}
		if (tag == "knockHarm" && Time.timeScale != 0f && !dead && !inCutscene && !knockedBack)
		{
			if (!eternal)
			{
				if (knockbackCor == null)
				{
					Vector3 localScale = base.transform.localScale;
					knockbackCor = StartCoroutine(knockBack(0f - localScale.x, 0.01f, 0f,false));
				}
				if (invFrames == 0)
				{
					Damage(ignoreStomp: true, ignoreFrames: false);
				}
			}
			else if (invFrames == 0)
			{
				Damage(ignoreStomp: true, ignoreFrames: false);
			}
		}
		if (tag == "blockHoldable")
		{
			if (!inverted)
			{
				if (!holdingObject && axis.Run)
				{
					Transform t = other.transform;
					Vector3 position5 = t.position;
					float y = position5.y;
					Vector3 position6 = base.transform.position;
					if (y >= position6.y + 0.05f && diveCor == null)
					{
						Vector3 parentedPos = (pSprites.state == 0) ? new Vector3(0.5f*(render.flipX?1:-1), 0.6f, 0f) : new Vector3(0.4f*(render.flipX?1:-1), 0.8f, 0f);
						t.parent.transform.SetParent(base.transform);
						t.parent.GetComponent<holdableBlockScript>().blockParent(parent: true, parentedPos, Vector2.zero, playerPickUp: true);
						if (!holdingObject && !dead)
						{
							StartCoroutine(throwBlock(t.parent, t.parent.GetComponent<holdableBlockScript>()));
						}
					}
				}
			}
			else if (!holdingObject && axis.Run)
			{
				Transform t = other.transform;
				Vector3 position7 = t.position;
				float y2 = position7.y;
				Vector3 position8 = base.transform.position;
				if (y2 <= position8.y - 0.05f && diveCor == null)
				{
					Vector3 parentedPos2 = (pSprites.state == 0) ? new Vector3(-0.5f*(render.flipX?1:-1), 0.6f, 0f) : new Vector3(-0.4f*(render.flipX?1:-1), 0.8f, 0f);
					t.parent.transform.SetParent(base.transform);
					t.parent.GetComponent<holdableBlockScript>().blockParent(parent: true, parentedPos2, Vector2.zero, playerPickUp: true);
					if (!holdingObject && !dead)
					{
						StartCoroutine(throwBlock(t.parent, t.parent.GetComponent<holdableBlockScript>()));
					}
				}
			}
		}
		if (!(tag == "Disc") || dead)
		{
			return;
		}
		if (!inverted)
		{
			shellScript component = other.transform.parent.GetComponent<shellScript>();
			if (component!=null && !component.moving && !component.inLava && axis.Run && !holdingObject && component.grabbable && !midDive && diveCor == null)
			{
				Vector3 parentedPos3 = (pSprites.state == 0) ? new Vector3(0.318f, 0.065f, 0f) : new Vector3(0.318f, 0.26f, 0f);
				other.transform.parent.transform.SetParent(base.transform);
				component.discParent(isParented: true, parentedPos3);
				if (!holdingObject && !dead)
				{
					StartCoroutine(throwHeldDisc(other.transform.parent));
				}
				return;
			}
			if (component!=null && component.stompAllowWait <= 0)
			{
				Vector2 velocity3 = rb.velocity;
				if (velocity3.y < -0.1f)
				{
					Vector3 position9 = base.transform.position;
					float y3 = position9.y;
					Vector3 position10 = other.transform.position;
					if (y3 > position10.y && component.moving)
					{
						goto IL_07bc;
					}
				}
				Vector2 velocity4 = rb.velocity;
				if (velocity4.y < -0.1f)
				{
					Vector3 position11 = base.transform.position;
					float y4 = position11.y;
					Vector3 position12 = other.transform.position;
					if (y4 > position12.y && heldObject != null)
					{
						goto IL_07bc;
					}
				}
				Vector2 velocity5 = rb.velocity;
				if (velocity5.y < -0.1f)
				{
					Vector3 position13 = base.transform.position;
					float y5 = position13.y;
					Vector3 position14 = other.transform.position;
					if (y5 > position14.y && !other.transform.parent.GetComponent<shellScript>().moving && !axis.Run)
					{
						goto IL_07bc;
					}
				}
				if (diveCor != null)
				{
					goto IL_07bc;
				}
			}
			Vector2 velocity6 = rb.velocity;
			if (velocity6.y > -0.1f && !other.transform.parent.GetComponent<shellScript>().moving && other.transform.parent.GetComponent<shellScript>().kickable)
			{
				Vector3 position15 = base.transform.position;
				float y6 = position15.y;
				Vector3 position16 = other.transform.position;
				if (y6 <= position16.y && !axis.Run)
				{
					goto IL_0a8d;
				}
			}
			if (!other.transform.parent.GetComponent<shellScript>().moving && other.transform.parent.GetComponent<shellScript>().kickable)
			{
				Vector3 position17 = base.transform.position;
				float y7 = position17.y;
				Vector3 position18 = other.transform.position;
				if (y7 <= position18.y && holdingObject && other.transform.parent != heldObject)
				{
					goto IL_0a8d;
				}
			}
			if (!other.transform.parent.GetComponent<shellScript>().moving)
			{
				return;
			}
			Vector3 position19 = base.transform.position;
			float y8 = position19.y;
			Vector3 position20 = other.transform.position;
			if (y8 <= position20.y)
			{
				Vector2 velocity7 = rb.velocity;
				if (velocity7.y > -0.1f && other.transform.parent.GetComponent<shellScript>().hurtsPlayer && Time.timeScale != 0f && invFrames == 0 && other.transform.parent.GetComponent<shellScript>().moving)
				{
					Damage(ignoreStomp: false, ignoreFrames: false);
				}
			}
			return;
		}
		shellScript component2 = other.transform.parent.GetComponent<shellScript>();
		if (!component2.moving && !component2.inLava && axis.Run && !holdingObject && component2.grabbable && !midDive)
		{
			Vector3 parentedPos4 = (pSprites.state == 0) ? new Vector3(-0.318f, 0.065f, 0f) : new Vector3(-0.318f, 0.26f, 0f);
			other.transform.parent.transform.SetParent(base.transform);
			component2.discParent(isParented: true, parentedPos4);
			if (!holdingObject)
			{
				StartCoroutine(throwHeldDisc(other.transform.parent));
			}
			return;
		}
		if (component2.stompAllowWait <= 0)
		{
			Vector2 velocity8 = rb.velocity;
			if (velocity8.y > 0.1f)
			{
				Vector3 position21 = base.transform.position;
				float y9 = position21.y;
				Vector3 position22 = other.transform.position;
				if (y9 < position22.y && other.transform.parent.GetComponent<shellScript>().moving)
				{
					goto IL_0dea;
				}
			}
			Vector2 velocity9 = rb.velocity;
			if (velocity9.y > 0.1f)
			{
				Vector3 position23 = base.transform.position;
				float y10 = position23.y;
				Vector3 position24 = other.transform.position;
				if (y10 < position24.y && heldObject != null)
				{
					goto IL_0dea;
				}
			}
			Vector2 velocity10 = rb.velocity;
			if (velocity10.y > 0.1f)
			{
				Vector3 position25 = base.transform.position;
				float y11 = position25.y;
				Vector3 position26 = other.transform.position;
				if (y11 < position26.y && !other.transform.parent.GetComponent<shellScript>().moving && !axis.Run)
				{
					goto IL_0dea;
				}
			}
		}
		Vector2 velocity11 = rb.velocity;
		if (velocity11.y < 0.1f && !other.transform.parent.GetComponent<shellScript>().moving && other.transform.parent.GetComponent<shellScript>().kickable)
		{
			Vector3 position27 = base.transform.position;
			float y12 = position27.y;
			Vector3 position28 = other.transform.position;
			if (y12 >= position28.y && !axis.Run)
			{
				goto IL_107c;
			}
		}
		if (!other.transform.parent.GetComponent<shellScript>().moving && other.transform.parent.GetComponent<shellScript>().kickable)
		{
			Vector3 position29 = base.transform.position;
			float y13 = position29.y;
			Vector3 position30 = other.transform.position;
			if (y13 >= position30.y && holdingObject && other.transform.parent != heldObject)
			{
				goto IL_107c;
			}
		}
		if (!other.transform.parent.GetComponent<shellScript>().moving)
		{
			return;
		}
		Vector3 position31 = base.transform.position;
		float y14 = position31.y;
		Vector3 position32 = other.transform.position;
		if (y14 >= position32.y)
		{
			Vector2 velocity12 = rb.velocity;
			if (velocity12.y < 0.1f && other.transform.parent.GetComponent<shellScript>().hurtsPlayer && Time.timeScale != 0f && invFrames == 0 && other.transform.parent.GetComponent<shellScript>().moving)
			{
				Damage(ignoreStomp: false, ignoreFrames: false);
			}
		}
		return;
		IL_0a8d:
		stompFrames = 5;
		shellScript component3 = other.transform.parent.GetComponent<shellScript>();
		Vector3 position33 = base.transform.position;
		component3.discLaunch(position33.x);
		other.transform.parent.GetComponent<shellScript>().hurtsPlayer = false;
		spawnImpactKick();
		data.playSoundStatic(20);
		StartCoroutine(kickDur(0.25f));
		return;
		IL_0dea:
		data.playSoundStatic(20);
		shellScript component4 = other.transform.parent.GetComponent<shellScript>();
		Vector3 position34 = base.transform.position;
		component4.discLaunch(position34.x);
		Vector3 position35 = other.transform.position;
		startJumpPoint = position35.y;
		stompFrames = 10;
		Rigidbody2D rigidbody2D = rb;
		Vector2 velocity13 = rb.velocity;
		rigidbody2D.velocity = new Vector2(velocity13.x, 0f);
		axis.currentDivider = axis.savedNormalDivider;
		grounded = false;
		slide = false;
		playStompSound(data.stompStreak);
		data.spawnImpact(base.transform.position);
		if (SuperInput.GetKey("Jump"))
		{
			releasedJump = false;
			Vector2 vector = base.transform.up * jumpForce / jumpForceReduct;
			rb.AddForce(new Vector2(0f, vector.y), ForceMode2D.Force);
		}
		else
		{
			releasedJump = true;
			Vector2 vector2 = base.transform.up * jumpForce / 2f;
			rb.AddForce(new Vector2(0f, vector2.y), ForceMode2D.Force);
		}
		return;
		IL_07bc:
		data.playSoundStatic(20);
		shellScript component5 = other.transform.parent.GetComponent<shellScript>();
		Vector3 position36 = base.transform.position;
		component5.discLaunch(position36.x);
		Vector3 position37 = other.transform.position;
		startJumpPoint = position37.y;
		stompFrames = 10;
		Rigidbody2D rigidbody2D2 = rb;
		Vector2 velocity14 = rb.velocity;
		rigidbody2D2.velocity = new Vector2(velocity14.x, 0f);
		axis.currentDivider = axis.savedNormalDivider;
		grounded = false;
		slide = false;
		playStompSound(data.stompStreak);
		data.spawnImpact(base.transform.position);
		if (anim.GetBool("dive"))
		{
			anim.SetBool("dive", value: false);
			if (canInterruptDive)
			{
				interruptDive = true;
			}
			diveCor = null;
		}
		if (SuperInput.GetKey("Jump"))
		{
			releasedJump = false;
			Vector2 vector3 = base.transform.up * jumpForce / jumpForceReduct;
			rb.AddForce(new Vector2(0f, vector3.y), ForceMode2D.Force);
		}
		else
		{
			releasedJump = true;
			Vector2 vector4 = base.transform.up * jumpForce / 2f;
			rb.AddForce(new Vector2(0f, vector4.y), ForceMode2D.Force);
		}
		return;
		IL_107c:
		stompFrames = 5;
		shellScript component6 = other.transform.parent.GetComponent<shellScript>();
		Vector3 position38 = base.transform.position;
		component6.discLaunch(position38.x);
		other.transform.parent.GetComponent<shellScript>().hurtsPlayer = false;
		spawnImpactKick();
		data.playSoundStatic(20);
		StartCoroutine(kickDur(0.25f));
	}

	public void disableDust()
	{
		if (dustSound.isPlaying)
		{
			dustSound.loop = false;
		}
		dust.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
	}

	private void Start()
	{
		axis = GetComponent<AxisSimulator>();
		rb = GetComponent<Rigidbody2D>();
		anim = base.transform.GetChild(0).GetComponent<Animator>();
		pSprites = GetComponent<playerSprite>();
		render = base.transform.GetChild(0).GetComponent<SpriteRenderer>();
		pCol = base.transform.GetChild(3).GetComponent<PolygonCollider2D>();
		ssCol = base.transform.GetChild(2).GetComponent<PolygonCollider2D>();
		playerAnimScript = base.transform.GetChild(0).GetComponent<PlayerTimePauser>();
		cam = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		cape = base.transform.GetChild(0).GetChild(2).gameObject;
		capeRender = cape.GetComponent<SpriteRenderer>();
		pathPoints = pCol.GetPath(0);
		Vector3 position = base.transform.position;
		platHeight = position.y;
		data = GameObject.Find("_GM").GetComponent<GameData>();
		mode = data.mode;
		HalvaOverlay = base.transform.GetChild(0).transform.GetChild(0);
		axeAura = base.transform.GetChild(0).transform.GetChild(1);
		axeAura.gameObject.SetActive(value: false);
		grav = GetComponent<Gravity>();
		savedGravity = grav.pushForces.y;
		savedMax = grav.maxVelocities.x;
		mat = render.material;
		dust = base.transform.GetChild(0).GetChild(3).GetComponent<ParticleSystem>();
		dustSound = dust.transform.GetComponent<AudioSource>();
		capeGravity = grav.maxVelocities.y / 4.25f;
		warpHitboxResize = base.transform.GetChild(3).transform.GetChild(0).GetComponent<WarpHitboxResize>();
		warpHitboxResize.setSize();
		semiSolid = GameObject.Find("SemiSolidMap").GetComponent<CompositeCollider2D>();
		semiSolidChecker = base.transform.GetChild(1).GetComponent<checkForSemiSolid>();
		if (pauseMenu == null)
		{
			pauseMenu = GameObject.Find("PauseCanvas").GetComponent<MenuScript>();
		}
		Physics2D.IgnoreLayerCollision(11, 27, ignore: false);
		ImpactSprites = new bloodEffectScript[2];
		slideSprites = new bloodEffectScript[2];
		GameObject gameObject = UnityEngine.Object.Instantiate(pointSound, base.transform.position, Quaternion.identity);
		sfxSound = transform.GetChild(0).GetComponent<AudioSource>();
		aSource = GetComponent<AudioSource>();
		gameObject.transform.SetParent(base.transform.GetChild(0));
		gameObject.transform.localPosition = Vector3.zero;
		sfxSound.loop = true;
		sort = GetComponent<SortingGroup>();
		HUD = GameObject.Find("HUD_Canvas");
		if (mode == 1 && postDeathCanvas != null)
		{
			jumpHeights = jumpHeightsAlt;
			maxHeightAdd = 1f;
			slideSpeed = 0.03f;
		}
		for (int i = 0; i < 2; i++)
		{
			gameObject = UnityEngine.Object.Instantiate(ImpactSprite, Vector3.zero, Quaternion.identity);
			slideSprites[i] = gameObject.GetComponent<bloodEffectScript>();
			slideSprites[i].setModeSlide();
		}
		for (int j = 0; j < 2; j++)
		{
			gameObject = UnityEngine.Object.Instantiate(ImpactSprite, Vector3.zero, Quaternion.identity);
			ImpactSprites[j] = gameObject.GetComponent<bloodEffectScript>();
		}
		if (GameObject.Find("DataShare") != null)
		{
			dataShare component = GameObject.Find("DataShare").GetComponent<dataShare>();
			dmgMode = component.difficulty;
			if (component.checkPointState == 0)
			{
				pSprites.state = component.playerState;
			}
			else if (component.checkPointState != 6)
			{
				pSprites.state = component.checkPointState;
			}
			else
			{
				pSprites.state = 3;
				data.healthBarEnable(2, withAnimation: false);
				eternal = true;
				pSprites.eternal = true;
			}

			if (pSprites.state == 4 || component.checkPointState == 6)
			{
				cape.SetActive(value: true);
				caped = true;
			}
			SetStateProperties(pSprites.state);
			if (pSprites.state > 0)
			{
				pathPoints[1] = new Vector2(0f, colliderHeight.y);
				pCol.SetPath(0, pathPoints);
			}
		}
		else
		{
			Debug.LogError("No data share found");
		}
		StartCoroutine(prepareSemiSolidCollision());
		Vector3 position2 = base.transform.position;
		lastXPos = position2.x;
	}

	private void LateUpdate()
	{
		if (Time.timeScale != 0f && !dead)
		{
			Vector3 position = base.transform.position;
			lastXPos = position.x;
		}
	}

	private void Update()
	{
		if (Time.timeScale != 0f && !dead)
		{
			if (!knockedBack)
			{
				horizontalMovement();
			}
			Animator animator = anim;
			Vector2 velocity = rb.velocity;
			animator.SetFloat("VerSpeed", Mathf.Abs(velocity.y));
			if (!crouching && controllable)
			{
				run();
				if (!grounded)
				{
					if (!SuperInput.GetKey("Dash") && !canDive && !anim.GetBool("dive") && ((!caped && axis.verAxis != -1f) || caped))
					{
						canDive = true;
					}
					if (!midDiscEat&&diveCooldown<=0 && canDive && !midDive && !midSpin && diveAfterJump && !knockedBack && gravityFlipCooldown <= 0 && stompFrames <= 1)
					{
						dive();
					}
					if (touchingGround)
					{
						if (!inverted)
						{
							Vector2 velocity2 = rb.velocity;
							if (velocity2.y < -0.9f && !anim.GetBool("dive"))
							{
								goto IL_01b4;
							}
						}
						if (inverted)
						{
							Vector2 velocity3 = rb.velocity;
							if (velocity3.y > 0.9f && !anim.GetBool("dive"))
							{
								goto IL_01b4;
							}
						}
						unStick(jumpOff: false);
					}
					else if (sticking && grav.maxVelocities.y != grav.savedMaxVelocities.y)
					{
						unStick(jumpOff: false);
					}
				}
				else
				{
					unStick(jumpOff: false);
				}
				goto IL_0451;
			}
			goto IL_046e;
		}
		goto IL_076d;
		IL_076d:
		Vector3 localEulerAngles = base.transform.GetChild(0).localEulerAngles;
		//if (localEulerAngles.z == 0f || !caped)
		//{
			groundDetect();
		//}
		if (grounded)
		{
			if (!bloodMode)
			{
				Vector2 velocity4 = rb.velocity;
				if ((Mathf.Abs(velocity4.x) < 0.1f && maxHeight != 4f && !crouching) || (crouching && maxHeight != 4f))
				{
					jumpForce = jumpHeights.x;
					maxHeight = 4f + maxHeightAdd;
				}
				else
				{
					Vector2 velocity5 = rb.velocity;
					if (Mathf.Abs(velocity5.x) > 0.1f)
					{
						Vector2 velocity6 = rb.velocity;
						if (Mathf.Abs(velocity6.x) < 5.8f && maxHeight != 4.25f && !crouching)
						{
							jumpForce = jumpHeights.y;
							maxHeight = 4.25f + maxHeightAdd;
							goto IL_0950;
						}
					}
					Vector2 velocity7 = rb.velocity;
					if (Mathf.Abs(velocity7.x) > 5.8f && maxHeight != 5f && !crouching)
					{
						jumpForce = jumpHeights.z;
						maxHeight = 5f + maxHeightAdd;
					}
				}
			}
			else if (maxHeight != 1f)
			{
				jumpForce = bloodjumpForce;
				maxHeight = bloodMaxHeight;
			}
		}
		goto IL_0950;
		IL_1c60:
		if (!semiSolidCooldown)
		{
			if (!inverted)
			{
				Vector2 velocity8 = rb.velocity;
				if ((velocity8.y > 0.005f && !ignoreSemiSolid) || (!ignoreSemiSolid && !semiSolidChecker.insideSemiSolid && !semiSolidPrepare))
				{
					ignoreSemiSolid = true;
					Physics2D.IgnoreCollision(semiSolid, ssCol, ignore: true);
					whatIsGround = ((int)whatIsGround ^ (1 << LayerMask.NameToLayer("semiSolidGround")));
					if (!semiSolidCooldown)
					{
						StartCoroutine(ssCooldown());
					}
				}
				else
				{
					Vector2 velocity9 = rb.velocity;
					if (velocity9.y <= 0.005f && ignoreSemiSolid && semiSolidChecker.insideSemiSolid && !anim.GetBool("dive"))
					{
						goto IL_1dbd;
					}
					if (anim.GetBool("dive"))
					{
						Vector2 velocity10 = rb.velocity;
						if (velocity10.y < 0f && ignoreSemiSolid && semiSolidChecker.insideSemiSolid)
						{
							goto IL_1dbd;
						}
					}
				}
			}
			else
			{
				Vector2 velocity11 = rb.velocity;
				if ((velocity11.y < -0.005f && !ignoreSemiSolid) || (!ignoreSemiSolid && !semiSolidChecker.insideSemiSolid && !semiSolidPrepare))
				{
					ignoreSemiSolid = true;
					Physics2D.IgnoreCollision(semiSolid, ssCol, ignore: true);
					whatIsGround = ((int)whatIsGround ^ (1 << LayerMask.NameToLayer("semiSolidGround")));
					if (!semiSolidCooldown)
					{
						StartCoroutine(ssCooldown());
					}
				}
				else
				{
					Vector2 velocity12 = rb.velocity;
					if (velocity12.y >= -0.005f && ignoreSemiSolid && semiSolidChecker.insideSemiSolid && !anim.GetBool("dive"))
					{
						goto IL_1f48;
					}
					if (anim.GetBool("dive"))
					{
						Vector2 velocity13 = rb.velocity;
						if (velocity13.y > 0f && ignoreSemiSolid && semiSolidChecker.insideSemiSolid)
						{
							goto IL_1f48;
						}
					}
				}
			}
		}
		goto IL_1f87;
		IL_0451:
		if (grounded && canDive)
		{
			canDive = false;
		}
		goto IL_046e;
		IL_1f48:
		ignoreSemiSolid = false;
		Physics2D.IgnoreCollision(semiSolid, ssCol, ignore: false);
		whatIsGround = ((int)whatIsGround | (1 << LayerMask.NameToLayer("semiSolidGround")));
		goto IL_1f87;
		IL_01b4:
		Vector3 localScale = base.transform.localScale;
		if (localScale.x == axis.horAxis && !SuperInput.GetKeyDown("Jump"))
		{
			GameData gameData = data;
			Vector3 position = base.transform.position;
			float num = position.x + axis.horAxis;
			Vector3 localScale2 = base.transform.localScale;
			float x = num - 0.5f * localScale2.x;
			Vector3 position2 = base.transform.position;
			string blockName = gameData.GetBlockName(new Vector2(x, position2.y));
			//blood attach
			if (blockName == "blood_ground" && !sticking && !dead)
			{
				sticking = true;
				data.playSound(73, base.transform.position);
				data.stompStreak = 0;
				if (!inverted)
				{
					if (num != 0f)
					{
						platHeight = position.y - 1f;
					}
				}
				else
				{
					if (num != 0f)
					{
						platHeight = position2.y + 1f;
					}
				}
				if (!sfxSound.loop)
				{
					sfxSound.loop = true;
				}
				if (sfxSound.clip != data.sounds[75])
				{
					sfxSound.clip = data.sounds[75];
				}
				if (sfxSound.isPlaying)
				{
					sfxSound.Stop();
				}
				sfxSound.Play();
				grav.maxVelocities.y = grav.savedMaxVelocities.y / 10f;
			}
			else if ((blockName != "blood_ground" && sticking) || dead)
			{
				unStick(jumpOff: false);
			}
		}
		else if (axis.horAxis == 0f && stick == null)
		{
			if (stick != null)
			{
				StopCoroutine(stick);
			}
			stick = StartCoroutine(stickCor());
		}
		else
		{
			if (!SuperInput.GetKeyDown("Jump"))
			{
				Vector3 localScale3 = base.transform.localScale;
				if (localScale3.x == axis.horAxis)
				{
					goto IL_0451;
				}
			}
			unStick(jumpOff: true);
		}
		goto IL_0451;
		IL_1f87:
		if (dead && !skipDeathAnim && (SuperInput.GetKeyDown("Run")))
		{
			skipDeathAnim = true;
		}
		return;
		IL_046e:
		if (pSprites.state != 0 && !anim.GetBool("dive") && controllable && !knockedBack)
		{
			crouch();
		}
		if (grounded && pSprites.state != 0 && !reachedGoal)
		{
			forceCrouch(false);
		}
		if (invFrames > 0 && Application.isFocused && !dead)
		{
			invFrames--;
			if (invFrames == 0)
			{
				if(!blinkingDuringInv)
				{
					data.halvaMusic = false;
					blinkingDuringInv = true;
					HalvaOverlay.gameObject.SetActive(value: false);
					if (!reachedGoal)
					{
						if (!data.switchMusic)
						{
							data.changeMusic(isDefault: true, 0, isLooping: false, normalPitch: false,0.35f);
						}
						else
						{
							data.changeMusic(isDefault: false, 45, isLooping: true, normalPitch: false,0.35f);
						}
					}
					data.halvaStreak = 0;
					invFrames = 60;
				}
				else
				{
					pCol.gameObject.SetActive(false);
					pCol.gameObject.SetActive(true);
				}
			}
			if (invFrames % 5 == 0 || !blinkingDuringInv)
			{
				render.enabled = true;
			}
			else
			{
				render.enabled = false;
			}
		}
		if (Application.isFocused)
		{
			if (stompFrames > 0)
			{
				stompFrames--;
			}
			if (powerUpWaitFrames > 0)
			{
				powerUpWaitFrames--;
				if(powerUpWaitFrames==0)
				{
					pCol.gameObject.SetActive(false);
					pCol.gameObject.SetActive(true);
				}
			}
			if (jumpInputStoreFrames > 0)
			{
				jumpInputStoreFrames--;
			}
			if (gravityFlipCooldown > 0)
			{
				gravityFlipCooldown--;
			}
			if (diveCooldown > 0)
			{
				diveCooldown--;
			}
		}
		if (!grounded)
		{
			if ((axis.acceptXInputs && axis.horAxis == 0f - lastPressedDir && !pressedOppositeDirInAir) || (axis.acceptFakeInputs && axis.artificialX == 0f - lastPressedDir && !pressedOppositeDirInAir))
			{
				pressedOppositeDirInAir = true;
			}
		}
		else if (pressedOppositeDirInAir)
		{
			pressedOppositeDirInAir = false;
		}
		if (axis.acceptXInputs && !axis.acceptFakeInputs && axis.horAxis != 0f)
		{
			lastPressedDir = axis.horAxis;
		}
		else if (axis.acceptFakeInputs && axis.artificialX != 0f)
		{
			lastPressedDir = axis.artificialX;
		}
		float num2 = lastXPos;
		Vector3 position3 = base.transform.position;
		posDifference = num2 - position3.x;
		goto IL_076d;
		IL_1a4c:
		if (!slideSprites[1].gameObject.activeInHierarchy)
		{
			slideSprites[1].Initialize(base.transform.position, 0);
			Transform transform = slideSprites[1].transform;
			Vector3 localScale4 = base.transform.localScale;
			transform.localScale = new Vector3(localScale4.x, 1f, 1f);
		}
		if (!inverted)
		{
			Transform transform2 = slideSprites[1].transform;
			Vector3 position4 = base.transform.position;
			float x2 = (float)Mathf.RoundToInt(position4.x - 0.5f) + 0.5f;
			Vector3 position5 = base.transform.position;
			float y = position5.y + 1f;
			Vector3 position6 = base.transform.position;
			transform2.position = new Vector3(x2, y, position6.z);
		}
		else
		{
			Transform transform3 = slideSprites[1].transform;
			Vector3 position7 = base.transform.position;
			float x3 = (float)Mathf.RoundToInt(position7.x - 0.5f) + 0.5f;
			Vector3 position8 = base.transform.position;
			float y2 = position8.y - 1f;
			Vector3 position9 = base.transform.position;
			transform3.position = new Vector3(x3, y2, position9.z);
		}
		goto IL_1c60;
		IL_0c49:
		if (!dead)
		{
			crusher();
		}
		if (!inverted)
		{
			if (!releasedJump && !grounded)
			{
				Vector2 velocity14 = rb.velocity;
				if (velocity14.y > 0f)
				{
					Vector3 position10 = base.transform.position;
					currentJumpPoint = position10.y;
				}
			}
		}
		else if (!releasedJump && !grounded)
		{
			Vector2 velocity15 = rb.velocity;
			if (velocity15.y < 0f)
			{
				Vector3 position11 = base.transform.position;
				currentJumpPoint = position11.y;
			}
		}
		if (!dead && controllable && Time.timeScale != 0f)
		{
			if ((!anim.GetBool("dive") || (anim.GetBool("dive") && canInterruptDive)))
			{
				jump();
				if (!grounded && !midJump && SuperInput.GetKeyDown("Jump"))
				{
					jumpInputStoreFrames = 10;
				}
			}
			if (!holdingObject && canAttack && SuperInput.GetKeyDown("Run") && !anim.GetBool("dive") && pSprites.state != 3)
			{
				if (pSprites.state <= 1 || pSprites.state == 4)
				{
					canAttack = false;
				}
				else
				{
					if (shootCor != null)
					{
						StopCoroutine(shootCor);
					}
					shootCor = StartCoroutine(shoot());
				}
			}
			if (!holdingObject && canAttack && SuperInput.GetKeyDown("Run") && !anim.GetBool("dive") && pSprites.state == 3 && !crouching && !midSpin && !dead)
			{
				if (axeCor != null)
				{
					StopCoroutine(axeCor);
				}
				axeCor = StartCoroutine(axeAttack());
			}
		}
		if (midDive && anim.GetBool("gravity") && !grounded && Time.timeScale != 0f)
		{
			Vector3 localEulerAngles2 = base.transform.GetChild(0).localEulerAngles;
			if (localEulerAngles2.z != 0f)
			{
				Vector3 localEulerAngles3 = base.transform.GetChild(0).localEulerAngles;
				Vector3 localEulerAngles4 = base.transform.GetChild(0).localEulerAngles;
				if (localEulerAngles4.z < 180f)
				{
					base.transform.GetChild(0).localEulerAngles = Vector3.MoveTowards(localEulerAngles3, Vector3.zero, 1f);
				}
				else
				{
					base.transform.GetChild(0).localEulerAngles = Vector3.MoveTowards(localEulerAngles3, Vector3.zero, -1f);
				}
			}
		}
		if (powerFrames > 0 && !dead && Application.isFocused)
		{
			if ((powerFrames % 5 == 0 && !eternal) || (powerFrames % 7 == 0 && eternal))
			{
				if (powerAnimStates.x != powerAnimStates.y)
				{
					if ((pSprites.state == powerAnimStates.y && powerAnimStates.y != 0) || (pSprites.state == 1 && powerAnimStates.y == 0))
					{
						pSprites.state = powerAnimStates.x;
						if (caped)
						{
							cape.SetActive(value: false);
						}
						else if (powerAnimStates.x == 4)
						{
							cape.SetActive(value: true);
						}
					}
					else if (pSprites.state != powerAnimStates.y)
					{
						if (powerAnimStates.y != 0)
						{
							pSprites.state = powerAnimStates.y;
						}
						else
						{
							pSprites.state = 1;
						}
						if (caped)
						{
							cape.SetActive(value: true);
						}
						else if (powerAnimStates.x == 4)
						{
							cape.SetActive(value: false);
						}
					}
				}
				else
				{
					if (powerAnimStates.y != 0)
					{
						pSprites.state = powerAnimStates.y;
					}
					else
					{
						pSprites.state = 1;
					}
					if (caped && cape.activeInHierarchy)
					{
						cape.SetActive(value: false);
					}
					else if (powerAnimStates.x == 4 && !cape.activeInHierarchy)
					{
						cape.SetActive(value: true);
					}
				}
			}
			powerFrames--;
			if (powerFrames == 0)
			{
				if (caped)
				{
					cape.SetActive(value: true);
				}
				else
				{
					cape.SetActive(value: false);
				}
				pSprites.state = powerAnimStates.y;
				if (Time.timeScale != 1f)
				{
					Time.timeScale = 1f;
				}
				anim.updateMode = AnimatorUpdateMode.Normal;
				pCol.gameObject.SetActive(false);
				pCol.gameObject.SetActive(true);
			}
		}
		if (goalAnimFrames > 0)
		{
			goalAnimFrames--;
		}
		if (!inverted)
		{
			if (!grounded && !dead && !sticking)
			{
				Vector2 velocity16 = rb.velocity;
				if (!inCutscene&&velocity16.y < -0.9f && caped && SuperInput.GetKey("Jump") && grav.maxVelocities.y != capeGravity)
				{
					grav.maxVelocities = new Vector2(grav.maxVelocities.x, capeGravity);
				}
			}
		}
		else if (!grounded && !dead && !sticking)
		{
			Vector2 velocity17 = rb.velocity;
			if (!inCutscene&&velocity17.y > 0.9f && caped && SuperInput.GetKey("Jump") && grav.maxVelocities.y != capeGravity)
			{
				grav.maxVelocities = new Vector2(grav.maxVelocities.x, capeGravity);
			}
		}
		if (((SuperInput.GetKeyUp("Jump")||inCutscene) && grav.maxVelocities.y == capeGravity) || (!caped && grav.maxVelocities.y == capeGravity) || (grounded && grav.maxVelocities.y == capeGravity))
		{
			grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
		}
		Vector3 localEulerAngles5 = base.transform.GetChild(0).localEulerAngles;
		if (localEulerAngles5.z != 0f && grounded)
		{
			base.transform.GetChild(0).localEulerAngles = Vector3.zero;
		}
		Vector2 velocity18 = rb.velocity;
		if (velocity18.x != 0f && grounded && Time.timeScale != 0f && !dead && !knockedBack && !anim.GetBool("dive") && !anim.GetBool("spinAttack"))
		{
			Animator animator2 = anim;
			Vector2 velocity19 = rb.velocity;
			animator2.speed = Mathf.Abs(velocity19.x / 2f);
		}
		else if (!grounded && Time.timeScale != 0f && !dead && !knockedBack && !anim.GetBool("dive") && !anim.GetBool("spinAttack"))
		{
			Animator animator3 = anim;
			Vector2 velocity20 = rb.velocity;
			animator3.speed = Mathf.Abs(velocity20.y / 10f);
		}
		else
		{
			Vector2 velocity21 = rb.velocity;
			if (velocity21.x == 0f && grounded && Time.timeScale != 0f && !dead && !knockedBack)
			{
				anim.speed = animatorSpeed;
			}
		}
		if ((Mathf.Abs(axis.horAxis) == 1f && !axis.acceptFakeInputs) || (Mathf.Abs(axis.verAxis) == 1f && !axis.acceptFakeInputs) || (axis.acceptFakeInputs && Mathf.Abs(axis.artificialX) > 0f) || (axis.acceptFakeInputs && Mathf.Abs(axis.artificialY) > 0f))
		{
			warpHitboxResize.setSize();
		}
		if (sticking)
		{
			if (Time.timeScale == 0f && !sfxPaused)
			{
				sfxPaused = true;
				sfxSound.Pause();
			}
			else if (Time.timeScale != 0f && sfxPaused)
			{
				sfxPaused = false;
				sfxSound.UnPause();
			}
			if (sort.sortingLayerName != "Default")
			{
				sort.sortingLayerName = "Default";
				sort.sortingOrder = -2;
			}
			GameData gameData2 = data;
			Vector3 position12 = base.transform.position;
			float x4 = position12.x;
			Vector3 localScale5 = base.transform.localScale;
			float x5 = x4 + localScale5.x;
			Vector3 position13 = base.transform.position;
			if (gameData2.GetBlockName(new Vector2(x5, position13.y)) == "blood_ground")
			{
				if (!slideSprites[0].gameObject.activeInHierarchy)
				{
					slideSprites[0].Initialize(base.transform.position, 0);
					Transform transform4 = slideSprites[0].transform;
					Vector3 localScale6 = base.transform.localScale;
					transform4.localScale = new Vector3(localScale6.x, 1f, 1f);
				}
				if (!inverted)
				{
					Transform transform5 = slideSprites[0].transform;
					Vector3 position14 = base.transform.position;
					float x6 = (float)Mathf.RoundToInt(position14.x - 0.5f) + 0.5f;
					Vector3 position15 = base.transform.position;
					float y3 = position15.y + 0.5f;
					Vector3 position16 = base.transform.position;
					transform5.position = new Vector3(x6, y3, position16.z);
				}
				else
				{
					Transform transform6 = slideSprites[0].transform;
					Vector3 position17 = base.transform.position;
					float x7 = (float)Mathf.RoundToInt(position17.x - 0.5f) + 0.5f;
					Vector3 position18 = base.transform.position;
					float y4 = position18.y - 0.5f;
					Vector3 position19 = base.transform.position;
					transform6.position = new Vector3(x7, y4, position19.z);
				}
			}
			else if (slideSprites[0].gameObject.activeInHierarchy)
			{
				slideSprites[0].gameObject.SetActive(value: false);
			}
			if (!inverted && pSprites.state > 0)
			{
				GameData gameData3 = data;
				Vector3 position20 = base.transform.position;
				float x8 = position20.x;
				Vector3 localScale7 = base.transform.localScale;
				float x9 = x8 + localScale7.x;
				Vector3 position21 = base.transform.position;
				if (gameData3.GetBlockName(new Vector2(x9, position21.y + 1f)) == "blood_ground")
				{
					goto IL_1a4c;
				}
			}
			if (inverted && pSprites.state > 0)
			{
				GameData gameData4 = data;
				Vector3 position22 = base.transform.position;
				float x10 = position22.x;
				Vector3 localScale8 = base.transform.localScale;
				float x11 = x10 + localScale8.x;
				Vector3 position23 = base.transform.position;
				if (gameData4.GetBlockName(new Vector2(x11, position23.y - 1f)) == "blood_ground")
				{
					goto IL_1a4c;
				}
			}
			if (slideSprites[1].gameObject.activeInHierarchy)
			{
				slideSprites[1].gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (!dead && sort.sortingLayerName != "Player")
			{
				sort.sortingLayerName = "Player";
				sort.sortingOrder = 0;
			}
			if (slideSprites[0].gameObject.activeInHierarchy || slideSprites[1].gameObject.activeInHierarchy)
			{
				slideSprites[0].gameObject.SetActive(value: false);
				slideSprites[1].gameObject.SetActive(value: false);
			}
		}
		goto IL_1c60;
		IL_0950:
		if (!midDive && !crouching && pSprites.state > 0 && !anim.GetBool("dive") && pathPoints[1].y != colliderHeight.y)
		{
			pathPoints[1] = new Vector2(0f, colliderHeight.y);
			pCol.SetPath(0, pathPoints);
		}
		else if ((midDive && pathPoints[1].y != colliderHeight.x) || (pSprites.state == 0 && pathPoints[1].y != colliderHeight.x) || (crouching && pathPoints[1].y != colliderHeight.x))
		{
			pathPoints[1] = new Vector2(0f, colliderHeight.x);
			pCol.SetPath(0, pathPoints);
		}
		Vector2 velocity22 = rb.velocity;
		if (Mathf.Abs(velocity22.x) > 1f && grounded && playerAnimScript.emitDust && !dust.isPlaying)
		{
			if (!inverted)
			{
				dust.transform.localPosition = new Vector3(0.25f, 0f, 0f);
				dust.transform.localScale = base.transform.localScale;
			}
			else
			{
				dust.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
				Transform transform7 = dust.transform;
				Vector3 localScale9 = base.transform.localScale;
				float x12 = 0f - localScale9.x;
				Vector3 localScale10 = base.transform.localScale;
				float y5 = localScale10.y;
				Vector3 localScale11 = base.transform.localScale;
				transform7.localScale = new Vector3(x12, y5, localScale11.z);
			}
			if (!dustSound.isPlaying)
			{
				dustSound.loop = true;
				dustSound.Play();
			}
			dust.Play();
		}
		else
		{
			if ((playerAnimScript.emitDust || !dust.isPlaying) && grounded)
			{
				Vector2 velocity23 = rb.velocity;
				if (!(Mathf.Abs(velocity23.x) <= 1f))
				{
					goto IL_0c49;
				}
			}
			disableDust();
		}
		goto IL_0c49;
		IL_1dbd:
		ignoreSemiSolid = false;
		Physics2D.IgnoreCollision(semiSolid, ssCol, ignore: false);
		whatIsGround = ((int)whatIsGround | (1 << LayerMask.NameToLayer("semiSolidGround")));
		goto IL_1f87;
	}

	private void unStick(bool jumpOff)
	{
		if (sticking)
		{
			sticking = false;
			if (sfxSound.isPlaying)
			{
				sfxSound.Stop();
			}
			data.playSound(74, base.transform.position);
			sfxPaused = false;
			float num = 0f;
			if (SuperInput.GetKey("Jump"))
			{
				num = 8f;
			}
			if (!grounded && jumpOff)
			{
				AxisSimulator axisSimulator = axis;
				Vector3 localScale = base.transform.localScale;
				axisSimulator.axisPosX = 1.2f * localScale.x * -1f;
				if (!inverted)
				{
					if (num != 0f)
					{
						Vector3 position = base.transform.position;
						platHeight = position.y - 1f;
					}
					rb.velocity = new Vector2(axis.axisPosX * horizontalSpeed, 5f + num);
				}
				else
				{
					if (num != 0f)
					{
						Vector3 position2 = base.transform.position;
						platHeight = position2.y + 1f;
					}
					rb.velocity = new Vector2(axis.axisPosX * horizontalSpeed, 0f - (5f + num));
				}
				Animator animator = anim;
				Vector2 velocity = rb.velocity;
				animator.SetFloat("HorSpeed", Mathf.Abs(velocity.x));
			}
		}
		grav.maxVelocities.y = grav.savedMaxVelocities.y;
	}

	private void horizontalMovement()
	{
		if (crouching && grounded && axis.acceptXInputs)
		{
			axis.acceptXInputs = false;
		}
		else if ( (!crouching||!grounded) && (!axis.acceptXInputs && !anim.GetBool("dive") && controllable) )
		{
			axis.acceptXInputs = true;
		}
		Rigidbody2D rigidbody2D = rb;
		float x = axis.axisPosX * horizontalSpeed;
		Vector2 velocity = rb.velocity;
		rigidbody2D.velocity = new Vector2(x, velocity.y);
		Animator animator = anim;
		Vector2 velocity2 = rb.velocity;
		animator.SetFloat("HorSpeed", Mathf.Abs(velocity2.x));
		Vector2 velocity3 = rb.velocity;
		if (Mathf.Abs(velocity3.x) > 0.1f && !running && grounded && !crouching)
		{
			jumpForce = jumpHeights.y;
			maxHeight = 4.25f + maxHeightAdd;
		}
		if (axis.horAxis != -1f)
		{
			Vector2 velocity4 = rb.velocity;
			if (velocity4.x > 0f)
			{
				Vector3 localScale = base.transform.localScale;
				if (localScale.x != 1f && !slide)
				{
					Transform transform = base.transform;
					Vector3 localScale2 = base.transform.localScale;
					float y = localScale2.y;
					Vector3 localScale3 = base.transform.localScale;
					transform.localScale = new Vector3(1f, y, localScale3.z);
					touchingGround = false;
					goto IL_02cb;
				}
			}
		}
		if (axis.horAxis != 1f)
		{
			Vector2 velocity5 = rb.velocity;
			if (velocity5.x < 0f)
			{
				Vector3 localScale4 = base.transform.localScale;
				if (localScale4.x != -1f && !slide)
				{
					Transform transform2 = base.transform;
					Vector3 localScale5 = base.transform.localScale;
					float y2 = localScale5.y;
					Vector3 localScale6 = base.transform.localScale;
					transform2.localScale = new Vector3(-1f, y2, localScale6.z);
					touchingGround = false;
				}
			}
		}
		goto IL_02cb;
		IL_0369:
		if (!canRun)
		{
			return;
		}
		slide = true;
		if (grounded)
		{
			axis.axisAdder = slideSpeed;
		}
		//Slide while holding dir in air
		else if (axis.horAxis != 0f || (axis.artificialX != 0f && axis.acceptFakeInputs))
		{
			axis.axisAdder = 0.04f + slideSpeed;
		}
		//Slide holding no buttons in air
		else
		{
			axis.axisAdder = 0.06f + slideSpeed;
		}
		anim.SetBool("slide", slide);
		return;
		IL_058e:
		if (grounded && !crouching)
		{
			jumpForce = jumpHeights.x;
			maxHeight = 4f + maxHeightAdd;
		}
		slide = false;
		if (!anim.GetBool("dive"))
		{
			running = false;
			if (axis.horAxis != 0f || (axis.artificialX != 0f && axis.acceptFakeInputs))
			{
				if (!reachedGoal)
				{
					axis.setRange(1.2f);
				}
				else
				{
					axis.setRange(1f);
				}
			}
			float bloodDivider = bloodMode ? 2 : 1;
			if (grounded) //Set adder for when standing still
			{
				axis.axisAdder = walkSpeedGain;
			}
			else if (axis.horAxis != 0f || (axis.artificialX != 0f && axis.acceptFakeInputs))
			{
				axis.axisAdder = passiveGain;
			}
			else if (!pressedOppositeDirInAir)
			{
				axis.axisAdder = airSameDirAcceleration / axis.horAxis == 0 ? 2 : 1;
			}
			else
			{
				axis.axisAdder = 0.12f;
			}
		}
		anim.SetBool("slide", slide);
		return;
		IL_02cb:
		Vector2 velocity6 = rb.velocity;
		if (velocity6.x * axis.horAxis < 0f && !anim.GetBool("dive") && axis.acceptXInputs)
		{
			goto IL_0369;
		}
		if (axis.acceptFakeInputs)
		{
			Vector2 velocity7 = rb.velocity;
			if (velocity7.x * axis.artificialX < 0f && !anim.GetBool("dive"))
			{
				goto IL_0369;
			}
		}
		Vector2 velocity8 = rb.velocity;
		if (!(velocity8.x * axis.horAxis > 0f) || !anim.GetBool("slide"))
		{
			if (axis.horAxis == 0f)
			{
				Vector2 velocity9 = rb.velocity;
				if (Mathf.Abs(velocity9.x) <= 0.1f)
				{
					goto IL_058e;
				}
			}
			if (axis.acceptFakeInputs)
			{
				Vector2 velocity10 = rb.velocity;
				if (velocity10.x * axis.artificialX > 0f && anim.GetBool("slide"))
				{
					goto IL_058e;
				}
			}
			if (!axis.acceptFakeInputs || axis.artificialX != 0f)
			{
				return;
			}
			Vector2 velocity11 = rb.velocity;
			if (!(Mathf.Abs(velocity11.x) <= 0.1f))
			{
				return;
			}
		}
		goto IL_058e;
	}

	private void run()
	{
		float bloodDivider = (bloodMode ? 2 : 1);
		if (axis.Run && !running && Mathf.Abs(axis.axisPosX) >= 1.2f && canRun && !anim.GetBool("dive"))
		{
			running = true;
			if (!bloodMode)
			{
				axis.setRange(2.2f);
			}
			else
			{
				axis.setRange(1.6f);
			}
			if (!slide && (grounded || (!grounded && axis.axisPosX * axis.horAxis > 0f) || (axis.acceptXInputs && !grounded && axis.artificialX * axis.horAxis > 0f)))
			{
				axis.axisAdder = runSpeedGain / bloodDivider;
			}
		}
		if ((SuperInput.GetKey("Run") || (!(Mathf.Abs(axis.axisPosX) >= 1.2f) && !running)) && (axis.horAxis != 0f || !running || anim.GetBool("dive")))
		{
			return;
		}
		running = false;
		if (reachedGoal || Mathf.Abs(axis.axisPosX) >= 1.2f)
		{
			if (!reachedGoal)
			{
				axis.setRange(1.2f);
			}
			else
			{
				axis.setRange(1f);
			}
		}
		if (grounded)
		{
			if(axis.axisPosX * axis.horAxis > 0f)
			axis.axisAdder = walkSpeedGain / bloodDivider;
			else axis.axisAdder = walkSpeedGain * bloodDivider;
		}
		else if (axis.horAxis != 0f && axis.artificialX != 0f && axis.acceptFakeInputs)
		{
			axis.axisAdder = passiveGain;
		}
		else if (!pressedOppositeDirInAir)
		{
			axis.axisAdder = airSameDirAcceleration / (axis.horAxis == 0 ? 2 : 1);
		}
		else
		{
			axis.axisAdder = passiveGain;
		}
	}

	private void dive()
	{
		bool key = SuperInput.GetKey("Dash");
		if (!caped && key && !grounded && !anim.GetBool("dive"))
		{
			if (axis.horAxis != 0f)
			{
				Transform transform = base.transform;
				float horAxis = axis.horAxis;
				Vector3 localScale = base.transform.localScale;
				float y = localScale.y;
				Vector3 localScale2 = base.transform.localScale;
				transform.localScale = new Vector3(horAxis, y, localScale2.z);
			}
			if (pSprites.state != 0)
			{
				pathPoints[1] = new Vector2(0f, colliderHeight.x);
			}
			pCol.SetPath(0, pathPoints);
			canDive = false;
			data.sMeterWorks = false;
			anim.SetBool("dive", value: true);
			if (pSprites.state == 3)
			{
				data.playSoundStatic(23);
			}
			else
			{
				data.playSoundStatic(18);
			}
			anim.speed = 1f;
			if (!frameCollision)
			{
				AxisSimulator axisSimulator = axis;
				Vector3 localScale3 = base.transform.localScale;
				axisSimulator.axisPosX = 3f * localScale3.x;
			}
			else
			{
				axis.axisPosX = 0f;
			}
			axis.axisAdder = 0f;
			float num;
			if (axis.horAxis == 0f)
			{
				num = 0f;
			}
			else
			{
				Vector2 velocity = rb.velocity;
				num = Mathf.Abs(velocity.x / 30f);
			}
			Rigidbody2D rigidbody2D = rb;
			float num2 = axis.axisPosX * horizontalSpeed;
			Vector3 localScale4 = base.transform.localScale;
			rigidbody2D.velocity = new Vector2(num2 * localScale4.x, 0f);
			if (!bloodMode)
			{
				diveCor = StartCoroutine(diveTime(0.05f + num, inDir: false));
			}
			else
			{
				diveCor = StartCoroutine(diveTime(0f, inDir: false));
			}
		}
		else if (caped && !grounded && !anim.GetBool("dive") && !capeDive && ((axis.verAxis == 0f && axis.horAxis == 0f) || (axis.verAxis != 0f && axis.horAxis != 0f) || (key && axis.horAxis != 0f)))
		{
			capeDive = true;
		}
		if ((!key && axis.verAxis != 1f) || !caped || !capeDive || grounded || anim.GetBool("dive"))
		{
			return;
		}
		if (axis.horAxis != 0f)
		{
			Transform transform2 = base.transform;
			float horAxis2 = axis.horAxis;
			Vector3 localScale5 = base.transform.localScale;
			float y2 = localScale5.y;
			Vector3 localScale6 = base.transform.localScale;
			transform2.localScale = new Vector3(horAxis2, y2, localScale6.z);
		}
		rb.velocity = Vector2.zero;
		capeDive = false;
		float num3 = axis.verAxis;
		if (num3 == 0f && key)
		{
			num3 = -1f;
		}
		if (pSprites.state != 0)
		{
			pathPoints[1] = new Vector2(0f, colliderHeight.x);
		}
		pCol.SetPath(0, pathPoints);
		canDive = false;
		data.sMeterWorks = false;
		anim.SetBool("dive", value: true);
		if (pSprites.state == 3)
		{
			data.playSoundStatic(23);
		}
		else if (pSprites.state == 4 && SuperInput.GetKey("Up"))
		{
			data.playSoundStatic(94);
		}
		else
		{
			data.playSoundStatic(18);
		}
		anim.speed = 1f;
		releasedJump = !SuperInput.GetKey("Jump");
		axis.axisAdder = 0f;
		float num4;
		if (axis.horAxis == 0f)
		{
			num4 = 0f;
		}
		else
		{
			Vector2 velocity2 = rb.velocity;
			num4 = Mathf.Abs(velocity2.x / 30f);
		}
		if (!inverted)
		{
			if (num3 == 1f)
			{
				midDive = true;
				grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
				grav.pushForces = new Vector2(grav.pushForces.x, grav.savedPushForces.y);
				currentJumpPoint = 0f;
				Vector3 position = base.transform.position;
				startJumpPoint = position.y;
				base.transform.GetChild(0).localEulerAngles = new Vector3(0f, 0f, 45f);
			}
			if (!frameCollision)
			{
				AxisSimulator axisSimulator2 = axis;
				Vector3 localScale7 = base.transform.localScale;
				axisSimulator2.axisPosX = 3f * localScale7.x;
			}
			else
			{
				axis.axisPosX = 0f;
			}
			if (num3 != 0f)
			{
				axis.axisPosY = 3f * num3;
			}
			if (num3 == 1f)
			{
				Rigidbody2D rigidbody2D2 = rb;
				float num5 = axis.axisPosX * horizontalSpeed;
				Vector3 localScale8 = base.transform.localScale;
				rigidbody2D2.velocity = new Vector2(num5 * localScale8.x / 1.5f, axis.axisPosY * horizontalSpeed);
			}
			else
			{
				Rigidbody2D rigidbody2D3 = rb;
				float num6 = axis.axisPosX * horizontalSpeed;
				Vector3 localScale9 = base.transform.localScale;
				rigidbody2D3.velocity = new Vector2(num6 * localScale9.x, 0f);
			}
		}
		else
		{
			if (num3 == 1f)
			{
				midDive = true;
				grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
				grav.pushForces = new Vector2(grav.pushForces.x, grav.savedPushForces.y);
				currentJumpPoint = 0f;
				Vector3 position2 = base.transform.position;
				startJumpPoint = position2.y;
				base.transform.GetChild(0).localEulerAngles = new Vector3(0f, 0f, -45f);
			}
			if (!frameCollision)
			{
				AxisSimulator axisSimulator3 = axis;
				Vector3 localScale10 = base.transform.localScale;
				axisSimulator3.axisPosX = 3f * localScale10.x;
			}
			else
			{
				axis.axisPosX = 0f;
			}
			if (num3 != 0f)
			{
				axis.axisPosY = 3f * num3;
			}
			Rigidbody2D rigidbody2D4 = rb;
			Vector2 velocity3 = rb.velocity;
			rigidbody2D4.velocity = new Vector2(velocity3.x, 0f);
			if (num3 == 1f)
			{
				Rigidbody2D rigidbody2D5 = rb;
				float num7 = axis.axisPosX * horizontalSpeed;
				Vector3 localScale11 = base.transform.localScale;
				rigidbody2D5.velocity = new Vector2(num7 * localScale11.x / 1.5f, axis.axisPosY * horizontalSpeed * -1f);
			}
			else
			{
				Rigidbody2D rigidbody2D6 = rb;
				float num8 = axis.axisPosX * horizontalSpeed;
				Vector3 localScale12 = base.transform.localScale;
				rigidbody2D6.velocity = new Vector2(num8 * localScale12.x, 0f);
			}
		}
		if (!inverted)
		{
			Vector3 position3 = base.transform.position;
			platHeight = position3.y + 30f;
		}
		else
		{
			Vector3 position4 = base.transform.position;
			platHeight = position4.y - 30f;
		}
		diveCor = StartCoroutine(diveTime(0.05f + num4, inDir: true));
	}

	public void bloodFloor(bool enter)
	{
		if (enter)
		{
			if (running)
			{
				axis.setRange(1.6f);
			}
			if (canInterruptDive && grounded)
			{
				axis.axisAdder = 0.3f;
			}
		}
		else
		{
			if (running)
			{
				axis.setRange(2.2f);
			}
			if (canInterruptDive && grounded)
			{
				axis.axisAdder = passiveGain;
			}
		}
	}

	public void poison(bool entering)
	{
		inPoison = entering;
		if (!inPoison || dead)
		{
			render.material = mat;
		}
		if (!dead && inPoison && invFrames == 0 && poisonCor == null)
		{
			poisonCor = StartCoroutine(poisonSequence());
		}
	}

	private void shrinkEvent(bool playSound)
	{
		if (playSound)
		{
			playPowerSound(data.sounds[mode!=1?3:110]);
		}
		render.enabled = true;
		if (Time.timeScale != 0f)
		{
			Time.timeScale = 0f;
		}
		anim.speed = 1f;
		midSpin = false;
		anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		anim.SetBool("Shrink", value: true);
		anim.SetBool("Grow", value: false);
		pathPoints[1] = new Vector2(0f, colliderHeight.x);
		pCol.SetPath(0, pathPoints);
		if (!HalvaOverlay.gameObject.activeInHierarchy)
		{
			invFrames = 120;
		}
		if (crouching)
		{
			crouching = false;
			anim.SetBool("crouch", value: false);
		}
		if (heldObject != null && heldObject.parent == base.transform)
		{
			int num = 1;
			if (inverted)
			{
				num = -1;
			}
			if (heldObject.localPosition == new Vector3(0.318f * (float)num, 0.26f, 0f))
			{
				heldObject.localPosition = new Vector3(0.318f * (float)num, 0.065f, 0f);
			}
			else if (heldObject.localPosition == new Vector3(0.4f * (float)num, 0.8f, 0f))
			{
				heldObject.localPosition = new Vector3(0.5f * (float)num, 0.6f, 0f);
			}
		}
	}

	private IEnumerator oneHitDeath()
	{
		if (pSprites.state != 0)
		{
			powerUpMethod(0, pSprites.state, null);
			shrinkEvent(playSound: false);
			Time.timeScale = 0f;
			yield return new WaitUntil(() => Time.timeScale != 0f && pSprites.state == 0);
		}
		Die();
	}

	public void Damage(bool ignoreStomp, bool ignoreFrames)
	{
		if (midDamage || reachedGoal)
		{
			return;
		}
		midDamage = true;
		if (!eternal)
		{
			base.transform.GetChild(0).localEulerAngles = Vector3.zero;
			if ((ignoreFrames || invFrames == 0) && ((stompFrames == 0 && !inCutscene) || (ignoreStomp && !inCutscene)))
			{
				if (axeAura.gameObject.activeInHierarchy)
				{
					axeAura.gameObject.SetActive(value: false);
				}
				if (pSprites.state > 1 && Time.timeScale != 0f)
				{
					switch (dmgMode)
					{
					default:
						powerUpMethod(1, pSprites.state, null);
						break;
					case 1:
						powerUpMethod(0, pSprites.state, null);
						shrinkEvent(playSound: false);
						break;
					case 2:
						if (!dataShare.godMode)
						{
							StartCoroutine(oneHitDeath());
							return;
						}
						shrinkEvent(playSound: true);
						break;
					}
					invFrames = 120;
				}
				if (pSprites.state == 1 && Time.timeScale != 0f && !anim.GetBool("Shrink"))
				{
					if (dmgMode != 2 || dataShare.godMode)
					{
						shrinkEvent(playSound: true);
					}
					else if (!dataShare.godMode)
					{
						StartCoroutine(oneHitDeath());
						return;
					}
				}
				if (!data.reachedGoal && pSprites.state == 0 && Time.timeScale != 0f && !dead)
				{
					if (!dataShare.godMode)
					{
						Die();
					}
					else
					{
						shrinkEvent(playSound: true);
					}
				}
			}
		}
		else
		{
			if (pSprites.state != 3 || !axeAura.gameObject.activeInHierarchy)
			{
				base.transform.GetChild(0).localEulerAngles = Vector3.zero;
			}
			if ((ignoreFrames || invFrames == 0) && ((stompFrames == 0 && !inCutscene) || (ignoreStomp && !inCutscene)))
			{
				switch (dmgMode)
				{
				default:
					data.health(-1);
					break;
				case 1:
					data.health(-2);
					break;
				case 2:
					data.health(-3);
					break;
				}
				if (data.currentHealth > 0)
				{
					invFrames = 120;
					Vector3 localScale = base.transform.localScale;
					knockbackCor = StartCoroutine(knockBack(0f - localScale.x, 0.01f, 0f,false));
				}
			}
		}
		midDamage = false;
	}

	public void NonLethalDamage(bool ignoreStomp)
	{
		if(invFrames>0)return;
		base.transform.GetChild(0).localEulerAngles = Vector3.zero;
		if (!eternal)
		{
			if (stompFrames == 0 || ignoreStomp)
			{
				if (pSprites.state > 1 && Time.timeScale != 0f)
				{
					powerUpMethod(1, pSprites.state, null);
				}
				if (pSprites.state == 1 && Time.timeScale != 0f)
				{
					shrinkEvent(playSound: true);
				}
				invFrames = 120;
			}
		}
		else if (stompFrames == 0 || ignoreStomp)
		{
			data.health(-1);
			if (data.currentHealth > 0)
			{
				Vector3 localScale = base.transform.localScale;
				knockbackCor = StartCoroutine(knockBack(0f - localScale.x, 0.01f, 0f,false));
			}
			invFrames = 120;
		}
	}

	public void stompEnemy(GameObject other, bool isEnemy)
	{
		if (isEnemy)
		{
			if (other.transform.parent.GetComponent<EnemyCorpseSpawner>() != null && !midSpin)
			{
				EnemyCorpseSpawner component = other.transform.parent.GetComponent<EnemyCorpseSpawner>();
				if (!ignoreSemiSolid)
				{
					ignoreSemiSolid = true;
					Physics2D.IgnoreCollision(semiSolid, ssCol, ignore: true);
					Physics2D.IgnoreLayerCollision(11, 24, ignore: true);
					whatIsGround = ((int)whatIsGround ^ (1 << LayerMask.NameToLayer("semiSolidGround")));
				}
				if (!semiSolidCooldown)
				{
					StartCoroutine(ssCooldown());
				}
				if (anim.GetBool("dive"))
				{
					anim.SetBool("dive", value: false);
					if (canInterruptDive)
					{
						interruptDive = true;
					}
					diveCor = null;
				}
				if (!inverted)
				{
					Vector3 position = other.transform.position;
					startJumpPoint = position.y + component.stompOffset + 1f;
					Vector3 position2 = base.transform.position;
					platHeight = position2.y - 0.3f + 1f;
				}
				else
				{
					Vector3 position3 = other.transform.position;
					startJumpPoint = position3.y - component.stompOffset - 1f;
					Vector3 position4 = base.transform.position;
					platHeight = position4.y + 0.3f - 1f;
				}
				stompFrames = 5;
				base.transform.GetChild(0).localEulerAngles = Vector3.zero;
				Rigidbody2D rigidbody2D = rb;
				Vector2 velocity = rb.velocity;
				rigidbody2D.velocity = new Vector2(velocity.x, 0f);
				grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
				grounded = false;
				axis.currentDivider = axis.savedNormalDivider;
				slide = false;
				pressedOppositeDirInAir = false;
				if (other.transform.parent != null)
				{
					component.spawnCorpse();
				}
				playStompSound(data.stompStreak);
				data.spawnImpact(base.transform.position);
				component.givePoints();
				if (diveCor == null)
				{
					if (SuperInput.GetKey("Jump"))
					{
						releasedJump = false;
						Vector2 vector = base.transform.up * jumpForce / jumpForceReduct;
						rb.AddForce(new Vector2(0f, vector.y), ForceMode2D.Force);
					}
					else
					{
						releasedJump = true;
						Vector2 vector2 = base.transform.up * jumpForce / 2f;
						rb.AddForce(new Vector2(0f, vector2.y), ForceMode2D.Force);
					}
				}
				canHit = true;
			}
		}
		else
		{
			if (anim.GetBool("dive"))
			{
				anim.SetBool("dive", value: false);
				if (canInterruptDive)
				{
					interruptDive = true;
				}
			}
			if (!inverted)
			{
				Vector3 position5 = other.transform.position;
				startJumpPoint = position5.y + 1f;
				Vector3 position6 = base.transform.position;
				platHeight = position6.y - 0.3f + 1f;
			}
			else
			{
				Vector3 position7 = other.transform.position;
				startJumpPoint = position7.y - 1f;
				Vector3 position8 = base.transform.position;
				platHeight = position8.y + 0.3f - 1f;
			}
			stompFrames = 5;
			base.transform.GetChild(0).localEulerAngles = Vector3.zero;
			Rigidbody2D rigidbody2D2 = rb;
			Vector2 velocity2 = rb.velocity;
			rigidbody2D2.velocity = new Vector2(velocity2.x, 0f);
			grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
			pressedOppositeDirInAir = false;
			grounded = false;
			axis.currentDivider = axis.savedNormalDivider;
			slide = false;
			currentJumpPoint = 0f;
			if (SuperInput.GetKey("Jump"))
			{
				releasedJump = false;
				Vector2 vector3 = base.transform.up * jumpForce * 0.8f / jumpForceReduct;
				rb.AddForce(new Vector2(0f, vector3.y), ForceMode2D.Force);
			}
			else
			{
				releasedJump = true;
				Vector2 vector4 = base.transform.up * jumpForce / 4f;
				rb.AddForce(new Vector2(0f, vector4.y), ForceMode2D.Force);
			}
			canHit = true;
		}
		if (!canRun && !crouching)
		{
			canRun = true;
		}
	}

	public void stompBoss(GameObject other, bool canHighBounce)
	{
		if (dead)
		{
			return;
		}
		Vector3 position = other.transform.position;
		startJumpPoint = position.y + 1f;
		stompFrames = 10;
		grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
		base.transform.GetChild(0).localEulerAngles = Vector3.zero;
		Rigidbody2D rigidbody2D = rb;
		Vector2 velocity = rb.velocity;
		rigidbody2D.velocity = new Vector2(velocity.x, 0f);
		grounded = false;
		axis.currentDivider = axis.savedNormalDivider;
		pressedOppositeDirInAir = false;
		slide = false;
		if (SuperInput.GetKey("Jump") && canHighBounce)
		{
			releasedJump = false;
			Vector2 vector = base.transform.up * jumpForce / jumpForceReduct;
			rb.AddForce(new Vector2(0f, vector.y), ForceMode2D.Force);
		}
		else
		{
			releasedJump = true;
			Vector2 vector2 = base.transform.up * jumpForce / 2f;
			rb.AddForce(new Vector2(0f, vector2.y), ForceMode2D.Force);
		}
		data.spawnImpact(base.transform.position);
		if (anim.GetBool("dive"))
		{
			anim.SetBool("dive", value: false);
			if (canInterruptDive)
			{
				interruptDive = true;
			}
		}
		diveCor = null;
		canHit = true;
		if (!canRun && !crouching)
		{
			canRun = true;
		}
	}

	public void Die()
	{
		if (data.reachedGoal||dead)
		{
			return;
		}
		pauseMenu.pauseLock = true;
		base.transform.GetChild(0).localEulerAngles = Vector3.zero;
		render.enabled = true;
		pauseMenu.enabled = false;
		data.timerGoesDown = false;
		data.litSMeterArrows = 0;
		data.sMeterWorks = false;
		grav.maxVelocities = new Vector2(grav.maxVelocities.x, grav.savedMaxVelocities.y);
		data.UpdateSMeterArrow();
		cam.autoscrollDir = Vector2.zero;
		dead = true;
		pauseMenu.playerDead = dead;
		if (cape != null)
		{
			cape.GetComponent<CapeScript>().stopCapeSound();
		}
		if (sticking)
		{
			unStick(jumpOff: false);
		}
		if (poisonCor != null)
		{
			StopCoroutine(poisonCor);
		}
		knockedBack = false;
		if (knockbackCor != null)
		{
			StopCoroutine(knockbackCor);
		}
		render.material = mat;
		controllable = false;
		inCutscene = true;
		axis.acceptXInputs = false;
		axis.acceptYInputs = false;
		anim.speed = 1f;
		anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		Time.timeScale = 0f;
		StartCoroutine(deathMelody());
		grounded = false;
		axis.currentDivider = axis.savedNormalDivider;
		rb.drag = 0f;
		grav.pushForces = new Vector2(grav.pushForces.x, grav.pushForces.y / 1.5f);
		anim.SetBool("dead", dead);
		if (mode != 1)
		{
			Vector2 velocity = rb.velocity;
			if (Mathf.Abs(velocity.x) >= 1f)
			{
				Rigidbody2D rigidbody2D = rb;
				Vector2 velocity2 = rb.velocity;
				rigidbody2D.velocity = new Vector2(0f - velocity2.x, 0f);
			}
			else
			{
				Rigidbody2D rigidbody2D2 = rb;
				Vector3 localScale = base.transform.localScale;
				rigidbody2D2.velocity = new Vector2(1f * (0f - localScale.x), 0f);
			}
			Vector2 vector = base.transform.up * jumpForce / 2f;
			rb.AddForce(new Vector2(0f, vector.y), ForceMode2D.Force);
		}
		else
		{
			cam.lockCamera = true;
			cam.lockscroll = true;
			rb.velocity = Vector2.zero;
			pCol.enabled = false;
			ssCol.enabled = false;
			rb.constraints = (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation);
			if (data.lives > 0 || data.infiniteLives)
			{
				Vector2 vector2 = base.transform.up * jumpForce / 1.5f;
				rb.AddForce(new Vector2(0f, vector2.y), ForceMode2D.Force);
			}
			transform.SetParent(null);
		}
		data.stopAllMusic();
		if (postDeathCanvas != null)
		{
			StartCoroutine(deathFade());
		}
		else
		{
			Debug.LogError("No post death canvas found.");
		}
		data.DataS.storedItem = 0;
	}

	public void goToCutsceneMode(bool toCutScene)
	{
		if (pauseMenu == null)
		{
			pauseMenu = GameObject.Find("PauseCanvas").GetComponent<MenuScript>();
		}
		pauseMenu.pauseLock = toCutScene;
		controllable = !toCutScene;
		inCutscene = toCutScene;
		axis.acceptXInputs = !toCutScene;
		axis.acceptYInputs = !toCutScene;
	}

	public IEnumerator timeUpDeath()
	{
		Animator a = postDeathCanvas.GetComponent<Animator>();
		dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		if (data.lives != 0 || data.infiniteLives)
		{
			if (skipDeathAnim)
			{
				cam.fadeAdditive = 0.06f;
			}
			cam.fadeScreen(fadeIn: true);
			postDeathCanvas.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>()
				.text = data.lives.ToString("00");
			postDeathCanvas.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>()
				.text = (data.lives - 1).ToString("00");
			if (mode == 1)
			{
				postDeathCanvas.transform.GetChild(1).GetComponent<Image>().sprite = HUD.transform.GetChild(2).GetComponent<Image>().sprite;
			}
			yield return new WaitUntil(() => cam.fadeAnim >= 1f);
			if (!skipDeathAnim && !data.infiniteLives)
			{
				postDeathCanvas.SetActive(value: true);
				yield return 0;
				cam.setInstantFade(fadeIn: false);
			}
			if (GameObject.Find("DataShare") != null)
			{
				if (!data.infiniteLives)
				{
					DataS.lives = data.lives - 1;
				}
				DataS.playerState = 0;
				yield return 0;
				if (skipDeathAnim || data.infiniteLives)
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
			}
			yield break;
		}
		for (int i = 0; i < HUD.transform.childCount; i++)
		{
			HUD.transform.GetChild(i).gameObject.SetActive(value: false);
		}
		HUD.GetComponent<Canvas>().sortingOrder = 18;
		Time.timeScale = 0f;
		data.sources[0].pitch = 1f;
		data.changeMusic(isDefault: false, 30, isLooping: false, normalPitch: true,0.35f);
		UnityEngine.Object.Destroy(GameObject.Find("timeUp"));
		ScreenOverlay overlay = cam.overlay;
		if (!overlay.enabled)
		{
			overlay.enabled = true;
		}
		overlay.grab = true;
		yield return new WaitUntil(() => overlay.screencap != null);
		Color32 col = overlay.screencap.color;
		overlay.screencap.color = new Color32(col.r, col.g, col.b, 0);
		byte fadeValue2 = 0;
		while (fadeValue2 < 254)
		{
			fadeValue2 = (byte)(fadeValue2 + 2);
			overlay.screencap.color = new Color32(col.r, col.g, col.b, fadeValue2);
			yield return 0;
		}
		fadeValue2 = byte.MaxValue;
		int waitframes2;
		for (waitframes2 = 240; waitframes2 > 0; waitframes2--)
		{
			waitframes2--;
			yield return 0;
		}
		postDeathCanvas.GetComponent<Canvas>().sortingOrder = 17;
		postDeathCanvas.SetActive(value: true);
		a.SetBool("Alt", (mode == 1) ? true : false);
		a.SetBool("GameOver", value: true);
		DataS.playerState = 0;
		DeathEventTriggered();
		while (fadeValue2 > 0)
		{
			fadeValue2 = (byte)((fadeValue2 != 1) ? ((byte)(fadeValue2 - 2)) : 0);
			if (fadeValue2 == 61)
			{
				postDeathCanvas.GetComponent<Animator>().SetBool("Intro", value: true);
			}
			overlay.screencap.color = new Color32(col.r, col.g, col.b, fadeValue2);
			yield return 0;
		}
	}

	private IEnumerator deathFade()
	{
		Animator a = postDeathCanvas.GetComponent<Animator>();
		dataShare DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		if (data.lives != 0 || data.infiniteLives)
		{
			yield return new WaitUntil(() => Time.timeScale != 0f);
			yield return 0;
			if (!skipDeathAnim)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (!skipDeathAnim)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (!skipDeathAnim)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (!skipDeathAnim)
			{
				yield return new WaitForSeconds(0.5f);
			}
			cam.fadeScreen(fadeIn: true);
			cam.shakeCamera(0f, 0f);
			postDeathCanvas.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>()
				.text = data.lives.ToString("00");
			postDeathCanvas.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>()
				.text = (data.lives - 1).ToString("00");
			if (mode == 1)
			{
				postDeathCanvas.transform.GetChild(1).GetComponent<Image>().sprite = HUD.transform.GetChild(2).GetComponent<Image>().sprite;
			}
			if (skipDeathAnim)
			{
				cam.fadeAdditive = 0.06f;
			}
			yield return new WaitUntil(() => cam.fadeAnim >= 1f);
			if (!skipDeathAnim && !data.infiniteLives)
			{
				postDeathCanvas.SetActive(value: true);
				yield return 0;
				cam.setInstantFade(fadeIn: false);
			}
			if (GameObject.Find("DataShare") != null)
			{
				if (!data.infiniteLives)
				{
					DataS.lives = data.lives - 1;
				}
				DataS.playerState = 0;
				yield return 0;
				if (skipDeathAnim || data.infiniteLives)
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
			}
			yield break;
		}
		yield return new WaitForSeconds(1f);
		cam.shakeCamera(0.1f, 0.2f);
		for (int i = 0; i < HUD.transform.childCount; i++)
		{
			HUD.transform.GetChild(i).gameObject.SetActive(value: false);
		}
		HUD.GetComponent<Canvas>().sortingOrder = 18;
		Time.timeScale = 0f;
		data.sources[0].pitch = 1f;
		data.changeMusic(isDefault: false, 30, isLooping: false, normalPitch: true,0.35f);
		ScreenOverlay overlay = cam.overlay;
		if (!overlay.enabled)
		{
			overlay.enabled = true;
		}
		overlay.grab = true;
		yield return new WaitUntil(() => overlay.screencap != null);
		Color32 col = overlay.screencap.color;
		overlay.screencap.color = new Color32(col.r, col.g, col.b, 0);
		byte fadeValue2 = 0;
		while (fadeValue2 < 254)
		{
			fadeValue2 = (byte)(fadeValue2 + 2);
			overlay.screencap.color = new Color32(col.r, col.g, col.b, fadeValue2);
			yield return 0;
		}
		fadeValue2 = byte.MaxValue;
		int waitframes2;
		for (waitframes2 = 240; waitframes2 > 0; waitframes2--)
		{
			waitframes2--;
			yield return 0;
		}
		Time.timeScale = 0f;
		postDeathCanvas.GetComponent<Canvas>().sortingOrder = 17;
		postDeathCanvas.SetActive(value: true);
		a.SetBool("Alt", (mode == 1) ? true : false);
		DataS.playerState = 0;
		a.SetBool("GameOver", value: true);
		DeathEventTriggered();
		while (fadeValue2 > 0)
		{
			fadeValue2 = (byte)((fadeValue2 != 1) ? ((byte)(fadeValue2 - 2)) : 0);
			if (fadeValue2 == 61)
			{
				postDeathCanvas.GetComponent<Animator>().SetBool("Intro", value: true);
			}
			overlay.screencap.color = new Color32(col.r, col.g, col.b, fadeValue2);
			yield return 0;
		}
	}

	public IEnumerator knockBack(float direction, float strength, float dazeTime, bool waitforground)
	{
		if (dead)
		{
			yield break;
		}
		axis.axisPosX = 0f;
		stompFrames = 0;
		axeAura.gameObject.SetActive(value: false);
		knockedBack = true;
		data.sMeterWorks = false;
		if (diveCor != null)
		{
			//print("Dive interrupted");
			StopCoroutine(diveCor);
			if (!inverted)
			{
				grav.pushForces = new Vector2(grav.pushForces.x, 0f - Mathf.Abs(savedGravity));
			}
			else
			{
				grav.pushForces = new Vector2(grav.pushForces.x, Mathf.Abs(savedGravity));
			}
			anim.SetBool("gravity", value: true);
			if (pSprites.state == 3)
			{
				if (midDive)
				{
					data.playSoundStatic(25);
				}
				midSpin = false;
				stompFrames = 5;
			}
			axis.axisAdder = passiveGain;
			midDive = false;
			grav.maxVelocities = new Vector2(savedMax, grav.maxVelocities.y);
			anim.SetBool("dive", value: false);
			diveCor = null;
			slide = false;
			anim.SetBool("slide", slide);
			running = false;
			canRun = true;
			if (pSprites.state != 0 && !crouching)
			{
				pathPoints[1] = new Vector2(0f, colliderHeight.y);
			}
			pCol.SetPath(0, pathPoints);
		}
		base.transform.GetChild(0).localEulerAngles = Vector3.zero;
		anim.SetFloat("HorSpeed", 0f);
		anim.SetBool("Grounded", value: false);
		yield return new WaitUntil(() => Time.timeScale != 0f);
		controllable = false;
		axis.acceptXInputs = false;
		axis.acceptYInputs = false;
		grounded = false;
		axis.currentDivider = axis.savedNormalDivider;
		rb.drag = 0f;
		rb.velocity = Vector2.zero;
		running = false;
		midJump = false;
		yield return 0;
		float gravityY = grav.pushForces.y;
		grav.maxVelocities = new Vector2(grav.maxVelocities.x * 2f, grav.maxVelocities.y);
		grav.pushForces = new Vector2(grav.pushForces.x, grav.pushForces.y * 0.8f);
		if (!inverted)
		{
			rb.velocity = new Vector2(direction * 80f * strength, 0f);
		}
		else
		{
			rb.velocity = new Vector2(direction * -80f * strength, 0f);
		}
		Vector2 dir = base.transform.up * jumpForce / 2f;
		rb.AddForce(new Vector2(0f, dir.y), ForceMode2D.Force);
		if (pSprites.state != 0 && !crouching)
		{
			pathPoints[1] = new Vector2(0f, colliderHeight.y);
		}
		else
		{
			pathPoints[1] = new Vector2(0f, colliderHeight.x);
		}
		pCol.SetPath(0, pathPoints);
		if (dazeTime != 0f)
		{
			if(waitforground)
			yield return new WaitUntil(() => grounded);
		}
		else
		{
			yield return new WaitUntil(() => grounded || invFrames <= 100);
		}
		rb.drag = 3f;
		yield return new WaitForSeconds(dazeTime);
		if (!running)
		{
			axis.setRange(1.2f);
		}
		rb.drag = 0f;
		data.sMeterWorks = true;
		controllable = !inCutscene;
		
		axis.acceptXInputs = !inCutscene;
		axis.acceptYInputs = !inCutscene;
		grav.maxVelocities = new Vector2(savedMax, grav.maxVelocities.y);
		if (!inverted)
		{
			grav.pushForces = new Vector2(grav.pushForces.x, 0f - Mathf.Abs(gravityY));
		}
		else
		{
			grav.pushForces = new Vector2(grav.pushForces.x, Mathf.Abs(gravityY));
		}
		knockedBack = false;
		knockbackCor = null;
	}

	private IEnumerator deathMelody()
	{
		if (data.lives != 0 || data.infiniteLives)
		{
			data.playSoundStatic(15);
			yield return new WaitUntil(() => Time.timeScale != 0f);
			data.playSoundStatic(16);
		}
		else
		{
			data.playSoundStatic(29);
		}
	}

	private void showBloodImpact(int mode, Vector3 pos, bool randomX)
	{
		if ((inverted || mode != 1 || !(data.GetBlockName(base.transform.position) == "blood_ground")) && (!inverted || mode != 1 || !(data.GetBlockName(base.transform.position + Vector3.up) == "blood_ground")))
		{
			return;
		}
		for (int i = 0; i < ImpactSprites.Length; i++)
		{
			if (ImpactSprites[i].gameObject.activeInHierarchy)
			{
				continue;
			}
			if (randomX)
			{
				int num = UnityEngine.Random.Range(0, 2);
				int num2 = 1;
				int num3 = 1;
				if (num == 0)
				{
					num2 = -1;
				}
				if (inverted)
				{
					num3 = -1;
				}
				ImpactSprites[i].transform.localScale = new Vector3(num2, num3, 1f);
			}
			if (!inverted)
			{
				ImpactSprites[i].Initialize(pos, mode);
			}
			else
			{
				ImpactSprites[i].Initialize(pos - Vector3.up, mode);
			}
			break;
		}
	}

	private void jump()
	{
		if ((SuperInput.GetKeyDown("Jump") && grounded && !midDive) || (SuperInput.GetKey("Jump") && jumpInputStoreFrames > 0 && grounded && !midDive) || (debugAutoJump && grounded && !midDive))
		{
			if (crouching)
			{
				crouch();
			}
			if (!ignoreSemiSolid)
			{
				ignoreSemiSolid = true;
				Physics2D.IgnoreCollision(semiSolid, ssCol, ignore: true);
				Physics2D.IgnoreLayerCollision(11, 24, ignore: true);
				whatIsGround = ((int)whatIsGround ^ (1 << LayerMask.NameToLayer("semiSolidGround")));
			}
			if (!semiSolidCooldown)
			{
				StartCoroutine(ssCooldown());
			}
			if (bloodMode)
			{
				showBloodImpact(1, base.transform.position + new Vector3(0f, 0.5f, 0f), randomX: true);
			}
			axis.currentDivider = axis.savedNormalDivider;
			if (axis.horAxis != 0f)
			{
				Transform transform = base.transform;
				float horAxis = axis.horAxis;
				Vector3 localScale = base.transform.localScale;
				float y = localScale.y;
				Vector3 localScale2 = base.transform.localScale;
				transform.localScale = new Vector3(horAxis, y, localScale2.z);
			}
			if (stickToGround != null)
			{
				StopCoroutine(stickToGround);
			}
			diveAfterJump = false;
			if (canInterruptDive)
			{
				interruptDive = true;
			}
			if (!bloodMode)
			{
				aSource.PlayOneShot(data.sounds[2]);
			}
			else
			{
				if (data.getBlockName(base.transform.position, data.map) == "test blocks_19")
				{
					data.spawnLipParticle(base.transform.position);
				}
				aSource.PlayOneShot(data.sounds[76]);
			}
			midJump = true;
			canHit = true;
			Vector3 position = base.transform.position;
			startJumpPoint = position.y;
			Rigidbody2D rigidbody2D = rb;
			Vector2 velocity = rb.velocity;
			rigidbody2D.velocity = new Vector2(velocity.x, 0f);
			grounded = false;
			if (!inverted)
			{
				Vector3 position2 = base.transform.position;
				platHeight = position2.y - 0.3f;
			}
			else
			{
				Vector3 position3 = base.transform.position;
				platHeight = position3.y + 0.3f;
			}
			slide = false;
			releasedJump = false;
			Vector2 vector = base.transform.up * jumpForce / jumpForceReduct;
			rb.AddForce(new Vector2(0f, vector.y), ForceMode2D.Force);
			StartCoroutine(jumpDur());
			jumpInputStoreFrames = 0;
		}
		if (!inverted)
		{
			if (SuperInput.GetKeyUp("Jump"))
			{
				Vector2 velocity2 = rb.velocity;
				if (velocity2.y > releaseJumpVelocity && !releasedJump && !midDive)
				{
					goto IL_03fc;
				}
			}
			if (currentJumpPoint - startJumpPoint > maxHeight && !releasedJump)
			{
				Vector2 velocity3 = rb.velocity;
				if (!(velocity3.y > releaseJumpVelocity) || midDive)
				{
					return;
				}
				goto IL_03fc;
			}
			return;
		}
		if (SuperInput.GetKeyUp("Jump"))
		{
			Vector2 velocity4 = rb.velocity;
			if (velocity4.y < releaseJumpVelocity * -1f && !releasedJump && !midDive)
			{
				goto IL_04e4;
			}
		}
		if (currentJumpPoint - startJumpPoint < 0f - maxHeight)
		{
			Vector2 velocity5 = rb.velocity;
			if (!(velocity5.y < releaseJumpVelocity * -1f) || releasedJump || midDive)
			{
				return;
			}
			goto IL_04e4;
		}
		return;
		IL_03fc:
		releasedJump = true;
		Rigidbody2D rigidbody2D2 = rb;
		Vector2 velocity6 = rb.velocity;
		float x = velocity6.x;
		Vector2 velocity7 = rb.velocity;
		rigidbody2D2.velocity = new Vector2(x, velocity7.y / 2.25f);
		return;
		IL_04e4:
		releasedJump = true;
		Rigidbody2D rigidbody2D3 = rb;
		Vector2 velocity8 = rb.velocity;
		float x2 = velocity8.x;
		Vector2 velocity9 = rb.velocity;
		rigidbody2D3.velocity = new Vector2(x2, velocity9.y / 2.25f);
	}

	public void resetCanHit()
	{
		canHit = true;
	}

	private void crouch()
	{
		if (grounded)
		{
			if (!inverted)
			{
				StartPoint = base.transform.position + new Vector3(0f, 0.1f, 0f);
			}
			else
			{
				StartPoint = base.transform.position + new Vector3(0f, -0.1f, 0f);
			}
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(StartPoint, base.transform.up, 1.5f, whatIsSolidGround);
		if (!slide && axis.verAxis == -1f && !crouching && !midSpin && diveCor == null && grounded)
		{
			crouching = true;
			axis.setRange(1.2f);
			canRun = false;
			running = false;
			pathPoints[1] = new Vector2(0f, colliderHeight.x);
			pCol.SetPath(0, pathPoints);
			anim.SetBool("crouch", crouching);
			if (grounded)
			{
				if (!running)
				{
					axis.axisAdder = walkSpeedGain;
				}
				else
				{
					axis.axisAdder = 0.05f;
				}
			}
			else if (!grounded && !running)
			{
				axis.axisAdder = passiveGain;
			}
			else if (!grounded && running)
			{
				axis.axisAdder = 0.05f;
			}
		}
		else
		{
			if ((axis.verAxis == -1f || !crouching || !(raycastHit2D.collider == null) || !grounded) && (!canAttack || !SuperInput.GetKeyDown("Run") || !grounded || !crouching || !(raycastHit2D.collider == null)))
			{
				return;
			}
			if (grounded)
			{
				if (!running || Mathf.Abs(axis.axisPosX) <= 1.2f)
				{
					running = false;
					axis.axisAdder = walkSpeedGain;
				}
				else
				{
					axis.axisAdder = runSpeedGain;
				}
			}
			if (running)
			{
				axis.setRange(2.2f);
			}
			crouching = false;
			crouchBlock = false;
			pathPoints[1] = new Vector2(0f, colliderHeight.y);
			pCol.SetPath(0, pathPoints);
			anim.SetBool("crouch", crouching);
			/*if (canAttack && SuperInput.GetKeyDown("Run") && !anim.GetBool("dive") && pSprites.state == 3 && grounded && raycastHit2D.collider == null && !dead)
			{
				if (axeCor != null)
				{
					StopCoroutine(axeCor);
				}
				axeCor = StartCoroutine(axeAttack());
			}*/
		}
	}

	public void unCrouch()
	{
		if (grounded)
		{
			if (!running || Mathf.Abs(axis.axisPosX) <= 1.2f)
			{
				running = false;
				axis.axisAdder = walkSpeedGain;
			}
			else
			{
				axis.axisAdder = runSpeedGain;
			}
		}
		crouching = false;
		crouchBlock = false;
		pathPoints[1] = new Vector2(0f, colliderHeight.y);
		pCol.SetPath(0, pathPoints);
		anim.SetBool("crouch", crouching);
		/*if (canAttack && SuperInput.GetKeyDown("Run") && !anim.GetBool("dive") && pSprites.state == 3 && grounded && !dead)
		{
			if (axeCor != null)
			{
				StopCoroutine(axeCor);
			}
			axeCor = StartCoroutine(axeAttack());
		}*/
	}

	private void forceCrouch(bool ignoreIfDive)
	{
		if (grounded)
		{
			if (!inverted)
			{
				StartPoint = base.transform.position + new Vector3(0f, 0.1f, 0f);
			}
			else
			{
				StartPoint = base.transform.position + new Vector3(0f, -0.1f, 0f);
			}
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(StartPoint, base.transform.up, 1.34f, whatIsSolidGround);
		if (!crouchBlock && raycastHit2D.collider != null && !raycastHit2D.collider.isTrigger && (ignoreIfDive || diveCor == null))
		{
			crouchBlock = true;
			if (!crouching)
			{
				Debug.DrawLine(StartPoint, raycastHit2D.point, Color.green, 3f);
				slide = false;
				anim.SetBool("slide", slide);
				crouching = true;
				canRun = false;
				running = false;
				axis.setRange(1.2f);
				pathPoints[1] = new Vector2(0f, colliderHeight.x);
				pCol.SetPath(0, pathPoints);
				anim.SetBool("crouch", crouching);
				axis.axisAdder = walkSpeedGain;
			}
		}
		else if (raycastHit2D.collider != null && crouchBlock)
		{
			crouchBlock = false;
		}
	}

	private void crusher()
	{
		float inv = inverted?-1:1;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position + new Vector3(0f, 0.1f*inv, 0f), -base.transform.right, 0.25f, whatIsGround);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(base.transform.position + new Vector3(0f, 0.1f*inv, 0f), base.transform.right, 0.25f, whatIsGround);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(base.transform.position + new Vector3(0f, 0.1f*inv, 0f), -base.transform.up, 0.2f, whatIsGround);
		RaycastHit2D raycastHit2D4 = (pathPoints[1].y != colliderHeight.x) ? 
		Physics2D.Raycast(base.transform.position + new Vector3(0f, 0.1f*inv, 0f), base.transform.up, 1.3f, whatIsSolidGround) : 
		Physics2D.Raycast(base.transform.position + new Vector3(0f, 0.1f*inv, 0f), base.transform.up, 0.7f, whatIsSolidGround);
		int num = -1;
		if (((raycastHit2D.collider != null) & (raycastHit2D2.collider != null)) && raycastHit2D.collider.transform != raycastHit2D2.collider.transform)
		{
			num = 0;
		}
		else if (((raycastHit2D4.collider != null) & (raycastHit2D3.collider != null)) && raycastHit2D4.collider.transform != raycastHit2D3.collider.transform)
		{
			num = 1;
		}
		switch (num)
		{
		case -1:
			return;
		case 0:
			dmgMode = 2;
			break;
		}
		//print(num);
		if(num==1)
		{
			if(raycastHit2D4.collider.name.Contains("NoCol")||raycastHit2D3.collider.name.Contains("NoCol"))
			return;
		}
		if (num == 0 || pSprites.state == 0 || eternal)
		{
			pauseMenu.pauseLock = true;
			pauseMenu.enabled = false;
			if (ssCol.enabled)
			{
				ssCol.enabled = false;
			}
			pCol.enabled = false;
			cam.lockCamera = true;
			rb.constraints = (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation);
			base.transform.GetChild(0).localEulerAngles = Vector3.zero;
		}
		if (eternal)
		{
			data.health(-3);
		}
		Damage(ignoreStomp: true, ignoreFrames: true);
	}

	private void determineEffect(int ray1, int ray2)
	{
		int num = 0;
		if ((ray1 == ray2 && ray1 != 1) || ray1 == 1 || ray2 == 1)
		{
			if (ray1 != 1)
			{
				num = ray1;
			}
			else if (ray2 != 1)
			{
				num = ray2;
			}
		}
		switch (num)
		{
		default:
			if (axis.currentDivider == axis.iceDivider)
			{
				axis.currentDivider = axis.savedNormalDivider;
			}
			if (bloodMode)
			{
				bloodMode = false;
				bloodFloor(enter: false);
			}
			break;
		case 2:
			if (bloodMode)
			{
				bloodMode = false;
				bloodFloor(enter: false);
			}
			if (grounded && axis.currentDivider != axis.iceDivider)
			{
				axis.currentDivider = axis.iceDivider;
			}
			break;
		case 3:
			if (axis.currentDivider == axis.iceDivider)
			{
				axis.currentDivider = axis.savedNormalDivider;
			}
			if (!bloodMode)
			{
				bloodMode = true;
				bloodFloor(enter: true);
			}
			break;
		}
	}

	private bool checkRay(RaycastHit2D ray)
	{
		return (ray.collider != null) ? true : false;
	}

	private void groundDetect()
	{
		int num = 1;
		if (inverted)
		{
			num = -1;
		}
		Vector3 position = base.transform.position;
		float x = position.x - 0.215f;
		Vector3 position2 = base.transform.position;
		float y = position2.y + 0.32f * (float)num;
		Vector3 position3 = base.transform.position;
		Vector3 vector = new Vector3(x, y, position3.z);
		Vector3 position4 = base.transform.position;
		float x2 = position4.x + 0.215f;
		Vector3 position5 = base.transform.position;
		float y2 = position5.y + 0.32f  * (float)num;
		Vector3 position6 = base.transform.position;
		Vector3 vector2 = new Vector3(x2, y2, position6.z);
		Vector3 vector3 = Vector3.zero;
		Vector3 vector4 = Vector3.zero;
		Vector3 vector5 = Vector3.zero;
		Vector3 vector6 = Vector3.zero;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, -base.transform.up, 0.35f, whatIsGround);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector2, -base.transform.up, 0.35f, whatIsGround);
		bool flag = false;
		if (diveCor == null)
		{
			Vector3 position7 = base.transform.position;
			float x3 = position7.x;
			Vector3 position8 = base.transform.position;
			float y3 = position8.y + 0.2f * (float)num;
			Vector3 position9 = base.transform.position;
			vector3 = new Vector3(x3, y3, position9.z);
			if (pSprites.state > 0 && !crouching)
			{
				Vector3 position10 = base.transform.position;
				float x4 = position10.x;
				Vector3 position11 = base.transform.position;
				float y4 = position11.y + 1.2f * (float)num;
				Vector3 position12 = base.transform.position;
				vector4 = new Vector3(x4, y4, position12.z);
			}
		}
		else
		{
			Vector3 position13 = base.transform.position;
			float x5 = position13.x;
			Vector3 position14 = base.transform.position;
			float y5 = position14.y + 0.6f * (float)num;
			Vector3 position15 = base.transform.position;
			vector3 = new Vector3(x5, y5, position15.z);
			Vector3 position16 = base.transform.position;
			float x6 = position16.x;
			Vector3 position17 = base.transform.position;
			float y6 = position17.y + 0.4f * (float)num;
			Vector3 position18 = base.transform.position;
			vector4 = new Vector3(x6, y6, position18.z);
			Vector3 position19 = base.transform.position;
			float x7 = position19.x;
			Vector3 position20 = base.transform.position;
			float y7 = position20.y + 1f * (float)num;
			Vector3 position21 = base.transform.position;
			vector5 = new Vector3(x7, y7, position21.z);
			Vector3 position22 = base.transform.position;
			float x8 = position22.x;
			Vector3 position23 = base.transform.position;
			float y8 = position23.y;
			Vector3 position24 = base.transform.position;
			vector6 = new Vector3(x8, y8, position24.z);
		}
		RaycastHit2D raycastHit2D3;
		if (!inverted)
		{
			Vector3 localScale = base.transform.localScale;
			if (localScale.x == 1f)
			{
				raycastHit2D3 = (midDive ? Physics2D.Raycast(vector3, base.transform.right, 0.3f, whatIsSolidGround) : Physics2D.Raycast(vector3, base.transform.right, 0.25f, whatIsGround));
				if (vector4 != Vector3.zero)
				{
					RaycastHit2D ray = midDive ? Physics2D.Raycast(vector4, base.transform.right, 0.3f, whatIsAllGround) : Physics2D.Raycast(vector4, base.transform.right, 0.25f, whatIsGround);
					flag = checkRay(ray);
				}
			}
			else
			{
				raycastHit2D3 = (midDive ? Physics2D.Raycast(vector3, -base.transform.right, 0.3f, whatIsSolidGround) : Physics2D.Raycast(vector3, -base.transform.right, 0.25f, whatIsGround));
				if (vector4 != Vector3.zero)
				{
					RaycastHit2D ray = midDive ? Physics2D.Raycast(vector4, -base.transform.right, 0.3f, whatIsAllGround) : Physics2D.Raycast(vector4, -base.transform.right, 0.25f, whatIsGround);
					flag = checkRay(ray);
				}
			}
		}
		else
		{
			Vector3 localScale2 = base.transform.localScale;
			if (localScale2.x == 1f)
			{
				raycastHit2D3 = (midDive ? Physics2D.Raycast(vector3, -base.transform.right, 0.3f, whatIsSolidGround) : Physics2D.Raycast(vector3, -base.transform.right, 0.25f, whatIsGround));
				if (vector4 != Vector3.zero)
				{
					RaycastHit2D ray = midDive ? Physics2D.Raycast(vector4, -base.transform.right, 0.3f, whatIsAllGround) : Physics2D.Raycast(vector4, -base.transform.right, 0.25f, whatIsGround);
					flag = checkRay(ray);
				}
			}
			else
			{
				raycastHit2D3 = (midDive ? Physics2D.Raycast(vector3, base.transform.right, 0.3f, whatIsSolidGround) : Physics2D.Raycast(vector3, base.transform.right, 0.25f, whatIsGround));
				if (vector4 != Vector3.zero)
				{
					RaycastHit2D ray = midDive ? Physics2D.Raycast(vector4, base.transform.right, 0.3f, whatIsAllGround) : Physics2D.Raycast(vector4, base.transform.right, 0.25f, whatIsGround);
					flag = checkRay(ray);
				}
			}
		}
		Debug.DrawRay(vector, -base.transform.up, Color.blue);
		Debug.DrawRay(vector2, -base.transform.up, Color.red);
		Vector3 start = vector3;
		Vector3 right = base.transform.right;
		Vector3 localScale3 = base.transform.localScale;
		Debug.DrawRay(start, right * localScale3.x, Color.green);
		Vector3 start2 = vector4;
		Vector3 right2 = base.transform.right;
		Vector3 localScale4 = base.transform.localScale;
		Debug.DrawRay(start2, right2 * localScale4.x, Color.magenta);
		Vector2 velocity = rb.velocity;
		if (midDive && !diveNudge && !anim.GetBool("gravity"))
		{
			Vector2 origin = vector5;
			Vector3 right3 = base.transform.right;
			Vector3 localScale5 = base.transform.localScale;
			raycastHit2D = Physics2D.Raycast(origin, right3 * localScale5.x * num, 0.3f, whatIsSolidGround);
			Vector2 origin2 = vector6;
			Vector3 right4 = base.transform.right;
			Vector3 localScale6 = base.transform.localScale;
			raycastHit2D2 = Physics2D.Raycast(origin2, right4 * localScale6.x * num, 0.3f, whatIsAllGround);
			Vector3 position25 = base.transform.position;
			int num2 = 0;
			if (raycastHit2D3.collider == null && (flag || raycastHit2D2.collider != null))
			{
				Vector3 start3 = vector5;
				Vector3 right5 = base.transform.right;
				Vector3 localScale7 = base.transform.localScale;
				Debug.DrawRay(start3, right5 * localScale7.x * num, Color.red, 5f);
				num2 = 1;
			}
			else if (!flag && (raycastHit2D3.collider != null || raycastHit2D.collider != null))
			{
				Vector3 start4 = vector6;
				Vector3 right6 = base.transform.right;
				Vector3 localScale8 = base.transform.localScale;
				Debug.DrawRay(start4, right6 * localScale8.x * num, Color.blue, 5f);
				num2 = 2;
			}
			else if (flag && raycastHit2D3.collider != null)
			{
				num2 = 3;
			}
			switch (num2)
			{
			case 1:
				if (!inverted)
				{
					base.transform.position = new Vector3(position25.x, Mathf.Ceil(position25.y) + 0.02f, position25.z);
				}
				else
				{
					base.transform.position = new Vector3(position25.x, Mathf.Floor(position25.y) - 0.02f, position25.z);
				}
				diveNudge = true;
				return;
			case 2:
				if (!inverted)
				{
					base.transform.position = new Vector3(position25.x, Mathf.Floor(position25.y) + 0.3f, position25.z);
				}
				else
				{
					base.transform.position = new Vector3(position25.x, Mathf.Ceil(position25.y) - 0.3f, position25.z);
				}
				diveNudge = true;
				return;
			case 3:
				midDive = false;
				if (rb.bodyType != RigidbodyType2D.Static && !dead)
				{
					Rigidbody2D rigidbody2D = rb;
					rigidbody2D.velocity = new Vector2(0f, velocity.y);
					base.transform.GetChild(0).localEulerAngles = Vector3.zero;
				}
				return;
			}
		}
		else if (!caped && midDive && anim.GetBool("gravity"))
		{
			midDive = false;
			if (rb.bodyType != RigidbodyType2D.Static && !dead)
			{
				rb.velocity = new Vector2(0f, velocity.y);
				//base.transform.GetChild(0).localEulerAngles = Vector3.zero;
			}
		}
		if ((raycastHit2D3.collider != null || flag) && running)
		{
			if(!grounded)
			{
				transform.GetChild(0).localEulerAngles = Vector3.zero;
				if(caped)
				{
					rb.velocity = new Vector2(0f, velocity.y);
					base.transform.GetChild(0).localEulerAngles = Vector3.zero;
				}
			}
			canRun = false;
			running = false;
			if (!reachedGoal)
			{
				axis.setRange(1.2f);
			}
			else
			{
				axis.setRange(1f);
			}
			AxisSimulator axisSimulator = axis;
			Vector3 localScale9 = base.transform.localScale;
			axisSimulator.axisPosX = 1f * localScale9.x;
			if (grounded)
			{
				axis.axisAdder = walkSpeedGain;
			}
			else
			{
				axis.axisAdder = passiveGain;
			}
		}
		if (raycastHit2D3.collider != null && !frameCollision)
		{
			frameCollision = true;
			axis.axisPosX = 0f;
			if (grounded)
			{
				axis.axisAdder = walkSpeedGain;
			}
			else
			{
				axis.axisAdder = passiveGain;
			}
			Transform t=raycastHit2D3.collider.transform;
			if (!knockedBack &&!t.name.Contains("NoCol") &&t.tag != "VeloReverse" && !dead)
			{
				rb.velocity = new Vector2(0f, velocity.y);
			}
			else if (!knockedBack && !dead &&!t.name.Contains("NoCol"))
			{
				Transform transform = base.transform;
				Vector3 localScale10 = base.transform.localScale;
				transform.localScale = new Vector3(0f - localScale10.x, 1f, 1f);
				Vector3 localScale11 = base.transform.localScale;
				float x9 = 12f * localScale11.x;
				rb.velocity = new Vector2(x9, velocity.y);
			}
		}
		else if (raycastHit2D3.collider == null)
		{
			if (frameCollision)
			{
				frameCollision = false;
			}
			if (!canRun)
			{
				canRun = true;
			}
		}
		float yVelo = Mathf.Round(velocity.y * 100f) / 100f;
		if (raycastHit2D.collider != null && !grounded)
		{
			if (yVelo <= 0f && Time.timeScale != 0f && !midJump && pCol.enabled && raycastHit2D.collider.transform.tag != "Spring" && !inverted)
			{
				goto IL_1041;
			}
		}
		if (raycastHit2D2.collider != null && !grounded)
		{
			if (yVelo <= 0f && Time.timeScale != 0f && !midJump && pCol.enabled && raycastHit2D2.collider.transform.tag != "Spring" && !inverted)
			{
				goto IL_1041;
			}
		}
		if (raycastHit2D.collider != null && !grounded)
		{
			if (yVelo  >= 0f && Time.timeScale != 0f && !midJump && pCol.enabled && raycastHit2D.collider.transform.tag != "Spring" && inverted)
			{
				goto IL_1041;
			}
		}
		if (raycastHit2D2.collider != null && !grounded)
		{
			if (yVelo  >= 0f && Time.timeScale != 0f && !midJump && pCol.enabled && raycastHit2D2.collider.transform.tag != "Spring" && inverted)
			{
				goto IL_1041;
			}
		}
		if (!(raycastHit2D.collider == null) || !grounded || touchingGround || stickingToGround)
		{
			if (raycastHit2D.collider == null)
			{
				if (yVelo  < -3.3f && grounded && !stickingToGround && !inverted)
				{
					goto IL_1304;
				}
			}
			if (raycastHit2D.collider == null)
			{
				if (yVelo > 3.3f && grounded && !stickingToGround && inverted)
				{
					goto IL_1304;
				}
			}
			if (!(raycastHit2D2.collider == null) || !grounded || touchingGround || stickingToGround)
			{
				if (raycastHit2D2.collider == null)
				{
					if (yVelo < -3.3f && grounded && !stickingToGround && !inverted)
					{
						goto IL_1304;
					}
				}
				if (raycastHit2D2.collider == null)
				{
					if (yVelo > 3.3f && grounded && !stickingToGround && inverted)
					{
						goto IL_1304;
					}
				}
				goto IL_1434;
			}
		}
		goto IL_1304;
		IL_1041:
		if (stickToGround != null)
		{
			StopCoroutine(stickToGround);
		}
		stickToGround = StartCoroutine(stickGround());
		if (!dead)
		{
			Rigidbody2D rigidbody2D5 = rb;
			Vector2 velocity13 = rb.velocity;
			rigidbody2D5.velocity = new Vector2(velocity13.x, 0f);
		}
		reTrampoline = false;
		grounded = true;
		diveCooldown = 0;
		if (!running && !crouching && axis.axisAdder != walkSpeedGain)
		{
			axis.axisAdder = walkSpeedGain;
		}
		capeDive = false;
		data.stompStreak = 0;
		if (!inverted)
		{
			Vector3 position26 = base.transform.position;
			platHeight = position26.y - 0.3f;
		}
		else
		{
			Vector3 position27 = base.transform.position;
			platHeight = position27.y + 0.3f;
		}
		currentJumpPoint = 0f;
		goto IL_1434;
		IL_1434:
		anim.SetBool("Grounded", grounded);
		int ray2 = 1;
		int ray3 = 1;
		if (raycastHit2D.collider != null)
		{
			ray3 = ((!inverted) ? ((!(raycastHit2D.collider.transform.name == "SemiSolidMap")) ? data.testForBlock(raycastHit2D.point, this, data.map) : data.testForBlock(raycastHit2D.point, this, data.ssmap)) : ((!(raycastHit2D.collider.transform.name == "SemiSolidMap")) ? data.testForBlock(raycastHit2D.point + Vector2.up, this, data.map) : data.testForBlock(raycastHit2D.point + Vector2.up, this, data.ssmap)));
		}
		if (raycastHit2D2.collider != null)
		{
			ray2 = ((!inverted) ? ((!(raycastHit2D2.collider.transform.name == "SemiSolidMap")) ? data.testForBlock(raycastHit2D2.point, this, data.map) : data.testForBlock(raycastHit2D2.point, this, data.ssmap)) : ((!(raycastHit2D2.collider.transform.name == "SemiSolidMap")) ? data.testForBlock(raycastHit2D2.point + Vector2.up, this, data.map) : data.testForBlock(raycastHit2D2.point + Vector2.up, this, data.ssmap)));
		}
		determineEffect(ray3, ray2);
		return;
		IL_1304:
		Vector2 velocity14 = rb.velocity;
		if (velocity14.y < 0f && !inverted)
		{
			Rigidbody2D rigidbody2D6 = rb;
			Vector2 velocity15 = rb.velocity;
			rigidbody2D6.velocity = new Vector2(velocity15.x, 0f);
		}
		else
		{
			Vector2 velocity16 = rb.velocity;
			if (velocity16.y > 0f && inverted)
			{
				Rigidbody2D rigidbody2D7 = rb;
				Vector2 velocity17 = rb.velocity;
				rigidbody2D7.velocity = new Vector2(velocity17.x, 0f);
			}
		}
		axis.currentDivider = axis.savedNormalDivider;
		grounded = false;
		if (dead)
		{
			if (rb.bodyType == RigidbodyType2D.Dynamic)
			{
				rb.angularVelocity = 0f;
			}
			if (!inverted)
			{
				base.transform.rotation = Quaternion.identity;
			}
			else
			{
				base.transform.eulerAngles = new Vector3(0f, 0f, 180f);
			}
		}
		goto IL_1434;
	}

	private IEnumerator stickGround()
	{
		stickingToGround = true;
		yield return new WaitForSeconds(0.08f);
		stickingToGround = false;
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		string tag = other.gameObject.tag;
		switch (tag)
		{
		case "semiSolid":
			if (ignoreSemiSolid || !semiSolidChecker.insideSemiSolid)
			{
				goto default;
			}
			goto case "Spring";
		default:
			switch (tag)
			{
			case "BigBlock":
			case "Puzzle":
			case "shopBlock":
				break;
			default:
				goto end_IL_0017;
			}
			goto case "Spring";
		case "Spring":
		case "NPC":
		case "Ground":
		case "Harm":
		case "blockHoldable":
			{
				touchingGround = true;
				break;
			}
			end_IL_0017:
			break;
		}
		if (tag == "Harm" && Time.timeScale != 0f && invFrames == 0 && !dead && !inCutscene)
		{
			Damage(ignoreStomp: true, ignoreFrames: false);
		}
		if (!inverted)
		{
			if (grounded || !(other.gameObject.name != "GameObject") || !canHit)
			{
				return;
			}
			RaycastHit2D raycastHit2D = (pSprites.state != 0 && !crouching) ? Physics2D.Raycast(base.transform.position, base.transform.up + new Vector3(0f, 0.1f, 0f), 1.5f, whatIsHittable) : Physics2D.Raycast(base.transform.position, base.transform.up + new Vector3(0f, 0.1f, 0f), 1f, whatIsHittable);
			if (!(raycastHit2D.collider != null))
			{
				return;
			}
			Vector2 velocity = rb.velocity;
			if (!(velocity.y > -3f))
			{
				return;
			}
			tag = raycastHit2D.collider.gameObject.tag;
			canHit = false;
			if (tag != "NPC" && tag != "BigBlock" && tag != "Puzzle" && tag != "shopBlock")
			{
				//print(other.gameObject.name);
				if (data.blockHit(raycastHit2D.point, base.transform.position, base.gameObject.name, inverted) && (pSprites.state == 0 || (pSprites.state != 0 && !crouching)))
				{
					Rigidbody2D rigidbody2D = rb;
					Vector2 velocity2 = rb.velocity;
					rigidbody2D.velocity = new Vector2(velocity2.x, 0f);
					base.transform.GetChild(0).localEulerAngles = Vector3.zero;
				}
				return;
			}
			Vector3 position = base.transform.position;
			float num = position.y + 0.3f;
			Vector3 position2 = raycastHit2D.collider.transform.position;
			if (!(num < position2.y))
			{
				return;
			}
			if (tag != "shopBlock" && raycastHit2D.collider.gameObject.GetComponent<NPCScript>() != null)
			{
				raycastHit2D.collider.gameObject.GetComponent<NPCScript>().infoBlockHit();
				return;
			}
			switch (tag)
			{
			case "BigBlock":
			{
				Rigidbody2D rigidbody2D4 = rb;
				Vector2 velocity5 = rb.velocity;
				rigidbody2D4.velocity = new Vector2(velocity5.x, 0f);
				data.bigBlockHit(raycastHit2D.collider.transform.parent.gameObject.name, raycastHit2D.collider.transform.parent.GetComponent<bigBlockScript>());
				break;
			}
			case "Puzzle":
			{
				Rigidbody2D rigidbody2D3 = rb;
				Vector2 velocity4 = rb.velocity;
				rigidbody2D3.velocity = new Vector2(velocity4.x, 0f);
				raycastHit2D.collider.gameObject.GetComponent<puzzleBlockScript>().blockHit(inverted: false);
				break;
			}
			case "shopBlock":
			{
				Rigidbody2D rigidbody2D2 = rb;
				Vector2 velocity3 = rb.velocity;
				rigidbody2D2.velocity = new Vector2(velocity3.x, 0f);
				if(other.gameObject.name.Contains("itemShop"))
				{
					raycastHit2D.collider.gameObject.GetComponent<itemShop>().shopBlockHit();
					return;
				}	
				levelSkip component = raycastHit2D.collider.gameObject.GetComponent<levelSkip>();
				if (component != null)
				{
					component.shopBlockHit();
				}
				else
				{
					raycastHit2D.collider.gameObject.GetComponent<shopBlock>().shopBlockHit();
				}
				break;
			}
			}
		}
		else
		{
			if (grounded || !(other.gameObject.name != "GameObject") || !canHit)
			{
				return;
			}
			RaycastHit2D raycastHit2D2 = (pSprites.state != 0 && !crouching) ? Physics2D.Raycast(base.transform.position, base.transform.up - new Vector3(0f, 0.1f, 0f), 1.5f, whatIsHittable) : Physics2D.Raycast(base.transform.position, base.transform.up - new Vector3(0f, 0.1f, 0f), 1f, whatIsHittable);
			if (!(raycastHit2D2.collider != null))
			{
				return;
			}
			Vector2 velocity6 = rb.velocity;
			if (!(velocity6.y < 3f))
			{
				return;
			}
			tag = raycastHit2D2.collider.gameObject.tag;
			canHit = false;
			if (tag != "NPC" && tag != "BigBlock" && tag != "Puzzle")
			{
				if (data.blockHit(raycastHit2D2.point - Vector2.up, base.transform.position, base.gameObject.name, inverted) && (pSprites.state == 0 || (pSprites.state != 0 && !crouching)))
				{
					Rigidbody2D rigidbody2D5 = rb;
					Vector2 velocity7 = rb.velocity;
					rigidbody2D5.velocity = new Vector2(velocity7.x, 0f);
					base.transform.GetChild(0).localEulerAngles = Vector3.zero;
				}
			}
			else if (raycastHit2D2.collider.gameObject.GetComponent<NPCScript>() != null)
			{
				raycastHit2D2.collider.gameObject.GetComponent<NPCScript>().infoBlockHit();
			}
			else if (tag == "BigBlock")
			{
				Rigidbody2D rigidbody2D6 = rb;
				Vector2 velocity8 = rb.velocity;
				rigidbody2D6.velocity = new Vector2(velocity8.x, 0f);
				data.bigBlockHit(raycastHit2D2.collider.transform.parent.gameObject.name, raycastHit2D2.collider.transform.parent.GetComponent<bigBlockScript>());
			}
			else if (tag == "Puzzle")
			{
				Rigidbody2D rigidbody2D7 = rb;
				Vector2 velocity9 = rb.velocity;
				rigidbody2D7.velocity = new Vector2(velocity9.x, 0f);
				raycastHit2D2.collider.gameObject.GetComponent<puzzleBlockScript>().blockHit(inverted: true);
			}
		}
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		switch (other.gameObject.tag)
		{
		case "Spring":
		case "NPC":
		case "Ground":
		case "Harm":
		case "semiSolid":
		case "blockHoldable":
		case "BigBlock":
		case "Puzzle":
			touchingGround = false;
			break;
		}
	}

	public void flipHeldObject(bool invertObj)
	{
		SpriteRenderer component = heldObject.GetChild(0).GetComponent<SpriteRenderer>();
		Gravity component2 = heldObject.GetComponent<Gravity>();
		component.flipX = invertObj;
		Transform transform = heldObject;
		transform.eulerAngles = base.transform.eulerAngles;
		Vector3 localPosition = heldObject.localPosition;
		if (!invertObj)
		{
			float x = Mathf.Abs(localPosition.x);
			transform.localPosition = new Vector3(x*(render.flipX?1:-1), localPosition.y, 0f);
			Vector2 pushForces = component2.pushForces;
			component2.savedPushForces = new Vector2(pushForces.x, -Mathf.Abs(pushForces.y));
		}
		else
		{
			float x2 = 0f - Mathf.Abs(localPosition.x);
			transform.localPosition = new Vector3(x2*(render.flipX?-1:1), localPosition.y, 0f);
			Vector2 pushForces = component2.pushForces;
			component2.savedPushForces = new Vector2(pushForces.x, Mathf.Abs(pushForces.y));
		}
		component2.pushForces = component2.savedPushForces;
		//print(heldObject.name+" gravity: "+component2.pushForces+" To invert: "+invertObj);
	}

	public void flipSprites(bool flip)
	{
		render.flipX = flip;
		HalvaOverlay.GetComponent<SpriteRenderer>().flipX = flip;
		capeRender.flipX = flip;
		cam.flipVertOffset(flip);
		if (flip)
		{
			Vector3 position = base.transform.position;
			platHeight = position.y - 0.3f;
		}
		else
		{
			Vector3 position2 = base.transform.position;
			platHeight = position2.y + 0.3f;
		}
	}

	private IEnumerator starBoolDisable()
	{
		yield return new WaitUntil(() => Time.timeScale != 0f && (controllable||reachedGoal||dead));
		anim.SetBool("invincible", value: false);
	}

	public void growMethod(Transform par, bool playSound)
	{
		if (pSprites.state != 0)
		{
			return;
		}
		if (!dead)
		{
			if (playSound)
			{
				playPowerSound(data.sounds[mode!=1?4:109]);
			}
			Time.timeScale = 0f;
			render.enabled = true;
			anim.speed = 1f;
			anim.updateMode = AnimatorUpdateMode.UnscaledTime;
			pSprites.state = 1;
			anim.SetBool("Grow", value: true);
			pathPoints[1] = new Vector2(0f, colliderHeight.y);
			pCol.SetPath(0, pathPoints);
			if (heldObject != null)
			{
				int num = 1;
				if (inverted)
				{
					num = -1;
				}
				if (heldObject.localPosition == new Vector3(0.318f * (float)num, 0.065f, 0f))
				{
					heldObject.localPosition = new Vector3(0.318f * (float)num, 0.26f, 0f);
				}
				else if (heldObject.localPosition == new Vector3(0.5f * (float)num, 0.6f, 0f))
				{
					heldObject.localPosition = new Vector3(0.4f * (float)num, 0.8f, 0f);
				}
			}
			forceCrouch(false);
		}
		if (par != null)
		{
			UnityEngine.Object.Destroy(par.gameObject);
		}
	}
	IEnumerator delayPowerup(int state,Transform par)
	{
		yield return 0;
		yield return new WaitUntil(()=>Time.timeScale!=0);
		if (pSprites.state == 0 && !eternal)
		{
			powerUpWaitFrames = 2;
			growMethod(null, playSound: false);
		}
		powerUpMethod(state,pSprites.state,par);
	}
	void playPowerSound(AudioClip clip)
	{
		aSource.clip = clip;
		if(aSource.isPlaying)aSource.Stop();
		aSource.Play();
	}
	private void powerUpMethod(int state, int lastState, Transform par)
	{
		midSpin = false;
		if (dead)
		{
			return;
		}
		if(invFrames==120&&blinkingDuringInv) //took damage
		{
			StartCoroutine(delayPowerup(state,par));
			return;
		}
		if (Time.timeScale != 0f)
		{
			Time.timeScale = 0f;
		}
		render.enabled = true;
		if ((state == 1 && !caped) || (state == 1 && caped && lastState == 4) || state == 0)
		{
			playPowerSound(data.sounds[mode!=1?3:110]);
		}
		else
		{
			playPowerSound(data.sounds[mode!=1?4:109]);
		}
		powerAnimStates = new Vector2Int(lastState, state);
		if (state == 4)
		{
			caped = true;
		}
		else
		{
			caped = false;
		}
		if (anim.GetBool("Shrink"))
		{
			powerFrames = 30;
		}
		else
		{
			powerFrames = 45;
		}
		anim.SetBool("shoot", value: false);
		if (par != null)
		{
			UnityEngine.Object.Destroy(par.gameObject);
		}
		SetStateProperties(state);
	}

	private void eternalPowerUpMethod(int state, int lastState, Transform par)
	{
		if (!eternal)
		{
			int itemID = 0;
			switch (state)
			{
			case 3:
				itemID = 1;
				break;
			case 6:
				itemID = 2;
				break;
			}
			data.healthBarEnable(itemID, withAnimation: true);
			eternal = true;
			pSprites.eternal = true;
			if (Time.timeScale != 0f)
			{
				Time.timeScale = 0f;
			}
			render.enabled = true;
			if ((state == 1 && !caped) || (state == 1 && caped && lastState == 4))
			{
				playPowerSound(data.sounds[mode!=1?3:110]);
			}
			else
			{
				aSource.PlayOneShot(data.sounds[41]);
			}
			if (lastState == state || state == 6)
			{
				lastState = 1;
			}
			if (state == 4 || state == 6)
			{
				caped = true;
			}
			else
			{
				caped = false;
			}
			if (state == 6)
			{
				state = 3;
			}
			powerAnimStates = new Vector2Int(lastState, state);
			powerFrames = 70;
		}
		midSpin = false;
		anim.SetBool("shoot", value: false);
		if (par != null)
		{
			UnityEngine.Object.Destroy(par.gameObject);
		}
		SetStateProperties(state);
	}

	public void SetStateProperties(int state)
	{
		switch (state)
		{
		default:
			canAttack = false;
			break;
		case 2:
		case 3:
		case 5:
		case 6:
			canAttack = true;
			break;
		}
	}

	private void starMethod()
	{
		bool flag = false;
		if (!blinkingDuringInv)
		{
			flag = true;
		}
		blinkingDuringInv = false;
		render.enabled = true;
		HalvaOverlay.gameObject.SetActive(value: true);
		playPowerSound(data.sounds[mode!=1?4:109]);
		if (!flag)
		{
			anim.SetBool("invincible", value: true);
			Time.timeScale = 0f;
			anim.speed = 1f;
			StartCoroutine(starBoolDisable());
			int num = UnityEngine.Random.Range(0, 100);
			data.halvaMusic = true;
			if(!data.reachedGoal)
			{
				if (!data.switchMusic)
				{
					if (num != 0)
					{
						if(data.mode!=1)
						{
							if (UnityEngine.Random.Range(0, 6) == 0)
							{
								data.changeMusic(isDefault: false, 9, isLooping: true, normalPitch: false,0.35f);
							}
							else
							{
								data.changeMusic(isDefault: false, 103, isLooping: true, normalPitch: false,0.35f);
							}
						}
						else data.changeMusic(isDefault: false, 120, isLooping: true, normalPitch: false,0.7f);
					}
					else
					{
						data.changeMusic(isDefault: false, 32, isLooping: true, normalPitch: false,0.35f);
					}
				}
				else
				{
					data.changeMusic(isDefault: false, 47, isLooping: true, normalPitch: false,0.35f);
				}
			}
		}
		invFrames = 780;
	}

	private void playStompSound(int streak)
	{
		AudioClip clip;
		switch (streak)
		{
		default:
			clip = stomp[0];
			break;
		case 1:
			clip = stomp[1];
			break;
		case 2:
			clip = stomp[2];
			break;
		case 3:
			clip = stomp[3];
			break;
		case 4:
			clip = stomp[4];
			break;
		case 5:
			clip = stomp[5];
			break;
		case 6:
			clip = stomp[6];
			break;
		}
		aSource.PlayOneShot(clip);
	}

	private IEnumerator goalAnimation(bool kick)
	{
		axis.setRange(1f);
		PlayerScript playerScript = this;
		playerScript.whatIsGround = ((int)playerScript.whatIsGround ^ (1 << LayerMask.NameToLayer("PlayerBlock")));
		PlayerScript playerScript2 = this;
		playerScript2.whatIsSolidGround = ((int)playerScript2.whatIsSolidGround ^ (1 << LayerMask.NameToLayer("PlayerBlock")));
		Physics2D.IgnoreLayerCollision(11, 27, ignore: true);
		crouching = false;
		anim.SetBool("crouch", crouching);
		bool goal = false;
		data.reachedGoal = true;
		cam.nukeLength = 3f;
		cam.nukeEvent = true;
		if (data.litSMeterArrows == 8)
		{
			goal = true;
		}
		if (kick)
		{
			StartCoroutine(kickDur(2f));
		}
		data.changeMusic(isDefault: true, 0, isLooping: false, normalPitch: true,0.35f);
		data.stopMusic(stop: true, reset: false);
		axis.acceptXInputs = false;
		data.timerGoesDown = false;
		axis.axisAdder = 0.4f;
		endLevelScreen endScreen = GameObject.Find("HUD_Canvas").transform.GetChild(12).GetComponent<endLevelScreen>();
		if (!kick)
		{
			data.addCoin(1,true);
			GameObject original = coin;
			Vector3 position = base.transform.position;
			float x = position.x;
			Vector3 position2 = base.transform.position;
			float y = position2.y + 1f;
			Vector3 position3 = base.transform.position;
			GameObject gameObject = UnityEngine.Object.Instantiate(original, new Vector3(x, y, position3.z), Quaternion.identity);
			gameObject.SetActive(value: true);
			data.playSoundStatic(0);
		}
		if (SuperInput.GetKey("Jump"))
		{
			Vector2 velocity = rb.velocity;
			if (velocity.y > 0.2f && kick && data.litSMeterArrows == 8)
			{
				yield return 0;
				cam.damping = 0.5f;
				Time.timeScale = 0f;
				cam.overWriteLockScroll = true;
				cam.workInStoppedTime = true;
				cam.AssignTarget(GameObject.Find("GoalCameraPoint").transform);
			}
		}
		yield return new WaitForSeconds(0.15f);
		Vector2 velocity2 = rb.velocity;
		if (!(velocity2.y <= 0f) && kick)
		{
			Vector2 velocity3 = rb.velocity;
			if (!(velocity3.y > 0f) || data.litSMeterArrows >= 8)
			{
				goto IL_042a;
			}
		}
		cam.damping = 0.5f;
		cam.overWriteLockScroll = true;
		cam.AssignTarget(GameObject.Find("GoalCameraPoint").transform);
		goto IL_042a;
		IL_042a:
		yield return new WaitUntil(() => Time.timeScale != 0f);
		cam.workInStoppedTime = false;
		yield return new WaitForSeconds(4f);
		axis.acceptFakeInputs = true;
		dataShare dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
		if (dataS.lastLoadedLevel != 41)
		{
			data.changeMusic(isDefault: false, 8, isLooping: false, normalPitch: true,0.35f);
		}
		else
		{
			data.changeMusic(isDefault: false, 117, isLooping: false, normalPitch: true,0.35f);
		}
		axis.artificialX = 1f;
		yield return new WaitForSeconds(3f);
		endScreen.gameObject.SetActive(value: true);
		if (kick && goal)
		{
			yield return new WaitForSeconds(3f);
			axis.artificialX = 0f;
			yield return new WaitForSeconds(0.5f);
			axis.artificialX = -1f;
			yield return new WaitForSeconds(0.5f);
			axis.artificialX = 0f;
			yield return new WaitForSeconds(0.5f);
			anim.SetTrigger("goal");
			yield return new WaitForSeconds(1f);
			data.timeToScore = true;
			endScreen.copyScore = true;
		}
		else
		{
			yield return new WaitForSeconds(2.5f);
			yield return new WaitForSeconds(3f);
			data.timeToScore = true;
			endScreen.copyScore = true;
		}
		StartCoroutine(endScreen.endSequence());
		goalAnimFrames = 180;
		yield return new WaitUntil(() => data.timer == 0 && !endScreen.coinsToGlobal && !endScreen.sausagesToGlobal);
		endScreen.timeDisplayMode = 2;
		endScreen.copyScore = false;
		yield return new WaitUntil(() => goalAnimFrames == 0);
		cam.fadeScreen(fadeIn: true);
		yield return new WaitUntil(() => cam.fadeAnim >= 1f);
		if (data.currentLevelProgress != string.Empty)
		{
			bool newClear = false;
			char c = data.currentLevelProgress[0];
			if (c == 'N')
			{
				newClear = true;
			}
			if(c!='D')
			{
				data.currentLevelProgress = data.currentLevelProgress.Remove(0, 1);
				if(data.cheated||dataS.difficulty!=2)
				data.currentLevelProgress = data.currentLevelProgress.Insert(0,"F");
				else data.currentLevelProgress = data.currentLevelProgress.Insert(0,"D");
			}
			
			data.saveLevelProgress(newClear, showClearedAnim: true);
			yield return new WaitUntil(() => data.finishedSaving);
		}
		dataS.coins = Mathf.Clamp(dataS.coins, 0, 999);
		dataS.floppies = Mathf.Clamp(dataS.floppies, 0, 99);
		yield return 0;
		dataS.loadWorldWithLoadScreen(dataS.currentWorld);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		string tag = other.tag;
		string name = other.name;
		if (name[0] != 'Q' && name.Contains("BlockParent") && grounded)
		{
			if (!inverted)
			{
				Rigidbody2D rigidbody2D = rb;
				Vector2 velocity = rb.velocity;
				rigidbody2D.velocity = new Vector2(velocity.x, 10f);
			}
			else
			{
				Rigidbody2D rigidbody2D2 = rb;
				Vector2 velocity2 = rb.velocity;
				rigidbody2D2.velocity = new Vector2(velocity2.x, -10f);
			}
		}
		if (tag == "Hazard" && Time.timeScale != 0f && invFrames == 0 && !dead && !inCutscene && !reachedGoal)
		{
			Damage(ignoreStomp: true, ignoreFrames: false);
		}
		if (!dead)
		{
			if (name == "BallTrigger" && controllable && !reachedGoal)
			{
				reachedGoal = true;
				controllable = false;
				StartCoroutine(goalAnimation(kick: true));
			}
			if (!inCutscene)
			{
				if (powerUpWaitFrames == 0)
				{
					switch (name)
					{
					case "Cola":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData7 = data;
							Vector3 position19 = other.transform.position;
							float x7 = position19.x;
							Vector3 position20 = other.transform.position;
							float y7 = position20.y;
							Vector3 position21 = base.transform.position;
							gameData7.ScorePopUp(new Vector3(x7, y7, position21.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0 && !eternal)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: true);
							break;
						}
						if (data.storedItemID < 1)
						{
							data.storeItem(1, silent: false);
						}
						else
						{
							data.playSoundStatic(81);
						}
						UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						break;
					case "GarlicCola":
						if (reachedGoal)
						{
							break;
						}
						if (pSprites.state != 0)
						{
							if (eternal)
							{
								eternal = false;
								pSprites.eternal = eternal;
								data.healthBarDisable();
							}
							powerUpWaitFrames = 8;
							powerUpMethod(0, pSprites.state, null);
							shrinkEvent(playSound: false);
						}
						else
						{
							data.storeItem(7, silent: false);
						}
						UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						break;
					case "Pepper":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData4 = data;
							Vector3 position10 = other.transform.position;
							float x4 = position10.x;
							Vector3 position11 = other.transform.position;
							float y4 = position11.y;
							Vector3 position12 = base.transform.position;
							gameData4.ScorePopUp(new Vector3(x4, y4, position12.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0 && !eternal)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (pSprites.state != 2)
						{
							if (pSprites.state > 1 || eternal)
							{
								if (!eternal)
								{
									data.storeItem(pSprites.state, silent: false);
								}
								else
								{
									data.storeItem(2, silent: false);
								}
							}
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
							if (!eternal)
							{
								powerUpWaitFrames = 2;
								powerUpMethod(2, pSprites.state, other.transform.parent);
							}
						}
						if (pSprites.state == 2)
						{
							data.storeItem(2, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "Knife":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData3 = data;
							Vector3 position7 = other.transform.position;
							float x3 = position7.x;
							Vector3 position8 = other.transform.position;
							float y3 = position8.y;
							Vector3 position9 = base.transform.position;
							gameData3.ScorePopUp(new Vector3(x3, y3, position9.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0 && !eternal)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (pSprites.state != 5)
						{
							if (pSprites.state > 1 || eternal)
							{
								if (!eternal)
								{
									data.storeItem(pSprites.state, silent: false);
								}
								else
								{
									data.storeItem(5, silent: false);
								}
							}
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
							if (!eternal)
							{
								powerUpWaitFrames = 2;
								powerUpMethod(5, pSprites.state, other.transform.parent);
							}
						}
						if (pSprites.state == 5)
						{
							data.storeItem(5, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "LKnife":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData8 = data;
							Vector3 position22 = other.transform.position;
							float x8 = position22.x;
							Vector3 position23 = other.transform.position;
							float y8 = position23.y;
							Vector3 position24 = base.transform.position;
							gameData8.ScorePopUp(new Vector3(x8, y8, position24.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0 && !eternal)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (pSprites.state != 6)
						{
							if (pSprites.state > 1 || eternal)
							{
								if (!eternal)
								{
									data.storeItem(pSprites.state, silent: false);
								}
								else
								{
									data.storeItem(6, silent: false);
								}
							}
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
							if (!eternal)
							{
								powerUpWaitFrames = 2;
								powerUpMethod(6, pSprites.state, other.transform.parent);
							}
						}
						if (pSprites.state == 6)
						{
							data.storeItem(6, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "Burek":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData5 = data;
							Vector3 position13 = other.transform.position;
							float x5 = position13.x;
							Vector3 position14 = other.transform.position;
							float y5 = position14.y;
							Vector3 position15 = base.transform.position;
							gameData5.ScorePopUp(new Vector3(x5, y5, position15.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (pSprites.state != 4 || (pSprites.state == 4 && eternal))
						{
							if (pSprites.state > 1 || (pSprites.state != 4 && eternal))
							{
								if ((eternal && pSprites.state != 4) || (eternal && pSprites.state == 4 && data.currentHealth == 3) || !eternal)
								{
									if (!eternal)
									{
										data.storeItem(pSprites.state, silent: false);
									}
									else
									{
										data.storeItem(4, silent: false);
									}
								}
								else
								{
									data.health(3);
								}
							}
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
							if (!eternal)
							{
								powerUpWaitFrames = 2;
								powerUpMethod(4, pSprites.state, other.transform.parent);
							}
						}
						if (pSprites.state == 4 && !eternal)
						{
							data.storeItem(4, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "EterBurek":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData6 = data;
							Vector3 position16 = other.transform.position;
							float x6 = position16.x;
							Vector3 position17 = other.transform.position;
							float y6 = position17.y;
							Vector3 position18 = base.transform.position;
							gameData6.ScorePopUp(new Vector3(x6, y6, position18.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (!eternal)
						{
							powerUpWaitFrames = 2;
							eternalPowerUpMethod(4, pSprites.state, other.transform.parent);
						}
						else if (pSprites.state == 4)
						{
							data.health(3);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						else
						{
							data.storeItem(4, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "Axe":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData2 = data;
							Vector3 position4 = other.transform.position;
							float x2 = position4.x;
							Vector3 position5 = other.transform.position;
							float y2 = position5.y;
							Vector3 position6 = base.transform.position;
							gameData2.ScorePopUp(new Vector3(x2, y2, position6.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (pSprites.state != 3 || (pSprites.state == 3 && eternal))
						{
							if (pSprites.state > 1 || (pSprites.state != 3 && eternal))
							{
								if ((eternal && pSprites.state != 3) || (eternal && pSprites.state == 3 && data.currentHealth == 3) || !eternal)
								{
									if (!eternal)
									{
										data.storeItem(pSprites.state, silent: false);
									}
									else
									{
										data.storeItem(3, silent: false);
									}
								}
								else
								{
									data.health(3);
								}
							}
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
							if (!eternal)
							{
								powerUpWaitFrames = 2;
								powerUpMethod(3, pSprites.state, other.transform.parent);
							}
						}
						if (pSprites.state == 3 && !eternal)
						{
							data.storeItem(3, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "EterAxe":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData9 = data;
							Vector3 position25 = other.transform.position;
							float x9 = position25.x;
							Vector3 position26 = other.transform.position;
							float y9 = position26.y;
							Vector3 position27 = base.transform.position;
							gameData9.ScorePopUp(new Vector3(x9, y9, position27.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (!eternal)
						{
							powerUpWaitFrames = 2;
							eternalPowerUpMethod(3, pSprites.state, other.transform.parent);
						}
						else if (pSprites.state == 3)
						{
							data.health(3);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						else
						{
							data.storeItem(3, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					case "EterAxeBurek":
						if (other.transform.parent.name != "Box_item(Clone)")
						{
							data.addScore(1000L);
							GameData gameData = data;
							Vector3 position = other.transform.position;
							float x = position.x;
							Vector3 position2 = other.transform.position;
							float y = position2.y;
							Vector3 position3 = base.transform.position;
							gameData.ScorePopUp(new Vector3(x, y, position3.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						if (pSprites.state == 0)
						{
							powerUpWaitFrames = 2;
							growMethod(other.transform.parent, playSound: false);
						}
						if (!eternal)
						{
							powerUpWaitFrames = 2;
							eternalPowerUpMethod(6, pSprites.state, other.transform.parent);
						}
						else if (pSprites.state == 3)
						{
							data.health(3);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						else
						{
							data.storeItem(3, silent: false);
							UnityEngine.Object.Destroy(other.transform.parent.gameObject);
						}
						break;
					}
				}
				switch (name)
				{
				case "1up":
					if (!data.infiniteLives)
					{
						data.addLives(1);
						if (mode != 1)
						{
							GameData gameData15 = data;
							Vector3 position43 = other.transform.position;
							float x15 = position43.x;
							Vector3 position44 = other.transform.position;
							float y15 = position44.y;
							Vector3 position45 = base.transform.position;
							gameData15.ScorePopUp(new Vector3(x15, y15, position45.z), "1up", new Color32(133, 251, 124, byte.MaxValue));
						}
						else
						{
							GameData gameData16 = data;
							Vector3 position46 = other.transform.position;
							float x16 = position46.x;
							Vector3 position47 = other.transform.position;
							float y16 = position47.y;
							Vector3 position48 = base.transform.position;
							gameData16.ScorePopUp(new Vector3(x16, y16, position48.z), "ALT1up", new Color32(133, 251, 124, byte.MaxValue));
						}
					}
					else
					{
						data.playSoundStatic(121);
						data.addCoin(20,true);
						data.addScore(8000L);
						GameData gameData17 = data;
						Vector3 position49 = other.transform.position;
						float x17 = position49.x;
						Vector3 position50 = other.transform.position;
						float y17 = position50.y;
						Vector3 position51 = base.transform.position;
						gameData17.ScorePopUp(new Vector3(x17, y17, position51.z), "+8000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
					UnityEngine.Object.Destroy(other.transform.parent.gameObject);
					break;
				case "Time_Clock":
				{
					other.transform.GetChild(0).GetComponent<Animator>().SetTrigger("collected");
					other.gameObject.GetComponent<coinScript>().killCoin();
					data.addScore(1000L);
					data.addTime(10);
					GameData gameData18 = data;
					Vector3 position52 = other.transform.position;
					float x18 = position52.x;
					Vector3 position53 = other.transform.position;
					float y18 = position53.y;
					Vector3 position54 = base.transform.position;
					gameData18.ScorePopUp(new Vector3(x18, y18, position54.z), "+10", new Color32(248, 206, 63, byte.MaxValue));
					break;
				}
				case "Halva":
				{
					data.addScore(1000L);
					GameData gameData14 = data;
					Vector3 position40 = other.transform.position;
					float x14 = position40.x;
					Vector3 position41 = other.transform.position;
					float y14 = position41.y;
					Vector3 position42 = base.transform.position;
					gameData14.ScorePopUp(new Vector3(x14, y14, position42.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					starMethod();
					UnityEngine.Object.Destroy(other.transform.parent.gameObject);
					break;
				}
				case "Coin":
				{
					other.transform.GetChild(0).GetComponent<Animator>().SetTrigger("collected");
					other.gameObject.GetComponent<coinScript>().killCoin();
					aSource.PlayOneShot(data.sounds[0]);
					data.addCoin(1,true);
					GameData gameData19 = data;
					Vector3 position55 = other.transform.position;
					float x19 = position55.x;
					Vector3 position56 = other.transform.position;
					float y19 = position56.y;
					Vector3 position57 = base.transform.position;
					gameData19.ScorePopUp(new Vector3(x19, y19, position57.z), "+10", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					break;
				}
				default:
					if (name.Contains("Key_Item"))
					{
						other.transform.GetChild(0).GetComponent<Animator>().SetTrigger("collected");
						other.gameObject.GetComponent<coinScript>().killCoin();
						aSource.PlayOneShot(data.sounds[49]);
						data.updateKeys(other.transform.GetChild(0).GetComponent<keyScript>().ID, playunlockSound: true);
						data.addScore(1000L);
						GameData gameData10 = data;
						Vector3 position28 = other.transform.position;
						float x10 = position28.x;
						Vector3 position29 = other.transform.position;
						float y10 = position29.y;
						Vector3 position30 = base.transform.position;
						gameData10.ScorePopUp(new Vector3(x10, y10, position30.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
					else if (name.Contains("Floppy") && other.gameObject.GetComponent<coinScript>() != null && !other.gameObject.GetComponent<coinScript>().collected)
					{
						other.transform.GetChild(0).GetComponent<Animator>().SetTrigger("collected");
						other.gameObject.GetComponent<coinScript>().killCoin();
						if (!name.Contains("Ghost"))
						{
							data.addFloppy(1, giveScore: true);
						}
						else
						{
							data.addScore(2000L);
							data.addCoin(30,true);
							data.playSound(79, other.transform.position);
							GameData gameData11 = data;
							Vector3 position31 = other.transform.position;
							float x11 = position31.x;
							Vector3 position32 = other.transform.position;
							float y11 = position32.y;
							Vector3 position33 = base.transform.position;
							gameData11.ScorePopUp(new Vector3(x11, y11, position33.z), "+2000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
						data.playSoundStatic(34);
					}
					else if (name.Contains("Sausage"))
					{
						other.transform.GetChild(0).GetComponent<Animator>().SetTrigger("collected");
						other.gameObject.GetComponent<coinScript>().killCoin();
						data.playSound(6, other.transform.position);
						data.addCoin(10,true);
						if (!name.Contains("Ghost"))
						{
							data.addScore(7900L);
							GameData gameData12 = data;
							Vector3 position34 = other.transform.position;
							float x12 = position34.x;
							Vector3 position35 = other.transform.position;
							float y12 = position35.y;
							Vector3 position36 = base.transform.position;
							gameData12.ScorePopUp(new Vector3(x12, y12, position36.z), "+8000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
							data.collectSausage(other.gameObject);
						}
						else
						{
							data.addScore(900L);
							GameData gameData13 = data;
							Vector3 position37 = other.transform.position;
							float x13 = position37.x;
							Vector3 position38 = other.transform.position;
							float y13 = position38.y;
							Vector3 position39 = base.transform.position;
							gameData13.ScorePopUp(new Vector3(x13, y13, position39.z), "+1000", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						}
					}
					break;
				}
			}
			if ((tag == "Enemy" || tag == "Friendly") && !dead && !inCutscene && !knockedBack && !reachedGoal)
			{
				if (!inverted)
				{
					Vector2 velocity3 = rb.velocity;
					if (velocity3.y < -0.1f)
					{
						Vector3 position58 = base.transform.position;
						float y20 = position58.y;
						Vector3 position59 = other.transform.position;
						if (y20 > position59.y && blinkingDuringInv)
						{
							stompEnemy(other.gameObject, isEnemy: true);
							goto IL_1a99;
						}
					}
					if (Time.timeScale != 0f && invFrames == 0 && tag != "Friendly")
					{
						Damage(ignoreStomp: false, ignoreFrames: false);
					}
				}
				else
				{
					Vector2 velocity4 = rb.velocity;
					if (velocity4.y > 0.1f)
					{
						Vector3 position60 = base.transform.position;
						float num = position60.y - 1f;
						Vector3 position61 = other.transform.position;
						if (num < position61.y && blinkingDuringInv)
						{
							stompEnemy(other.gameObject, isEnemy: true);
							goto IL_1a99;
						}
					}
					if (Time.timeScale != 0f && invFrames == 0 && stompFrames == 0)
					{
						Damage(ignoreStomp: false, ignoreFrames: false);
					}
				}
			}
			goto IL_1a99;
		}
		goto IL_1f93;
		IL_1f93:
		if (name == "InstantDeath" && Time.timeScale != 0f)
		{
			if (Time.timeScale == 0f && dead)
			{
				Time.timeScale = 1f;
			}
			pauseMenu.pauseLock = true;
			pauseMenu.enabled = false;
			if (inverted)
			{
				flipSprites(flip: false);
				if (pSprites.state != 0)
				{
					base.transform.position -= new Vector3(0f, 1.5f, 0f);
				}
				else
				{
					base.transform.position -= new Vector3(0f, 0.75f, 0f);
				}
				base.transform.eulerAngles = Vector3.zero;
			}
			Vector3 position62 = base.transform.position;
			pCol.enabled = false;
			data.spawnCheeseSplatterPoint(base.transform.position + new Vector3(0f, 0.5f, 0f));
			if (!dead)
			{
				dead = true;
				pauseMenu.playerDead = dead;
				render.enabled = true;
				StartCoroutine(deathMelody());
				data.stopAllMusic();
				base.transform.position = position62;
				GetComponent<Gravity>().enabled = false;
				if (cape != null)
				{
					cape.GetComponent<CapeScript>().stopCapeSound();
				}
				if (sticking)
				{
					unStick(jumpOff: false);
				}
				if (lavaSortLayerName == string.Empty)
				{
					sort.sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
				}
				else
				{
					sort.sortingLayerName = lavaSortLayerName;
				}
				if (lavaSortLayerOrder == 0)
				{
					sort.sortingOrder = other.GetComponent<Renderer>().sortingOrder - 1;
				}
				else
				{
					sort.sortingOrder = lavaSortLayerOrder;
				}
				base.transform.GetChild(0).GetChild(3).GetComponent<Renderer>()
					.sortingOrder = -2;
				data.timerGoesDown = false;
				data.litSMeterArrows = 0;
				data.sMeterWorks = false;
				data.DataS.storedItem = 0;
				data.UpdateSMeterArrow();
				cam.resetVelocity(resetX: true);
				cam.autoscrollDir = Vector2.zero;
				anim.speed = 1f;
				axis.currentDivider = axis.savedNormalDivider;
				grounded = false;
				axis.currentDivider = axis.savedNormalDivider;
				rb.drag = 0f;
				stompFrames = 0;
				anim.SetBool("drowned", value: true);
				anim.SetBool("dead", dead);
				render.material = mat;
				rb.velocity = Vector2.zero;
				rb.constraints = RigidbodyConstraints2D.FreezeAll;
				if (postDeathCanvas != null)
				{
					StartCoroutine(deathFade());
				}
				else
				{
					Debug.LogError("No post death canvas found.");
				}
			}
			else
			{
				if (lavaSortLayerName == string.Empty)
				{
					sort.sortingLayerName = other.GetComponent<Renderer>().sortingLayerName;
				}
				else
				{
					sort.sortingLayerName = lavaSortLayerName;
				}
				if (lavaSortLayerOrder == 0)
				{
					sort.sortingOrder = other.GetComponent<Renderer>().sortingOrder - 1;
				}
				else
				{
					sort.sortingOrder = lavaSortLayerOrder;
				}
				base.transform.GetChild(0).GetChild(3).GetComponent<Renderer>()
					.sortingOrder = -2;
				StartCoroutine(drown(other, position62));
			}
			if (cape.activeInHierarchy)
			{
				capeRender.sortingOrder = render.sortingOrder - 1;
			}
		}
		if ((name == "deathZone" && !inverted && !reachedGoal) || (name == "deathZoneInvert" && inverted && !reachedGoal))
		{
			StartCoroutine(hitDeathZone(dead));
		}
		return;
		IL_1d13:
		if (tag == "EnemyCustomPivot" && !dead && !inCutscene && !knockedBack && !reachedGoal)
		{
			if (!inverted)
			{
				Vector2 velocity5 = rb.velocity;
				if (velocity5.y < -0.1f)
				{
					Vector3 position63 = base.transform.position;
					float y21 = position63.y;
					Vector3 position64 = other.transform.position;
					if (y21 > position64.y + other.transform.parent.GetComponent<EnemyCorpseSpawner>().stompOffset && blinkingDuringInv)
					{
						stompEnemy(other.gameObject, isEnemy: true);
						goto IL_1eb4;
					}
				}
				if (Time.timeScale != 0f && invFrames == 0)
				{
					Damage(ignoreStomp: false, ignoreFrames: false);
				}
			}
			else
			{
				Vector2 velocity6 = rb.velocity;
				if (velocity6.y > 0.1f)
				{
					Vector3 position65 = base.transform.position;
					float y22 = position65.y;
					Vector3 position66 = other.transform.position;
					if (y22 < position66.y + 0.5f && blinkingDuringInv)
					{
						stompEnemy(other.gameObject, isEnemy: true);
						goto IL_1eb4;
					}
				}
				if (Time.timeScale != 0f && invFrames == 0)
				{
					Damage(ignoreStomp: false, ignoreFrames: false);
				}
			}
		}
		goto IL_1eb4;
		IL_1eb4:
		if (((tag == "EnemyUnstompable" && !dead && !inCutscene && !knockedBack && !reachedGoal) || (tag == "Enemy_Projectile" && !dead && !inCutscene && !knockedBack && !reachedGoal)) && Time.timeScale != 0f && invFrames == 0 && (!midSpin || (other.transform.parent != null && other.transform.parent.name.ToLower().Contains("lknife"))))
		{
			Damage(ignoreStomp: false, ignoreFrames: false);
		}
		goto IL_1f93;
		IL_1a99:
		if (name.Contains("Switch"))
		{
			if (!inverted)
			{
				Vector2 velocity7 = rb.velocity;
				if (velocity7.y < -0.1f)
				{
					Vector3 position67 = base.transform.position;
					float y23 = position67.y;
					Vector3 position68 = other.transform.position;
					if (y23 > position68.y)
					{
						stompEnemy(other.gameObject, isEnemy: false);
						other.transform.parent.GetComponent<switchScript>().Activate(true);
					}
				}
			}
			else
			{
				Vector2 velocity8 = rb.velocity;
				if (velocity8.y > 0.1f)
				{
					Vector3 position69 = base.transform.position;
					float num2 = position69.y - 1f;
					Vector3 position70 = other.transform.position;
					if (num2 < position70.y)
					{
						other.GetComponent<switchScript>().Activate(true);
					}
				}
			}
		}
		if (tag == "EnemyCenterPivot" && !dead && !inCutscene && !knockedBack && !reachedGoal)
		{
			if (!inverted)
			{
				Vector2 velocity9 = rb.velocity;
				if (velocity9.y < -0.1f)
				{
					Vector3 position71 = base.transform.position;
					float y24 = position71.y;
					Vector3 position72 = other.transform.position;
					if (y24 > position72.y - 0.5f && blinkingDuringInv)
					{
						stompEnemy(other.gameObject, isEnemy: true);
						goto IL_1d13;
					}
				}
				if (Time.timeScale != 0f && invFrames == 0)
				{
					Damage(ignoreStomp: false, ignoreFrames: false);
					if (name == "bumbo")
					{
						other.transform.parent.GetComponent<bumbo_AI>().playerSteal();
					}
				}
			}
			else
			{
				Vector2 velocity10 = rb.velocity;
				if (velocity10.y > 0.1f)
				{
					Vector3 position73 = base.transform.position;
					float y25 = position73.y;
					Vector3 position74 = other.transform.position;
					if (y25 < position74.y + 0.5f && blinkingDuringInv)
					{
						stompEnemy(other.gameObject, isEnemy: true);
						goto IL_1d13;
					}
				}
				if (Time.timeScale != 0f && invFrames == 0)
				{
					Damage(ignoreStomp: false, ignoreFrames: false);
					if (name == "bumbo")
					{
						other.transform.parent.GetComponent<bumbo_AI>().playerSteal();
					}
				}
			}
		}
		goto IL_1d13;
	}

	private IEnumerator hitDeathZone(bool hitDead)
	{
		if(inCutscene&&!dead)yield break;
		bool startDead = dead;
		dead = true;
		pauseMenu.playerDead = dead;
		yield return 0;
		hitDead = startDead;
		if (Time.timeScale == 0f && dead)
		{
			Time.timeScale = 1f;
		}

		pauseMenu.pauseLock = true;
		pauseMenu.enabled = false;
		yield return 0;
		yield return new WaitUntil(() => Time.timeScale != 0f);
		render.enabled = false;
		if (!hitDead)
		{
			controllable = false;
			canAttack = false;
			Vector3 position = base.transform.position;
			StartCoroutine(deathMelody());
			data.stopAllMusic();
			base.transform.position = position;
			grav.enabled = false;
			if (cape != null)
			{
				cape.GetComponent<CapeScript>().stopCapeSound();
			}
			if (sticking)
			{
				unStick(jumpOff: false);
			}
			data.timerGoesDown = false;
			data.litSMeterArrows = 0;
			data.sMeterWorks = false;
			data.UpdateSMeterArrow();
			cam.autoscrollDir = Vector2.zero;
			data.DataS.storedItem = 0;
			anim.speed = 1f;
			grounded = false;
			axis.currentDivider = axis.savedNormalDivider;
			rb.drag = 0f;
			stompFrames = 0;
			anim.SetBool("dead", dead);
			rb.velocity = Vector2.zero;
			rb.bodyType = RigidbodyType2D.Static;
			if (postDeathCanvas != null)
			{
				StartCoroutine(deathFade());
			}
			else
			{
				Debug.LogError("No post death canvas found.");
			}
		}
		else
		{
			grav.enabled = false;
			cam.autoscrollDir = Vector2.zero;
			rb.drag = 0f;
			stompFrames = 0;
			rb.velocity = Vector2.zero;
		}
		cam.lockCamera = true;
	}

	private IEnumerator drown(Collider2D other, Vector3 pos)
	{
		anim.SetBool("drowned", value: true);
		base.transform.position = pos;
		yield return new WaitUntil(() => Time.timeScale != 0f);
		GetComponent<Gravity>().enabled = false;
		rb.velocity = Vector2.zero;
	}

	public void DeathEventTriggered()
	{
		onDeathEvent.Invoke();
	}
}
