using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GalleryScript : MonoBehaviour
{
    public bool unlockAll = false;
    TextAsset lines;
    public TextAsset[] cutsceneSubtitles;
    public string[] titles;
    public Sprite[] thumbnails;
    public VideoClip[] clips;
    public AudioClip[] sounds;
    int selection = 0,lastSelection = -1;
    Vector2 bounds = new Vector2(4,11);
    TextMeshProUGUI titleText;
    bool canMove = false;
    SpriteRenderer highlightRender,fader;
    Transform highlight,iconsHold;
    AudioSource u,musicSource,staticSource;
    public AudioClip music;
    float startVolume = 1;
    //unlocking
    bool[] videoCells = new bool[40];
    public int[] cutsceneSubtitleHandles;
    dataShare DataS;
    VideoPlayer vidplayer;
    cutsceneEvent cEvent;
    SpriteRenderer overlayRender;
    int flickerFrame = -1;
    bool flickering = false;

    void unlockVids()
    {
        int totalCompBase = 0;
        bool unlock7 = false;
        int totalCompPlayuh = 0;
        
        string playuhCells = "";
        if(!unlockAll)
        {
            string baseSave,playuhSave = "";
            int saveID = DataS.saveFileID,trueSave = saveID;
            //print("Save file ID: "+trueSave);
            //get base save
            if(saveID>=3)saveID-=3;
            else if(saveID>=6)saveID-=6;
            //print("Base save: "+saveID);
            //print("True save: "+trueSave);
            saveID+=1;

            baseSave=PlayerPrefs.GetString("Save"+saveID);

            if(PlayerPrefs.HasKey("Save"+(saveID+3).ToString()))
            {
                playuhSave = PlayerPrefs.GetString("Save"+(saveID+3));
            }
            //print("Getting string: "+("Save"+saveID)+" and for playuh: "+("Save"+(saveID+3)));
            //print("Base save data: "+baseSave);
            //print("Playuh save data: "+playuhSave);
            List <string> textAsLines = new List<string>();
            textAsLines.AddRange(baseSave.Split("\n"[0]));

            //get data from dataS first
            if(trueSave<=2)
            {
                totalCompBase = dataShare.totalCompletedLevels;
                unlock7 = DataS.worldProgression>=7 ? true : false;
            }
            if(trueSave>2&&baseSave!="")
            if(textAsLines.Count>=13)
            {
                string s = "";
                for(int i = 0;i<textAsLines[2].Length;i++)
                {
                    char c=textAsLines[2][i];
                    if(c!='/')
                    s+=c;
                    else break;
                }
                int.TryParse(s, out totalCompBase);
                //print("Base total completed levels: "+s+", as int: "+totalCompBase);
                s="";
                for(int i = 0;i<textAsLines[12].Length;i++)
                {
                    char c=textAsLines[12][i];
                    if(c!='/')
                    s+=c;
                    else break;
                }
                //print("Total world progression: "+textAsLines[12]+" string: "+s);
                int t = 0;
                int.TryParse(s, out t);
                //print("World unlock int: "+t);
                if(t>=7)unlock7 = true;
                //print("Unlocked world 7: "+unlock7);

            }
            else
            {
                Debug.LogError("Invalid base save file");
                return;
            }

            //get data from playuh save, if exists
            if(trueSave>=3&&trueSave<=5)
            {
                totalCompPlayuh = dataShare.totalCompletedLevels;
                for(int i = 1;i<14;i++)
                {
                    if(DataS.cellData[i])
                    playuhCells+='1';
                    else playuhCells+='0';
                }
            }

            if((trueSave<=2||trueSave>=6)&&playuhSave!="")
            {
                textAsLines = new List<string>();
                textAsLines.AddRange(playuhSave.Split("\n"[0]));
                if(textAsLines.Count>=3)
                {
                    string s = "";
                    for(int i = 0;i<textAsLines[2].Length;i++)
                    {
                        char c=textAsLines[2][i];
                        if(c!='/')
                        s+=c;
                        else break;
                    }
                    int.TryParse(s, out totalCompPlayuh);
                    //print("Playuh total completed levels: "+s+", as int: "+totalCompPlayuh);

                }
                else
                {
                    Debug.LogError("Invalid base save file");
                    return;
                }
                //get data cells
                for(int x = 0;x<textAsLines.Count;x++)
                {
                    if(textAsLines[x].StartsWith(";3"))
                    {
                        playuhCells = textAsLines[x+1];
                        //print("Playuh Cells: "+playuhCells);
                        playuhCells = playuhCells.Substring(1,13);
                        break;
                    }
                }
                //print("Playuh data cells: "+playuhCells);
            }
        }
        else
        {
            totalCompBase = 42;
            totalCompPlayuh = 42;
            unlock7 = true;
            playuhCells = "1111111111111";
        }
        //unlock videos
        for(int i = 0;i<videoCells.Length;i++)
        {
            switch(i)
            {
                default: break;
                case 0: videoCells[i] = true; break;
                case 1:
                if(totalCompBase>=1)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
                case 3:
                if(totalCompBase>=5)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
                case 5:
                if(totalCompBase>=11)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
                case 7:
                if(totalCompBase>=17)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
                case 9:
                if(totalCompBase>=23)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
                case 11:
                if(totalCompBase>=29)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
                case 13:
                if(totalCompBase>=35)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    videoCells[33] = true;
                    i++;
                }
                break;
                case 15:
                if(totalCompBase>=35&&unlock7)
                {
                    videoCells[i] = true;
                }
                break;
                case 16:
                if(totalCompBase>=41)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    videoCells[34] = true;
                    videoCells[35] = true;
                    i++;
                }
                break;
                //playuh mode
                case 18:
                if(totalCompPlayuh>=1)
                videoCells[i]=true;
                break;
                case 19:
                if(totalCompPlayuh>=41)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    videoCells[38] = true;
                    i++;
                }
                break;
                //unlockable videos
                case 21:
                //print(playuhCells+" Length: "+playuhCells.Length);
                float l = Mathf.Clamp(playuhCells.Length,0,13);
                for(int x = 0;x<l;x++)
                {
                    if(playuhCells[x]!='0'&&playuhCells[x]!='1')
                    {
                        //print("Line end: "+"\""+playuhCells[x]+"\"");
                        break;
                    }
                    else if(x==0)
                    {
                        if(playuhCells[x]=='1')
                        {
                        videoCells[39]=true;
                        //print("Wait unlock");
                        }
                    }
                    else
                    {
                        if(playuhCells[x]=='1')
                        {
                            videoCells[20+x]=true;
                            //print("Unlock: "+(20+x));
                        }
                    }
                }
                i=33;
                break;
                //credits
                case 36:
                if(totalCompBase>=42)
                {
                    videoCells[i]=true;
                    videoCells[i+1]=true;
                    i++;
                }
                break;
            }
        }

        //display thumbnails
        GameObject thumbObj = iconsHold.GetChild(0).GetChild(0).gameObject;
        for(int i = 1;i<videoCells.Length;i++)
        {
            if(videoCells[i])
            {
                Transform t = Instantiate(thumbObj,iconsHold.GetChild(i).position,Quaternion.identity).transform;
                t.SetParent(iconsHold.GetChild(i));
                t.GetComponent<SpriteRenderer>().sprite = thumbnails[i];
            }
        }
    }
    string getBackKeyName()
    {
        string s = SuperInput.GetKeyName("Start");
        if(s.Contains("Joy"))
        return changeWord(s);
        else return s;
    }
    string changeWord(string wordInsert)
	{
		for(int i = 0; i<wordInsert.Length;i++)
		{
			if(wordInsert[i]=='B')
			{
				wordInsert = wordInsert.Substring(i);
				//print(wordInsert);
				break;
			}
		}
		string oldWord = wordInsert;
		wordInsert = InputReader.joystickInterpreter(wordInsert);
		if(wordInsert==oldWord)
		{
			for(int i = 0; i<wordInsert.Length-1;i++)
			{
				if(char.IsDigit(wordInsert[i+1]))
				{
					wordInsert = wordInsert.Substring(i+1);
					break;
				}
			}
			wordInsert = "Btn"+wordInsert;
			//print(wordInsert);
		}
        return wordInsert;
	}
    // Start is called before the first frame update
    void Start()
    {
        DataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        DataS.clearedUnbeatenLevel = false;
        lines = Resources.Load<TextAsset>("Text/galleryCutsceneLines");
        titleText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        iconsHold = transform.GetChild(1);
        highlight = transform.GetChild(0);
        highlightRender = highlight.GetComponent<SpriteRenderer>();
        overlayRender = transform.parent.GetChild(3).GetComponent<SpriteRenderer>();
        u = highlight.GetChild(0).GetComponent<AudioSource>();
        staticSource = GetComponent<AudioSource>();
        fader = transform.parent.GetChild(1).GetComponent<SpriteRenderer>();
        musicSource = transform.parent.GetComponent<AudioSource>();
        vidplayer = GameObject.Find("Pixels").transform.GetChild(2).GetComponent<VideoPlayer>();
        cEvent = vidplayer.transform.GetComponent<cutsceneEvent>();
        startVolume = musicSource.volume;
        fader.color = new Color(0,0,0,1);
        //set back key name
        transform.parent.GetChild(4).GetChild(0).GetComponent<TextMeshPro>().text = getBackKeyName();
        unlockVids();
        if(DataS.specialData==0)
        StartCoroutine(fade(true,false));
        else
        {
            switch(DataS.specialData)
            {
                default: selection = 30+DataS.specialData; break;
                case 1:
                case 2:
                selection = 0; break;
            }
            //print("Selection: "+selection);
            if(selection>=33&&selection<=39)//bottom
            {
                bounds = new Vector2(28,35);
                iconsHold.localPosition = new Vector3(-665,1790,0);
            }
            else//top
            {
                bounds = new Vector2(4,11);
                iconsHold.localPosition = new Vector3(-665,590,0);
            }
            updateSelection(Vector2Int.zero);
            StartCoroutine(cutsceneEndAnim());
        }
        StartCoroutine(spawnDjole());
        cEvent.Start();
        DataS.specialData = 0;
    }
    IEnumerator spawnDjole()
    {
        Transform t = transform.parent.GetChild(2);
        Vector3 targetPos = new Vector3(-208.6f,-70.7f,0);
        if(DataS.specialData==0)
        {
            float progress = 0;
            while(progress<0.8f)
            {
                progress+=Time.deltaTime*0.5f;
                t.localPosition = Vector3.Lerp(t.localPosition,targetPos,progress);
                yield return 0;
            }
        }
        t.localPosition = targetPos;
    }
    IEnumerator despawnDjole()
    {
        Transform t = transform.parent.GetChild(2);
        Vector3 targetPos = new Vector3(-539f,-70.7f,0);
        float progress = 0;
        while(progress<1)
        {
            progress+=Time.deltaTime*0.5f;
            t.localPosition = Vector3.Lerp(t.localPosition,targetPos,progress);
            yield return 0;
        }
        t.localPosition = targetPos;
    }
    void playSound(int ID)
    {
        if(u.isPlaying) u.Stop();
        u.clip = sounds[ID];
        u.Play();
    }
    void playSoundGlobal(int ID)
    {
        staticSource.PlayOneShot(sounds[ID]);
    }
    void updateSelection(Vector2Int toAdd)
    {
        lastSelection = selection;
        if(toAdd.x!=0) //move left or right
        {
            selection=(int)Mathf.Repeat(selection+toAdd.x,40);
        }
        else //move up or down
        {
            int s = selection;
            if(toAdd.y==1) //up
            {
                //subtract 4
                s-=4;

            }
            else if(toAdd.y==-1) //down
            {
                s+=4;
            }
            selection = (int)Mathf.Repeat(s,40);
            //print("Selection: "+selection);
        }
        setSelPos(true,toAdd!=Vector2Int.zero ? true : false); //set highlight position
        setTitle();
    }
    void shiftCells()
    {
        if(selection<4&&lastSelection>35)//go back to top
        {
            bounds = new Vector2(4,11);
            iconsHold.localPosition = new Vector3(-665,590,0);
            return;
        }
        else if(selection>35&&lastSelection<4)//go back to bottom
        {
            bounds = new Vector2(28,35);
            iconsHold.localPosition = new Vector3(-665,1790,0);
        }
        if(selection<bounds.x&&bounds.x>4) //selection is lower than minimum, shift up
        {
            bounds-=(Vector2.one*4);
            iconsHold.localPosition = new Vector3(-665,iconsHold.localPosition.y-200,0);
        }
        else if(selection>bounds.y&&bounds.y<32) //selection is higher than maximum, shift down
        {
            bounds+=(Vector2.one*4);
            iconsHold.localPosition = new Vector3(-665,iconsHold.localPosition.y+200,0);
        }
    }
    void setSelPos(bool playBounce,bool playSoundEffect)
    {
        //shift cells if needed
        shiftCells();
        highlight.position = iconsHold.GetChild(selection).position; //set global postion, before this the icons have to shift up or down if they must
        //set selection size
        if(highlightAnim!=null)StopCoroutine(highlightAnim);
        if(!playBounce)
        {
            highlightRender.size = new Vector2(7.3f,4.7f);
            return;
        }
        if(playSoundEffect)
        playSound(0);
        highlightAnim = StartCoroutine(IHighlightAnim());
    }
    Coroutine highlightAnim;
    IEnumerator IHighlightAnim()
    {
        highlightRender.size = new Vector2(7.3f,4.7f);
        float progress = 0;
        Vector2 startSize = highlightRender.size,targetSize = startSize+(Vector2.one*0.4f);
        while(progress<1)
        {
            progress+=Time.deltaTime*20;
            Vector2 curSize = Vector2.Lerp(startSize,targetSize,progress);
            highlightRender.size = curSize;
            yield return 0;
        }
        progress = 0;
        targetSize = startSize;
        startSize = highlightRender.size;
        while(progress<1)
        {
            progress+=Time.deltaTime*10;
            Vector2 curSize = Vector2.Lerp(startSize,targetSize,progress);
            highlightRender.size = curSize;
            yield return 0;
        }
    }
    void setTitle()
    {
        if(videoCells[selection])
        titleText.text = titles[Mathf.Clamp(selection,0,titles.Length)];
        else titleText.text = "LOCKED";
    }
    Coroutine movementCor;
    string pressedKey = "";
    IEnumerator IMovementCor(Vector2Int dir)
    {
        int frameWait = 16;
        updateSelection(dir);
        //infinite loop
        while(true)
        {
            frameWait--;

            if(frameWait<=0)
            {
                updateSelection(dir);
                frameWait = 6;
            }
            yield return 0;
        }
    }
    void activateMovement(int dir,bool alt)//0 = down, 1 = up, 2 = right, 3 = left
    {
        if(movementCor!=null)StopCoroutine(movementCor);
        pressedKey = "";
        if(alt)
        {
            pressedKey+="Alt ";
        }

        switch(dir)
        {
            default:
            pressedKey += "Down";
            movementCor = StartCoroutine(IMovementCor(new Vector2Int(0,-1)));
            break;
            case 1:
            pressedKey += "Up";
            movementCor = StartCoroutine(IMovementCor(new Vector2Int(0,1)));
            break;
            case 2:
            pressedKey += "Right";
            movementCor = StartCoroutine(IMovementCor(new Vector2Int(1,0)));
            break;
            case 3:
            pressedKey += "Left";
            movementCor = StartCoroutine(IMovementCor(new Vector2Int(-1,0)));
            break;
        }
    }
    Coroutine fCor,overlayFadeCor;
    IEnumerator fadeMusicCor(bool fadeIn)
	{
		//print("Fading music");
		float TargetVolume = 0,curVolume = musicSource.volume,progress = 0;
		if(fadeIn)TargetVolume = startVolume;

		while(progress<=1)
		{
			progress+=Time.unscaledDeltaTime*2f;
			musicSource.volume = Mathf.Lerp(curVolume,TargetVolume,progress);
			//print("Fade progress: "+progress+" Volume: "+music.volume);
			yield return 0;
		}
		//print("Fading music over");
	}
    IEnumerator fadeOverlay(bool fadeIn,bool fast)
    {
        flickering = false;
        float progress = 0;
        Color curColor,targetColor;
        if(fadeIn)
        {
            curColor = new Color(1,1,1,1);
            targetColor = new Color(1,1,1,0.25f);
            overlayRender.color = curColor;
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            curColor = overlayRender.color;
            targetColor = new Color(1,1,1,1);
        }
        float speed = fast ?  1.5f : 0.7f;
		while(progress<=1)
		{
			progress+=Time.deltaTime*speed;
			overlayRender.color = Color.Lerp(curColor,targetColor,progress);
			yield return 0;
		}
        flickering = fadeIn;
    }
    IEnumerator fade(bool appear,bool playCutscene)
    {
        if(appear)
        {
            fader.color = new Color(0,0,0,1);
            yield return 0;
            yield return 0;
        }
        canMove = false;
        if(appear)
        {
            playSoundGlobal(2);
            if(overlayFadeCor!=null)StopCoroutine(overlayFadeCor);
                overlayFadeCor = StartCoroutine(fadeOverlay(true,false));
        }
        float progress = 0;
        if(!appear)
        {
            if(fCor!=null)StopCoroutine(fCor);
                fCor = StartCoroutine(fadeMusicCor(false));
            if(!playCutscene)
            {
                StartCoroutine(despawnDjole());
            }
            if(overlayFadeCor!=null)StopCoroutine(overlayFadeCor);
            overlayFadeCor = StartCoroutine(fadeOverlay(false,playCutscene ? false : true));
            float speed = 2;
            if(playCutscene)
            {
                speed = 1;
                yield return new WaitForSeconds(1f);
            }
            else yield return new WaitForSeconds(0.5f);
            while(progress<1)
            {
                progress+=Time.deltaTime*speed;
                fader.color = new Color(0,0,0,Mathf.Lerp(0,1,progress));
                yield return 0;
            }
        }
        //show/hide book assets
        /*for(int i = 0;i<transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActive(appear);
        }*/
        progress = 0;
        if(appear)
        {
            selection = 0;
            setSelPos(false,false);
            setTitle();
            //play music
            musicSource.loop = true;
            musicSource.clip = music;
            musicSource.Play();
            while(progress<1)
            {
                progress+=Time.deltaTime*2;
                fader.color = new Color(0,0,0,Mathf.Lerp(1,0,progress));
                yield return 0;
            }
            yield return new WaitForSeconds(0.5f);
            canMove = true;
        }
        if(playCutscene)
        {
            if(selection==0||selection==33) //normal credits
            {
                SceneManager.LoadScene(52);
            }
            else if(selection>33&&selection<=38) //true credits
            {
                SceneManager.LoadScene(53);
            }
            else
            {
                DataS.playCutscene = true;
                GameObject o = vidplayer.gameObject;
                o.SetActive(true);
                yield return 0;
                yield return new WaitUntil(()=>!o.activeInHierarchy);
                StartCoroutine(cutsceneEndAnim());
            }
        }
        if(!appear&&!playCutscene)
        {
            DataS.loadSceneWithoutLoadScreen(2);
        }
    }
    IEnumerator cutsceneEndAnim()
    {
        fader.color = new Color(0,0,0,1);
        yield return 0;
        yield return 0;
        playSoundGlobal(3);
        //overlayRender.color = new Color(1,1,1,0.25f);
        if(overlayFadeCor!=null)StopCoroutine(overlayFadeCor);
            overlayFadeCor = StartCoroutine(fadeOverlay(true,false));
        float progress = 0;
        if(musicSource.isPlaying)
        musicSource.UnPause();
        else
        {
            musicSource.loop = true;
            musicSource.clip = music;
            musicSource.Play();
        }
        if(fCor!=null)StopCoroutine(fCor);
            fCor = StartCoroutine(fadeMusicCor(true));
        while(progress<1)
        {
            progress+=Time.deltaTime*2;
            //print(progress);
            fader.color = new Color(0,0,0,Mathf.Lerp(1,0,progress));
            yield return 0;
        }
        canMove = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(pressedKey!=""&&SuperInput.GetKeyUp(pressedKey))
        {
            pressedKey = "";
            if(movementCor!=null)StopCoroutine(movementCor);
        }
        if(flickering)
        {
            //flicker
            if(flickerFrame<=0)
            {
                flickerFrame=UnityEngine.Random.Range(2,6);
                overlayRender.color = new Color(1,1,1,0.25f+Mathf.Repeat(overlayRender.color.a-0.25f+0.005f,0.01f));
            }
            else flickerFrame--;
        }
        //Priority: Down, Up, Right, Left
        if(canMove)
        {

            if(pressedKey!="Down")
            {
                if(SuperInput.GetKeyDown("Down"))
                    activateMovement(0,false);
                else if(SuperInput.GetKeyDown("Alt Down"))
                    activateMovement(0,true);
            }
            if(pressedKey!="Up")
            {
                if(SuperInput.GetKeyDown("Up"))
                    activateMovement(1,false);
                else if(SuperInput.GetKeyDown("Alt Up"))
                    activateMovement(1,true);
            }
            if(pressedKey!="Right")
            {
                if(SuperInput.GetKeyDown("Right"))
                    activateMovement(2,false);
                else if(SuperInput.GetKeyDown("Alt Right"))
                    activateMovement(2,true);
            }
            if(pressedKey!="Left")
            {
                if(SuperInput.GetKeyDown("Left"))
                    activateMovement(3,false);
                else if(SuperInput.GetKeyDown("Alt Left"))
                    activateMovement(3,true);
            }
            if(pressedKey==""&&SuperInput.GetKeyDown("Start"))
            {
                //exiting the menu
                canMove = false;
                if(musicSource.isPlaying)
                musicSource.Stop();
                playSoundGlobal(4);
                StartCoroutine(fade(false,false));
            }
            if(pressedKey==""&&SuperInput.GetKeyDown("Jump")&&videoCells[selection]) //select
            {
                canMove = false;
                playSelection();
            }
        }
    }
    void playSelection()
    {
        playSoundGlobal(1);
        setSelPos(true,false);
        musicSource.Pause();
        //get subtitles
        if(selection!=0&&selection<=17)
        {
            cEvent.subtitleFrames = new List<long>();
            List <string> textAsLines = new List<string>();
		    textAsLines.AddRange(lines.text.Split("\n"[0]));
            int i = cutsceneSubtitleHandles[selection-1]+1;
            while(i<textAsLines.Count-1&&Char.IsDigit(textAsLines[i][0]))
            {
                long o = 0;
                long.TryParse(textAsLines[i],out o);
                cEvent.subtitleFrames.Add(o);
                //print(o);
                i++;
            }
            cEvent.subtitlesFile = cutsceneSubtitles[selection-1];
            //print(cEvent.subtitlesFile.name+" "+cutsceneSubtitles[selection-1].name);
        }
        else if(selection==0||(selection>=33&&selection<=38))
        {
            if(selection==0)DataS.specialData=2;
            else DataS.specialData=selection-30;
            StartCoroutine(fade(false,true));
            return;
        }
        else
        cEvent.subtitleFrames = new List<long>();
        //set clip
        vidplayer.clip = clips[selection];
        cEvent.ignoreEmpty = true;
        DataS.playCutscene = true;
        cEvent.reloadSubtitles();
        StartCoroutine(fade(false,true));
    }
}
