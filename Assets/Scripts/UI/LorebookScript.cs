using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LorebookScript : MonoBehaviour
{
    [Header("Debug")]
    public bool updateIcons = false;
    [Space]
    public TextAsset TextFile;
    List<string> textAsLines;
    public Sprite[] profiles;
    public Sprite[] locked = new Sprite[2];
    Sprite[] unlockedIcons;
    SpriteRenderer[] iconRenderers;
    public int[] textHandles;
    Transform highlight;
    SpriteRenderer highlightRender,portrait,fader;
    int selection = 0,lastSelection = 0;
    Transform iconsHold;
    Transform slider,slider2;
    TextMeshProUGUI tm,escapeText,scrollText;
    dataShare dataS;
    int unlockedChars,unlockedItems,unlockedEnemies,unlockedBosses;
    bool canMove=false,scrollMode = false;
    float scrollDistance = 0;
    bool isAlt = false;
    public GameObject notif;
    public bool disableFlag = false;
    public AudioClip[] music = new AudioClip[2];
    AudioSource musicSource;
    public AudioClip[] clips = new AudioClip[5];
    AudioSource[] sfxSource = new AudioSource[2];
    void updateSelection(Vector2Int toAdd)
    {
        lastSelection = selection;
        if(toAdd.x!=0) //move left or right
        {
            selection=(int)Mathf.Repeat(selection+toAdd.x,62);
        }
        else //move up or down
        {
            int s = selection;
            if(toAdd.y==1) //up
            {
                if(s<=2) //set 60
                    s=60;

                else if(s>=3&&s<5) //set 61
                    s=61;

                else if(s>=5&&s<=18||s>=23&&s<=53) //subtract 5
                    s-=5;

                else if(s>=19&&s<=22) //subtract 4
                    s-=4;

                else if(s==54) //set 49
                    s=49;

                else if(s==55) //set 52
                    s=52;

                else //if(s>=56&&s<=61) //subtract 2
                    s-=2;

            }
            else //down
            {
                if(s<=13||s>=19&&s<=48) //add 5
                    s+=5;
                else if(s==14)
                    s=18; //set 18

                else if(s>=15&&s<=48) //add 4
                    s+=4;

                else if(s>=49&&s<=51) //set 54
                    s=54;

                else if(s>=52&&s<=53) //set 55
                    s=55;

                else if(s>=54&&s<=59) //add 2
                    s+=2;

                else if(s==60) //set 0
                    s=0;
                else if(s==61) //set 4
                    s=4;
                
            }
            selection = (int)Mathf.Repeat(s,62);
            //print("Selection: "+selection);
        }
        setSelPos(true); //set highlight position
        setProfile();
    }
    void setProfile()
    {
        Sprite s = iconRenderers[selection].sprite;
        if(s!=locked[0]&&s!=locked[1])
        {
            portrait.sprite = profiles[selection];
            //set text
            string textToSet = "";
            int pointer = textHandles[selection];
            while(textAsLines[pointer][0]!='-')
            {
                textToSet+=textAsLines[pointer]+'\n';
                pointer++;
            }
            //print("Text:\n"+textToSet);
            tm.text = textToSet;
            tm.ForceMeshUpdate();
            float lineAmount = tm.textBounds.size.y/18;
            //print("Line count: "+lineAmount);
            if(lineAmount>10)
            scrollDistance = 0.5625f*(lineAmount-5);
            else scrollDistance = 0;
            //print("Scroll distance: "+scrollDistance);
            slider2.localPosition = new Vector3(0,2.2f,0);
            if(scrollDistance==0)
            {
                slider2.parent.gameObject.SetActive(false);
            }
            else slider2.parent.gameObject.SetActive(true);

            tm.transform.localPosition = new Vector3(-4.47f,2.1f,0);
        }
        else
        {
            scrollDistance = 0;
            slider2.parent.gameObject.SetActive(false);
            portrait.sprite = profiles[62];
            tm.transform.localPosition = new Vector3(-4.47f,2.1f,0);
            tm.text = "<align=\"center\">???";
        }
    }
    void setSelPos(bool playBounce)
    {
        //shift cells if needed
        shiftCells();
        highlight.position = iconsHold.GetChild(selection).position; //set global postion, before this the icons have to shift up or down if they must
        //set selection size
        if(highlightAnim!=null)StopCoroutine(highlightAnim);
        if(!playBounce)
        {
            highlightRender.size = new Vector2(3.25f,3.25f);
            return;
        }
        playSound(1);
        if(selection<54) //turn small
        {
            highlightAnim = StartCoroutine(IHighlightAnim(false));
        }
        else if(selection>=54) //turn big
        {
            highlightAnim = StartCoroutine(IHighlightAnim(true));
        }
        //destroy notif
        Transform t = iconRenderers[selection].transform;
        if(t.childCount!=0)
        {
            dataS.cellData[selection] = false;
            Destroy(t.GetChild(0).gameObject);
        }
    }
    Coroutine highlightAnim;
    IEnumerator IHighlightAnim(bool big)
    {
        if(big)highlightRender.size = new Vector2(7.5f,7.5f);
        else highlightRender.size = new Vector2(3.25f,3.25f);
        float progress = 0,startSize = highlightRender.size.x,targetSize = startSize+0.4f;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*20;
            float curSize = Mathf.Lerp(startSize,targetSize,progress);
            highlightRender.size = new Vector2(curSize,curSize);
            yield return 0;
        }
        progress = 0;
        targetSize = startSize;
        startSize = highlightRender.size.x;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*10;
            float curSize = Mathf.Lerp(startSize,targetSize,progress);
            highlightRender.size = new Vector2(curSize,curSize);
            yield return 0;
        }
    }
    void shiftCells()
    {
        bool play = false;
        if(selection<=28&&lastSelection>28) //page 1
        {
            iconsHold.transform.localPosition = new Vector3(-665,590,0);
            slider.localPosition = new Vector3(0,5.25f,0);
            play = true;
        }
        else if(selection>=29&&selection<=55&&(lastSelection<=28||lastSelection>=56)) //page 2
        {
            iconsHold.transform.localPosition = new Vector3(-665,1780,0);
            slider.localPosition = Vector3.zero;
            play = true;
        }
        else if(selection>=56&&lastSelection<56) //page 3
        {
            iconsHold.transform.localPosition = new Vector3(-665,2970,0);
            slider.localPosition = new Vector3(0,-5.25f,0);
            play = true;
        }

        if(play)playSound(3);
    }
    public bool updateUnlocked(bool spawnNotifs)
    {
        int lastChars = unlockedChars,lastItems = unlockedItems,lastEnemies = unlockedEnemies,lastBosses = unlockedBosses;
        //print("Lorebook updated.");
        //Get unlocked cells
        if(dataS.mode==0)
        {
            int p = dataShare.totalCompletedLevels;
            //Get unlocked chars
            if(p>=0) unlockedChars = 5;
            //Get unlocked items
            if(p>=35) unlockedItems = 14;
            else if(p>=27) unlockedItems = 13;
            else if(p>=16) unlockedItems = 12;
            else if(p>=13) unlockedItems = 11;
            else if(p>=8) unlockedItems = 10;
            else if(p>=3) unlockedItems = 9;
            else if(p>=2) unlockedItems = 8;
            else if(p>=0) unlockedItems = 1;
            //Get unlocked bosses
            if(p>=41) unlockedBosses = 8;
            else if(p>=35) unlockedBosses = 7;
            else if(p>=29) unlockedBosses = 5;
            else if(p>=23) unlockedBosses = 4;
            else if(p>=17) unlockedBosses = 3;
            else if(p>=11) unlockedBosses = 2;
            else if(p>=5 ) unlockedBosses = 1;
            else unlockedBosses = 0;
            //Get unlocked enemies
            switch(p)
            {
                default: unlockedEnemies = 35; break;
                case 0:
                case 1: unlockedEnemies = 0; break;
                case 2: unlockedEnemies = 2; break;
                case 3: unlockedEnemies = 3; break;
                case 4:
                case 5: 
                unlockedEnemies = 6; break;
                case 6: unlockedEnemies = 8; break;
                case 7: unlockedEnemies = 9; break;
                case 8:
                case 9:
                case 10:
                case 11:
                unlockedEnemies = 11; break;
                case 12:
                case 13:
                unlockedEnemies = 12; break;
                case 14: unlockedEnemies = 13; break;
                case 15: unlockedEnemies = 14; break;
                case 16:
                case 17:
                unlockedEnemies = 15; break;
                case 18: unlockedEnemies = 17; break;
                case 19: unlockedEnemies = 18; break;
                case 20: unlockedEnemies = 20; break;
                case 21: unlockedEnemies = 21; break;
                case 22:
                case 23:
                unlockedEnemies = 23; break;
                case 24: unlockedEnemies = 24; break;
                case 25: unlockedEnemies = 25; break;
                case 26: unlockedEnemies = 26; break;
                case 27:
                case 28:
                case 29:
                unlockedEnemies = 27; break;
                case 30: unlockedEnemies = 29; break;
                case 31: unlockedEnemies = 31; break;
                case 32: unlockedEnemies = 33; break;
                case 33: unlockedEnemies = 34; break;
            }
        }
        else
        {
            unlockedChars = 5;
            unlockedBosses = 8;
            unlockedItems = 14;
            unlockedEnemies = 35;
        }
        //Unlock cells
        //Chars
        for(int i = 0;i<5;i++)
        {
            if(i>=unlockedChars)
            iconRenderers[i].sprite = locked[0];
            else
            {
                if(spawnNotifs&&dataS.mode==0&&iconRenderers[i].sprite == locked[0]&&i!=0) //display notif
                {
                    dataS.cellData[i] = true;
                    createNotif(iconRenderers[i].transform,false);
                }
                iconRenderers[i].sprite = unlockedIcons[i];
            }
        }
        int offset = 5;
        //items
        for(int i = 0;i<14;i++)
        {
            if(i>=unlockedItems)
            iconRenderers[i+offset].sprite = locked[0];
            else
            {
                if(spawnNotifs&&dataS.mode==0&&iconRenderers[i+offset].sprite == locked[0]) //display notif
                {
                    dataS.cellData[i+offset] = true;
                    createNotif(iconRenderers[i+offset].transform,false);
                }
                iconRenderers[i+offset].sprite = unlockedIcons[i+offset];
            }
        }
        offset = 19;
        //enemies
        for(int i = 0;i<35;i++)
        {
            if(i>=unlockedEnemies)
            iconRenderers[i+offset].sprite = locked[0];
            else
            {
                if(spawnNotifs&&dataS.mode==0&&iconRenderers[i+offset].sprite == locked[0]) //display notif
                {
                    dataS.cellData[i+offset] = true;
                    createNotif(iconRenderers[i+offset].transform,false);
                }
                iconRenderers[i+offset].sprite = unlockedIcons[i+offset];
            }
        }
        offset = 54;
        //bosses
        for(int i = 0;i<8;i++)
        {
            if(i>=unlockedBosses)
            iconRenderers[i+offset].sprite = locked[1];
            else
            {
                if(spawnNotifs&&dataS.mode==0&&iconRenderers[i+offset].sprite == locked[1]) //display notif
                {
                    dataS.cellData[i+offset] = true;
                    createNotif(iconRenderers[i+offset].transform,true);
                }
                iconRenderers[i+offset].sprite = unlockedIcons[i+offset];
            }
        }

        if(unlockedChars!=lastChars||unlockedItems!=lastItems||unlockedEnemies!=lastEnemies||unlockedBosses!=lastBosses) //change detected
        {
            print("Lorebook changed.");
            return true;
        }
        //print("Lorebook didn't change.");
        return false; //no change
    }
    void createNotif(Transform par, bool isBoss)
    {
        if(par.childCount!=0) return;
        GameObject o = Instantiate(notif,par.position,Quaternion.identity);
        Transform t = o.transform;
        t.SetParent(par);
        t.localScale = Vector3.one;
        if(!isBoss)
        {
            t.localPosition = new Vector3(0.67f,0.53f); //normal icon
            return;
        }
        t.localPosition = new Vector3(1.5f,1.225f); //boss icon
    }
    void setBackIcon()
    {
        escapeText.text = getBackKeyName();
    }
    void setScrollTip()
    {
        scrollText.text = getScrollTipName(isAlt);
    }
    string getBackKeyName()
    {
        string s = SuperInput.GetKeyName("Start");
        if(s.Contains("Joy"))
        return changeWord(s);
        else return s;
    }
    string getScrollTipName(bool isAlt)
    {
        string main = "Scroll: ";
        string s = SuperInput.GetKeyName("Jump");
        if(s.Contains("Joy"))
        s = changeWord(s);
        main+=s+"+"; //Scroll: Jump+
        string altAdd = "";
        if(isAlt)altAdd+="Alt ";
        
        s = SuperInput.GetKeyName(altAdd+"Up");
        if(s.Contains("Joy"))
        s = changeWord(s);
        main+=s+"/"; //Scroll: Jump+Up/
        
        s = SuperInput.GetKeyName(altAdd+"Down");
        if(s.Contains("Joy"))
        s = changeWord(s);
        main+=s; //Scroll: Jump+Up/Down
        return main;
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
    float startVolume = 1;
    Coroutine musicPlayCor;
    IEnumerator musicIntro()
	{

		float clipLength = music[0].length;
		//Debug.Log("Clip length: "+clipLength);
		musicSource.loop = false;
		musicSource.clip = music[0];
		musicSource.Play();
		yield return new WaitUntil(()=>musicSource.time>=clipLength||!musicSource.isPlaying);

		if(musicSource.isPlaying)
			musicSource.Stop();
			
        musicSource.loop = true;
		musicSource.clip = music[1];
		musicSource.Play();
	}
    Coroutine fCor;
    IEnumerator fadeMusicCor()
	{
		float TargetVolume = 0,curVolume = musicSource.volume,progress = 0;

		while(progress<=1)
		{
			progress+=Time.unscaledDeltaTime*2f;
			musicSource.volume = Mathf.Lerp(curVolume,TargetVolume,progress);
			yield return 0;
		}
	}
    void playSound(int ID)
    {
        AudioSource u;
        if(ID==1||ID==2) u = sfxSource[0];
        else u = sfxSource[1];

        if(u.isPlaying) u.Stop();
        u.clip = clips[ID];
        u.Play();
    }
    public void Initialize()
    {
        highlight = transform.GetChild(0);
        sfxSource[0] = highlight.GetComponent<AudioSource>();
        sfxSource[1] = highlight.GetChild(0).GetComponent<AudioSource>();
        highlightRender = highlight.GetComponent<SpriteRenderer>();
        portrait = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        tm = transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        escapeText = transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>();
        scrollText = escapeText.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>();
        slider = transform.GetChild(2).GetChild(0).GetChild(0);
        slider2 = transform.GetChild(2).GetChild(1).GetChild(0);
        iconsHold = transform.GetChild(3);
        fader = transform.parent.GetChild(1).GetComponent<SpriteRenderer>();
        int c = iconsHold.childCount;
        musicSource = transform.parent.GetComponent<AudioSource>();
        startVolume = musicSource.volume;
        //load all default icons and icon renderers.
        unlockedIcons = new Sprite[c];
        iconRenderers = new SpriteRenderer[c];
        for(int i = 0;i<c;i++)
        {
            iconRenderers[i] = iconsHold.GetChild(i).GetComponent<SpriteRenderer>();
            unlockedIcons[i] = iconRenderers[i].sprite;
        }
        dataS = transform.parent.parent.GetComponent<dataShare>();

		textAsLines = new List<string>();
		textAsLines.AddRange(TextFile.text.Split("\n"[0]));

        //setBackIcon();
        //setScrollTip();

        //initial icon load
        //updateUnlocked();
        GetComponent<UnityEngine.UI.Image>().enabled = false;
        for(int i = 0;i<transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        //load icons
        forceNotifs();
        //print("Lorebook data initialized.");
    }
    public void forceNotifs()
    {
        if(dataS.mode==0)
        for(int i = 1;i<iconsHold.childCount;i++)
        {
            if(dataS.cellData[i])
            {
                createNotif(iconsHold.GetChild(i).transform, i>=54 ? true : false);
            }
            else
            {
                if(iconsHold.GetChild(i).transform.childCount!=0)
                {
                    Destroy(iconsHold.GetChild(i).transform.GetChild(0).gameObject);
                }
            }
        }
        else
        for(int i = 1;i<iconsHold.childCount;i++)
        {
            if(iconsHold.GetChild(i).transform.childCount!=0)
            {
                Destroy(iconsHold.GetChild(i).transform.GetChild(0).gameObject);
            }
        }
    }
    void OnEnable()
    {
        iconsHold.transform.localPosition = new Vector3(-665,590,0);
        slider.localPosition = new Vector3(0,5.25f,0);
        setBackIcon();
        setScrollTip();
        StartCoroutine(fade(true));
    }
    IEnumerator fade(bool showBook)
    {
        disableFlag = false;
        canMove = false;
        if(!showBook)
        {
            if(fCor!=null)StopCoroutine(fCor);
            fCor = StartCoroutine(fadeMusicCor());
            playSound(4);
        }
        float progress = 0;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*2;
            fader.color = new Color(0,0,0,Mathf.Lerp(0,1,progress));
            yield return 0;
        }
        //show/hide book assets
        GetComponent<UnityEngine.UI.Image>().enabled = showBook;
        for(int i = 0;i<transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActive(showBook);
        }
        progress = 0;
        if(showBook)
        {
            selection = 0;
            setSelPos(false);
            setProfile();
            //play music
            musicSource.volume = startVolume;
            if(musicPlayCor!=null)StopCoroutine(musicPlayCor);
            musicPlayCor = StartCoroutine(musicIntro());
            playSound(0);
        }
        else disableFlag = true;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*2;
            fader.color = new Color(0,0,0,Mathf.Lerp(1,0,progress));
            yield return 0;
        }
        disableFlag = false;
        if(showBook)
        {
            canMove = true;
        }
        else
        {
            if(musicPlayCor!=null)StopCoroutine(musicPlayCor);
            transform.parent.gameObject.SetActive(false);
        }
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
        isAlt = alt;
        if(alt)
        {
            pressedKey+="Alt ";
        }
        setScrollTip();
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
    void setSlider2Pos(float yCurrent)
    {
        float point = (yCurrent-2.1f)/(scrollDistance-2.1f);
        slider2.transform.localPosition = new Vector3(0,2.2f-4.4f*point,0);
    }
    void Update()
    {
        if(updateIcons)
        {
            updateIcons = false;
            updateUnlocked(true);
        }
        if(pressedKey!=""&&SuperInput.GetKeyUp(pressedKey))
        {
            pressedKey = "";
            if(movementCor!=null)StopCoroutine(movementCor);
        }
        //Priority: Down, Up, Right, Left
        if(canMove)
        {
            if(!scrollMode)
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
                    scrollMode = false;
                    canMove = false;
                    StartCoroutine(fade(false));
                }
                if(scrollDistance!=0&&pressedKey==""&&SuperInput.GetKeyDown("Jump")) //move the text
                {
                    pressedKey = "";
                    scrollMode = true;
                    playSound(2);
                    if(highlightAnim!=null)StopCoroutine(highlightAnim);
                    if(selection<54) //small bounce
                    {
                        highlightAnim = StartCoroutine(IHighlightAnim(false));
                    }
                    else if(selection>=54) //big bounce
                    {
                        highlightAnim = StartCoroutine(IHighlightAnim(true));
                    }
                    if(movementCor!=null)StopCoroutine(movementCor);
                }
            }
            else if(scrollDistance!=0)
            {
                if(SuperInput.GetKeyDown("Up")||SuperInput.GetKeyDown("Down"))
                {
                    isAlt = false;
                    setScrollTip();
                }
                if(SuperInput.GetKeyDown("Alt Up")||SuperInput.GetKeyDown("Alt Down"))
                {
                    isAlt = true;
                    setScrollTip();
                }
                if(!isAlt&&SuperInput.GetKey("Up")||isAlt&&SuperInput.GetKey("Alt Up"))
                {
                    float yCur = tm.transform.localPosition.y;
                    tm.transform.localPosition = new Vector3(-4.47f,Mathf.Clamp(yCur-=0.05625f*2,2.1f,scrollDistance),0);
                    setSlider2Pos(tm.transform.localPosition.y);
                }
                else if(!isAlt&&SuperInput.GetKey("Down")||isAlt&&SuperInput.GetKey("Alt Down"))
                {
                    float yCur = tm.transform.localPosition.y;
                    tm.transform.localPosition = new Vector3(-4.47f,Mathf.Clamp(yCur+=0.05625f*2,2.1f,scrollDistance),0);
                    setSlider2Pos(tm.transform.localPosition.y);
                }
                if(pressedKey==""&&SuperInput.GetKeyUp("Jump")) //disable scrolling
                {
                    scrollMode = false;
                }
            }
        }
    }
}
