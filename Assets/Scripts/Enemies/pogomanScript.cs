using UnityEngine;

public class pogomanScript : MonoBehaviour
{
    Transform player;
    public Sprite[] sprites = new Sprite[6];
    SimpleAnim2 anim;
    public AudioClip[] jumpSounds = new AudioClip[2];
    Jumper jumpScript;
    MovementAI ai;
    public float[] jumpHeights = new float[4]{10,15,17.5f,20};
    LayerMask itemLayerMask;
    bool panicMode = false;
    public bool alwaysLookAtPlayer = true;
    GameData data;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player_main").transform;
        anim = transform.GetChild(0).GetComponent<SimpleAnim2>();
        jumpScript = GetComponent<Jumper>();
        ai = GetComponent<MovementAI>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
        itemLayerMask |= (1 << LayerMask.NameToLayer("Item"));
    }
    public void reset()
    {
        //print(Time.timeSinceLevelLoad);
        if(Time.timeSinceLevelLoad>1)
        {
            //print("r");
            panicMode=false;
            panicModeSwitch();
        }
    }
    public void pogoEvent()
    {
        if(!panicMode)
        {
            if(!jumpScript.inverted)
            {
                float difference = 0;
                if(player.position.y>=transform.position.y)
                difference = Mathf.Abs(Mathf.Floor(Mathf.Abs(player.position.y)-Mathf.Abs(transform.position.y)));
                //print(difference);

                if(difference<=0){jumpScript.jump = 10; jumpScript.bounceSound=jumpSounds[0];}
                else
                {
                    jumpScript.bounceSound=jumpSounds[1];
                    if(difference==1) jumpScript.jump = 15;
                    else if(difference==2) jumpScript.jump = 17.5f;
                    else jumpScript.jump = 20f;
                }
            }
            else
            {
                float difference = 0;
                if(player.position.y<=transform.position.y)
                difference = Mathf.Abs(Mathf.Ceil(Mathf.Abs(transform.position.y)-Mathf.Abs(player.position.y)));
                //print(difference);

                if(difference<=0){jumpScript.jump = 10; jumpScript.bounceSound=jumpSounds[0];}
                else
                {
                    jumpScript.bounceSound=jumpSounds[1];
                    if(difference==1) jumpScript.jump = 15;
                    else if(difference==2) jumpScript.jump = 17.5f;
                    else jumpScript.jump = 20f;
                }
            }
        }
        RaycastHit2D ray = Physics2D.Raycast(transform.position+new Vector3(0,transform.up.y*0.2f,0),-Vector3.right*transform.localScale.x,5f,itemLayerMask);
        if(ray.collider!=null&&ray.collider.transform.name.ToLower().Contains("cola"))
        {
            Debug.DrawLine(transform.position+new Vector3(0,transform.up.y*0.2f,0),ray.point,Color.red,2f);
            if(!panicMode)
            {
                panicMode = true;
                panicModeSwitch();
            }
        }
        else if(ray.collider==null)
        {
            if(panicMode)
            {
                panicMode = false;
                panicModeSwitch();
            }
        }
        ai.speed = panicMode ? 3.5f : 0;
    }
    void panicModeSwitch()
    {
        if(panicMode)
        {
            anim.sprites[0] = sprites[4];
            anim.sprites[1] = sprites[5];
            anim.sprites[2] = sprites[5];
            anim.sprites[3] = sprites[5];
            anim.sprites[4] = sprites[4];
            anim.sprites[5] = sprites[3];
            anim.sprites[6] = sprites[3];
            ai.changeDirTowardsPlayer = false;
            ai.directionInverter = 1;
            jumpScript.jump = 15;
            jumpScript.bounceSound=jumpSounds[0];
            jumpScript.waitBetweenJumps = 5;
            ai.changeDirection(-ai.direction);
            data.playUnlistedSoundPoint(jumpSounds[2],transform.position);
        }
        else
        {
            anim.sprites[0] = sprites[1];
            anim.sprites[1] = sprites[2];
            anim.sprites[2] = sprites[2];
            anim.sprites[3] = sprites[2];
            anim.sprites[4] = sprites[1];
            anim.sprites[5] = sprites[0];
            anim.sprites[6] = sprites[0];
            ai.directionInverter = -1;
            ai.changeDirTowardsPlayer = alwaysLookAtPlayer;
            jumpScript.waitBetweenJumps = 15;
        }
    }
}
