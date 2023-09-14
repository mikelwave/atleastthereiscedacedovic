using System.Collections;
using UnityEngine;
using TMPro;

public class CreditsTrue : MonoBehaviour
{
    Transform background;
    AudioSource music;
    HubCamera cam;
    float creditsLength = 0;
    public int creditsSequence = 0;
    public float backgroundSpeed;
    dataShare DataS;
    public GameObject cutsceneQuad;
    bool skipping = false,skippable = false;
    Transform par;
    Color transparent = new Color(1,1,1,0),opaque = Color.white;
    int waitFrames = 0;
    public AudioClip altMusic;
    void loadMenuMethod()
    {
        //print("loadingMenu");
        StartCoroutine(loadMenu());
    }
    IEnumerator loadMenu()
    {
        yield return 0;
        //Load gallery
        if(DataS.specialData>=4)
        {
            DataS.loadSceneWithoutLoadScreen(54);
            yield break;
        }
        //Load main menu here
        if(DataS.clearedUnbeatenLevel&&!PlayerPrefs.HasKey("Save"+(Mathf.Clamp(DataS.saveFileID+4,4,6))))
        UnityEngine.SceneManagement.SceneManager.LoadScene (2);
        else
        {
            DataS.loadSceneWithLoadScreen(2);
        }
    }
    IEnumerator intro()
    {
        if(DataS.playCutscene)
        {
            Time.timeScale = 0;
            cutsceneQuad.SetActive(true);
            yield return new WaitUntil(()=>Time.timeScale!=0&&!cutsceneQuad.activeInHierarchy);
        }
        cam.setInstantFade(true);
        yield return 0;
        cam.fadeScreen(false);
        music.Play();
        if(music.clip!=altMusic)
        {
            StartCoroutine(bgMovement());
            if(par.gameObject.activeInHierarchy)
            StartCoroutine(displayCredits());
        }
        else
        {
            yield return new WaitForSeconds(10);
            cam.fadeScreen(true);
            //print("Alt credits end here");
            waitFrames = 60;
            StopAllCoroutines();
        }
    }
    IEnumerator skip()
    {
        cam.fadeScreen(true);
        yield return 0;
        yield return new WaitUntil(()=>cam.fadeAnim>=1);
        //print("End skip here");
        waitFrames = 15;
        StopAllCoroutines();
    }
    IEnumerator bgMovement()
    {
        bool fading = false;
        while(background.localPosition.y>-126)
        {
            background.localPosition = new Vector3(0,Mathf.Lerp(-6,-126,music.timeSamples/creditsLength));
            yield return 0;
            if(background.localPosition.y<=-120&&!fading)
            {
                fading = true;
                cam.fadeScreen(true);
                yield return new WaitUntil(()=>cam.fadeAnim>=1);
                background.gameObject.SetActive(false);
                yield return 0;
                cam.setInstantFade(false);
                skippable = false;
                //show end text
                TextMeshProUGUI t = par.GetChild(par.childCount-1).GetComponent<TextMeshProUGUI>();
                float progress= 0;
                t.color = transparent;
                t.gameObject.SetActive(true);
                while(progress<1)
                {
                    progress+=Time.deltaTime;
                    t.color = Color.Lerp(transparent,opaque,progress);
                    yield return 0;
                }
                t.color = opaque;
                yield return new WaitForSeconds(6f);
                progress = 0;
                while(progress<1)
                {
                    progress+=Time.deltaTime;
                    t.color = Color.Lerp(opaque,transparent,progress);
                    yield return 0;
                }
                t.color = transparent;
                t.gameObject.SetActive(false);
                //print("Credits end here");
                waitFrames = 15;
                StopAllCoroutines();
            }
        }
    }
    IEnumerator displayCredits()
    {
        float progress = 0;
        float halfLength = creditsLength*0.4f,longEnd = creditsLength*0.68f,node = halfLength/5.2f;
        yield return new WaitForSeconds(2f);
        for(int i = 0;i<7;i++)
        {
            TextMeshProUGUI t = par.GetChild(i).GetComponent<TextMeshProUGUI>();
            if(i!=7)
            {
                //show credits
                //fade in
                progress= 0;
                t.color = transparent;
                t.gameObject.SetActive(true);
                while(progress<1)
                {
                    progress+=Time.deltaTime*4;
                    t.color = Color.Lerp(transparent,opaque,progress);
                    yield return 0;
                }
                t.color = opaque;
                if(i==0)
                yield return new WaitForSeconds(4f);
                else yield return new WaitForSeconds(7f);
                progress = 0;
                while(progress<1)
                {
                    progress+=Time.deltaTime*4;
                    t.color = Color.Lerp(opaque,transparent,progress);
                    yield return 0;
                }
                t.color = transparent;
                t.gameObject.SetActive(false);
                //wait until next point
                print("Current point: "+music.timeSamples+" waiting for: "+(node*(i+1)));
                if(i!=6)
                yield return new WaitUntil(()=>music.timeSamples>=node*(i+1));
                //print("show next credit");
            }
        }
        print("Long credit here");
        Transform t7 = par.GetChild(7);
        float curPoint = music.timeSamples;
        t7.gameObject.SetActive(true);
        while(music.timeSamples<=longEnd)
        {
            t7.localPosition = new Vector3(-106,Mathf.Lerp(-450,718,(music.timeSamples-curPoint)/(longEnd-curPoint)));
            yield return 0;
        }
        t7.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        par = GameObject.Find("Textbox_Canvas").transform;
        background = transform.GetChild(0);
        music = transform.GetChild(1).GetComponent<AudioSource>();
        cam = transform.GetChild(2).GetComponent<HubCamera>();
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        int s = DataS.specialData;
        if(!DataS.clearedUnbeatenLevel||s>=4)
        {
            skippable = true;
        }
        if(s>=4)DataS.playCutscene = false;
        if(s==8||s==2||s<4&&DataS.mode==1)//playuh mode
        {
            background.gameObject.SetActive(false);
            transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
            music.clip = altMusic;
            cutsceneQuad.GetComponent<cutsceneEvent>().forceAlt = true;
        }
        else if(s>=6||s==1||((s<=1||s>=6)&&DataS.levelProgress.Length>41&&DataS.levelProgress[41].Contains("F")))//check color
        {
            if(s==0)
            {
                DataS.specialData = 1;
                s = DataS.specialData;
            }
            background.gameObject.layer = 5;
            background.GetChild(0).gameObject.SetActive(true);
        }
        //toggle credits
        if(s==5||s==7)
        {
            par.gameObject.SetActive(false);
        }
        creditsLength = music.clip.samples;
        //print("Music length: "+creditsLength);
        StartCoroutine(intro());
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale!=0&&skippable&&!skipping&&SuperInput.GetKeyDown("Jump"))
        {
            skipping = true;
            StartCoroutine(skip());
            //print("Skipping credits");
        }
        if(skipping&&music.volume>0)
        {
            music.volume = Mathf.Clamp(music.volume-=Time.deltaTime*2,0,1);
        }
        if(waitFrames>0)
        {
            waitFrames--;
            if(waitFrames<=0)loadMenuMethod();
        }
    }
}
