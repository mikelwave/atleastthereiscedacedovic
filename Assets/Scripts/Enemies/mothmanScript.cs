using UnityEngine;

public class mothmanScript : MonoBehaviour
{
    public Vector3 offset = new Vector3(0,4,0);
    int spinInt = 0,waitFrames = 0;
    float targetY = -999;
    Animator anim;
    Transform cam,player;
    float t = 0.0f;
    bool canResetT = false,atPoint = false;
    int progress = 0;
    float roundedPos,roundedTarget;
    SpriteRenderer render;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        cam = GameObject.Find("Main Camera").transform;
        player = GameObject.Find("Player_main").transform;
        render = transform.GetChild(0).GetComponent<SpriteRenderer>();
        render.sortingLayerName = "Background";
        render.sortingOrder = -1;
    }
    void FixedUpdate()
    {
        if(progress==2)
        {
            targetY = cam.position.y+offset.y;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(progress == 0&&targetY==-999)
        transform.position = new Vector3(cam.position.x+offset.x,cam.position.y+offset.y,transform.position.z);
        if(Time.timeScale!=0)
        {
            if(targetY!=-999)
            {
                transform.position = new Vector3(cam.position.x+offset.x,Mathf.Lerp(transform.position.y,targetY,t),transform.position.z);
                if(progress>=1)
                t += 0.3f * Time.deltaTime;

                if(progress==2)
                {
                    targetY = cam.position.y+offset.y;
                }

                if(roundedPos==targetY&&canResetT&&progress==1)
                {
                    t = 0.0f;
                    canResetT = false;
                    progress=2;
                    //print("set to cam");
                    transform.position = new Vector3(cam.position.x+offset.x,targetY,transform.position.z);
                }
                else if(roundedPos==roundedTarget&&progress==2&&atPoint)
                {
                    //print(roundedPos+" "+roundedTarget);
                    progress=3;
                    //print("reset");
                    transform.position = new Vector3(cam.position.x+offset.x,targetY,transform.position.z);
                    targetY = -999f;
                    activate();
                }
            }
            if(spinInt==4&&waitFrames>0)
            {
                waitFrames--;
                //swoop
                if(waitFrames==0)
                {
                    Swoop();
                }
            }
            roundedPos = Mathf.Round(transform.position.y * 10f) / 10f;
            roundedTarget = Mathf.Round(targetY * 10f) / 10f;
        }
    }
    public void activate()
    {
        render.sortingLayerName = "Player";
        render.sortingOrder = 4;
        atPoint = false;
        progress = 0;
        spinInt = 0;
        anim.SetTrigger("Spin");
    }
    public void flip()
    {
        atPoint = true;
        transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
        transform.GetChild(0).localScale = transform.localScale;
        t += 0.2f;
    }
    public void addSpin()
    {
        spinInt++;
        if(spinInt==1)
        {
            waitFrames = 30;
        }
        if(spinInt==3)
        {
            anim.SetTrigger("Idle");
        }
    }
    void Swoop()
    {
        progress = 1;
        t = 0.0f;
        if(player.position.y<=cam.position.y)
        targetY = Mathf.Round((player.position.y)*10f)/10f;
        else if(player.position.y<=cam.position.y-6) targetY = Mathf.Round((cam.position.y-6)*10f)/10f;
        else targetY = Mathf.Round((cam.position.y)*10f)/10f;

        anim.SetTrigger("Swoop");
        canResetT = true;
    }
}
