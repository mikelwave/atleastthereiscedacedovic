using UnityEngine;

public class megaSatanHandCollisionDetect : MonoBehaviour
{
    megaSatan_Boss mBoss;
    megaSatanHandEvent mHand;
    public Vector2Int handPoints;
    float TargetPoint = 0;
    public bool moving = false,returning = false,raising = false;
    public float speedDivider = 4;
    public Vector3 startPoint;
    Rigidbody2D rb;
    Gravity grav;
    public bool canTakeDamage = false;
    bool canSmash = false;
    public int HP = 3;
    GameObject slamWave;
    float progress = 0;
    [HideInInspector]
    public trashFlyScript movementScript;
    float xTracker = 0;
    bool trackingRight = false;
    Transform handSpriteTr;
    public static float RoundDigit(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
    // Start is called before the first frame update
    void Start()
    {
        mBoss = transform.parent.parent.parent.GetComponent<megaSatan_Boss>();
        mHand = transform.parent.GetComponent<megaSatanHandEvent>();
        startPoint = transform.position;
        movementScript = GetComponent<trashFlyScript>();
        slamWave = transform.parent.GetChild(6).gameObject;
        rb = GetComponent<Rigidbody2D>();
        grav = GetComponent<Gravity>();
        handSpriteTr = transform.GetChild(0);
    }
    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        if(moving)
        {
            transform.position = Vector3.Slerp(pos, new Vector2(TargetPoint,pos.y),progress);
            progress=Mathf.Clamp(progress+=((Time.deltaTime/speedDivider)*mBoss.handsAnim.speed),0,1);
            if(progress>=1)
            {
                progress=0;
                pos = new Vector3(TargetPoint,pos.y,pos.z);
                moving = false;
            }
        }
        if(returning)
        {
            transform.position = Vector3.Slerp(pos, startPoint,progress);
            progress=Mathf.Clamp(progress+=((Time.deltaTime/speedDivider)*mBoss.handsAnim.speed),0,1);
            if(progress>=0.75f||transform.position==startPoint)
            {
                progress=0;
                pos = startPoint;
                returning = false;
                mBoss.handEvent(9);
            }
        }
        if(raising)
        {
            transform.position = Vector3.Lerp(pos, new Vector2(pos.x,TargetPoint),progress);
            progress=Mathf.Clamp(progress+=((Time.deltaTime/speedDivider)),0,1);
            if(progress>=1f||transform.position==startPoint)
            {
                progress=0;
                raising = false;
            }
        }
        if(xTracker!=0&&
        (trackingRight&&transform.position.x>=xTracker||!trackingRight&&transform.position.x<=xTracker))
        {
            startBleedSequence(trackingRight);
        }
    }
    public void startBleedSequence(bool trackingDir)
    {
        trackingRight = trackingDir;
        if(trackingRight) xTracker = Mathf.Floor(transform.position.x+4)+0.5f;
        else xTracker = Mathf.Floor(transform.position.x-4)+0.5f;
        mBoss.dripBlood(transform.position.x,handSpriteTr.position.y);
    }
    public void pickPoint()
    {
        speedDivider = 4;
        TargetPoint = Random.Range(handPoints[0],handPoints[1]+1);
        TargetPoint+=0.5f;
        moving = true;
        canSmash = true;
        mHand.setWarningPos(TargetPoint);
        progress = 0;
    }
    public void raiseHand()
    {
        speedDivider = 4;
        grav.enabled = false;
        TargetPoint = transform.position.y+1;
        canSmash = false;
        progress = 0;
        rb.velocity = Vector2.zero;
        raising = true;
    }
    public void returnToStart()
    {
        xTracker = 0;
        speedDivider = 4;
        raising = false;
        grav.enabled = false;
        rb.velocity = Vector2.zero;
        moving = false;
        returning = true;
        canSmash = false;
        progress = 0;
    }
    public void resetPos()
    {
        transform.position = startPoint;
    }
    //phase 2 attack
    public void bleedPoint(float xPoint,float setSpeedDivider)
    {
        speedDivider = setSpeedDivider;
        TargetPoint = xPoint;
        TargetPoint+=0.5f;
        moving = true;
        progress = 0;
    }
    public void setStartPoint()
    {
        startPoint = transform.position;
        //print(transform.position);
    }
    public void spawnGreed()
    {
        mBoss.StartCoroutine(mBoss.spawnEnemy(transform.position-(Vector3.up/2),0,0));
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        //print(other.transform.name);
        if(canSmash)
        {
            canSmash = false;
            float pos = Mathf.Floor(transform.position.x)+0.5f;
            mBoss.spawnSpikes(
            Mathf.CeilToInt(pos-45.5f)
            ,Mathf.CeilToInt(68.5f-pos)
            ,pos);
            slamWave.transform.position = transform.position-new Vector3(0,0.7f,0);
            slamWave.SetActive(true);
            //calculate distance between ceda and hand
            if(mBoss.pScript.grounded)
            {
                float distance = Mathf.Abs(Mathf.Abs(mBoss.pScript.transform.position.x)-Mathf.Abs(handSpriteTr.position.x)),
                power = (12-distance)/20f;
                //print(distance+" , strength: "+power);
                if(distance<12)
                {
                    float dir = 1;
                    if(mBoss.pScript.transform.position.x<handSpriteTr.position.x) dir = -1;
                    mBoss.pScript.knockbackCor = StartCoroutine(mBoss.pScript.knockBack(dir,power,0.5f,true));
                }
            }
        }

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //print(other.name);
        if(other.name=="PlayerCollider")
        {
            PlayerScript p = mBoss.pScript;
            if(other.transform.position.y>transform.position.y-0.3f&&p.rb.velocity.y<-0.1f)
            {
                //print("Stomp");
                p.stompBoss(gameObject,canTakeDamage);
                if(canTakeDamage)
                {
                    raising = false;
                    if(transform.name.Contains("left"))
                        mBoss.handsAnim.SetInteger("SideVar",0);
                    else mBoss.handsAnim.SetInteger("SideVar",1);
                    mBoss.data.playUnlistedSound(mBoss.sounds[3]);
                    HP--;
                    mBoss.decreaseHP();
                    if(HP!=0)
                    {
                        mBoss.handEvent(10);
                    }
                    else
                    {
                        //print("Kill hand");
                        mBoss.handEvent(12);
                    }
                }
                else
                {
                    mBoss.data.playSound(100,transform.position);
                    p.knockbackCor = StartCoroutine(p.knockBack(-p.transform.localScale.x,0.05f,0.1f,true));
                }
            }
            else
            {
                p.Damage(false,false);
            }
        }
    }
}
