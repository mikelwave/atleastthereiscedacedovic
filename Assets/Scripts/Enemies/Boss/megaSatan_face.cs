using UnityEngine;

public class megaSatan_face : MonoBehaviour
{
    public bool looking = true;
    public bool laserFollow = false;
    bool laserReset = false;
    public float maxLaserSpeed = 5;
    public Sprite[] faceSprites = new Sprite[8],revSprites = new Sprite[8];
    SpriteRenderer render;
    Transform rotLook;
    Transform player,main;
    LaserScript[] laser = new LaserScript[2];
    // Start is called before the first frame update
    void Start()
    {
        rotLook = transform.GetChild(0);
        main = transform.parent.parent;
        player = GameObject.Find("Player_main").transform;
        render = GetComponent<SpriteRenderer>();
        laser[0] = transform.GetChild(1).GetComponent<LaserScript>();
        laser[1] = transform.GetChild(2).GetComponent<LaserScript>();
        laser[0].gameObject.SetActive(true);
        laser[1].gameObject.SetActive(true);
    }
    void LateUpdate()
    {
            if(looking)
            {
                int value = Mathf.CeilToInt((((rotLook.eulerAngles.z))+22.5f)/45);
                if(value>8||value<1)
                {
                    if(main.localScale.x==1)
                    render.sprite = faceSprites[0];
                    else render.sprite = revSprites[0];
                }
                else
                {
                    if(main.localScale.x==1)
                    render.sprite = faceSprites[value-1];
                    else render.sprite = revSprites[value-1];
                }
            }
        if(Time.timeScale!=0)
        {
            if(laserFollow)
            {
                if(!laserReset)laserReset = true;
                float step = maxLaserSpeed*Time.deltaTime;
                laser[0].transform.up = Vector3.Lerp(laser[0].transform.up,-(player.position - laser[0].transform.position),step);
                laser[1].transform.up = Vector3.Lerp(laser[1].transform.up,-(player.position - laser[1].transform.position),step);
            }
        }
        if(laserReset&&!laserFollow)
        {
            laserReset = false;
            laser[0].transform.localEulerAngles = Vector3.zero;
            laser[1].transform.localEulerAngles = Vector3.zero;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(looking)
        {
            Vector3 difference = player.position - rotLook.position;
            float rotationZ = Mathf.Atan2(difference.y,difference.x) * Mathf.Rad2Deg;
            rotLook.rotation = Quaternion.Euler(0,0,rotationZ);
        }
    }
}
