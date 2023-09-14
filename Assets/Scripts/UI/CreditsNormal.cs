using System.Collections;
using UnityEngine;

public class CreditsNormal : MonoBehaviour
{
    Transform background,ground,player,parentPainting,bgSprite;
    RectTransform credits;
    AudioSource music;
    Animator playerAnim;
    HubCamera cam;
    public Sprite[] paintingSprites;
    float creditsLength = 0,songProgress = 0,bgProgress=0;
    public int creditsSequence = 0;
    public float floorSpeed = 1,backgroundSpeed = 1,bgSpeed = 1;
    float groundPos = 0,bgPos = 0;
    public float paintingOffset = 6;
    Vector3[] bgScribblePos = {new Vector3(-8.25f,0,10),new Vector3(-16.25f,0,10)};
    dataShare DataS;
    public GameObject cutsceneQuad;
    bool skipping = false,mini = false;
    [Header("Mini credits")]
    public AudioClip altMusic;
    public float creditsTop = 1030;
    public float creditsTopMini = 1030;
    bool canSkip = true;
    //float totalDistance = 0;
    IEnumerator waitForFade(bool withStartWait)
    {
        if(withStartWait)
        yield return new WaitForSeconds(3f);
        if(dataShare.totalCompletedLevels==35&&DataS.specialData==0)
        {
            cam.fadeScreen(true);
            yield return new WaitForSeconds(1f);
            cam.transform.position-=Vector3.up*13.5f;
            credits.parent.GetChild(2).gameObject.SetActive(true);
            credits.gameObject.SetActive(false);
            cam.setInstantFade(false);
            GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(3f);
        }
        if(skipping||!mini)
        {
            cam.fadeScreen(true);
            yield return new WaitUntil(()=>cam.fadeAnim>=1);
        }
        //Load main menu here
        if(DataS.specialData!=0)//go to gallery
        {
            DataS.loadSceneWithoutLoadScreen(54);
            yield break;
        }
        DataS.loadSceneWithLoadScreen(2);
    }
    IEnumerator intro()
    {
        if(DataS.playCutscene&&DataS.specialData==0)
        {
            Time.timeScale = 0;
            cutsceneQuad.SetActive(true);
            yield return new WaitUntil(()=>Time.timeScale!=0&&!cutsceneQuad.activeInHierarchy);
        }
        cam.setInstantFade(true);
        yield return 0;
        cam.fadeScreen(false);
        music.Play();
    }
    // Start is called before the first frame update
    void Start()
    {
        background = transform.GetChild(0);
        ground = transform.GetChild(1);
        player = transform.GetChild(2);
        playerAnim = player.GetChild(0).GetComponent<Animator>();
        music = transform.GetChild(3).GetComponent<AudioSource>();
        cam = transform.GetChild(4).GetComponent<HubCamera>();
        bgSprite = cam.transform.GetChild(0);
        creditsLength = music.clip.samples;
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        parentPainting = transform.GetChild(5);
        //print("Song length: "+creditsLength);
        //player.gameObject.SetActive(true);
        //display normal credits;
        if((dataShare.totalCompletedLevels>=35&&DataS.specialData!=2)||DataS.specialData == 3)
        {
            if(dataShare.totalCompletedLevels==35&&DataS.specialData==0)canSkip = false;
            credits = GameObject.Find("TextBoxTextNormal").GetComponent<RectTransform>();
            credits.parent.GetChild(0).gameObject.SetActive(false);
            playerAnim.SetBool("walk",true);
            groundPos = ground.position.x;
            bgPos = background.position.x;
            //create paintings
            GameObject basePainting = background.GetChild(0).gameObject;
            basePainting.transform.SetParent(parentPainting);
            //distance
            Vector3 distance = new Vector3(paintingOffset,0,0);
            for(int i = 1;i<paintingSprites.Length;i++)
            {
                GameObject obj;
                if(i==paintingSprites.Length-1)
                {
                    obj = Instantiate(basePainting,basePainting.transform.position+(distance*(i+1))+Vector3.up,Quaternion.identity);
                }
                else
                {
                    obj = Instantiate(basePainting,basePainting.transform.position+(distance*i),Quaternion.identity);
                }
                obj.GetComponent<SpriteRenderer>().sprite = paintingSprites[i];
                obj.transform.SetParent(parentPainting);
            }
            StartCoroutine(intro());
        }
        //display mini credits
        else
        {
            credits = GameObject.Find("TextBoxTextMini").GetComponent<RectTransform>();
            credits.parent.GetChild(1).gameObject.SetActive(false);
            creditsTop = creditsTopMini;
            mini = true;
            creditsLength /= 2;
            Destroy(ground.gameObject);
            Destroy(player.gameObject);
            Destroy(background.gameObject);
            Destroy(parentPainting.gameObject);
            Destroy(bgSprite.gameObject);
            TMPro.TextMeshProUGUI tm = credits.GetComponent<TMPro.TextMeshProUGUI>();
            tm.alignment = TMPro.TextAlignmentOptions.Center;
            //tm.ForceMeshUpdate();
            credits.localPosition = new Vector3(0,-980,32);
            cam.fadeScreen(false);
            music.clip = altMusic;
            music.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0)
        {
            if(canSkip&&creditsSequence<=1&&!skipping&&SuperInput.GetKeyDown("Jump"))
            {
                skipping = true;
                StartCoroutine(waitForFade(false));
            }
            if(skipping&&music.volume>0)
            {
                music.volume = Mathf.Clamp(music.volume-=Time.deltaTime*2,0,1);
            }
            if(songProgress!=1)
            songProgress = (float)music.timeSamples/(float)creditsLength;

            credits.localPosition = new Vector3(credits.localPosition.x,Mathf.Lerp(-980,creditsTop,songProgress),32);

            if(!mini)
            {
                if(creditsSequence<=1&&player!=null)
                player.position = new Vector3(Mathf.Lerp(-5.45f,13.45f,songProgress),-5.75f,0);


                //bg slideoff
                if(creditsSequence==0&&songProgress>=0.9998f)
                {
                    creditsSequence = 1;
                    playerAnim.SetBool("walk",false);
                }
                if(creditsSequence==1)
                {
                    floorSpeed=Mathf.Clamp(floorSpeed-=(Time.deltaTime*2.5f),0,30);
                    backgroundSpeed=Mathf.Clamp(backgroundSpeed-=(Time.deltaTime*2.5f),0,30);

                    bgProgress+=Time.deltaTime*bgSpeed;
                    bgSprite.localPosition = Vector3.Slerp(bgScribblePos[0],bgScribblePos[1],bgProgress);

                    if(floorSpeed==0&&backgroundSpeed==0&&bgProgress>=1)
                    {
                        creditsSequence = 2;
                        if(!skipping)
                        StartCoroutine(waitForFade(true));
                    }
                }
                ground.position = new Vector3(Mathf.Repeat(groundPos-=(Time.deltaTime*floorSpeed),6)-3,-6.25f,0);
                background.position = new Vector3(Mathf.Repeat(bgPos-=(Time.deltaTime*backgroundSpeed),24)-12,0,0);
                //totalDistance +=Time.deltaTime*backgroundSpeed; 
                parentPainting.position -= new Vector3(Time.deltaTime*backgroundSpeed,0,0);
            }
            else
            {
                if(songProgress>=1)
                {
                    if(!skipping)
                    StartCoroutine(waitForFade(true));
                    skipping = true;
                }
            }
        }
    }
}
