using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class endLevelScreen : MonoBehaviour
{
    Animator anim;
    public Sprite[] numbers = new Sprite[10];
    Image[] coinNumbers = new Image[3],sausageNumbers = new Image[3],scoreNumbers = new Image[8],timeNumbers = new Image[4];
    GameData data;
    dataShare dataS;
    int coins,sausages,totalCoins,totalSausages;
    long score;
    float minutes,seconds;
    public bool coinsToGlobal = false,sausagesToGlobal = false,copyScore = false;
    public int timeDisplayMode = 0;
    int oldTime = 6000;
    public IEnumerator endSequence()
    {
        timeDisplayMode = 1;
        anim.SetTrigger("time show");
        coinsToGlobal = true;
        yield return new WaitUntil(()=>data.coins==0||coins==999);
        dataS.coins = Mathf.Clamp(totalCoins,0,999);
        sausagesToGlobal = true;
        yield return new WaitUntil(()=>data.levelSausages==0||sausages==999);
        dataS.sausages = Mathf.Clamp(totalSausages,0,999);

        //print(totalCoins+" datas: "+ dataS.coins);
    }
    void valToNumber(char c,Image numberDisplay)
    {
        int val = int.Parse(c.ToString());
        //print(val);
        numberDisplay.sprite = numbers[val];
    }
    void updateCoins()
    {
        if(data.timer==0) data.playTickSound();
        for(int i = 0;i<coinNumbers.Length;i++)
        {
            char value = coins.ToString("000")[i];
            valToNumber(value,coinNumbers[i]);
        }
    }
    void updateSausages()
    {
        if(data.timer==0) data.playTickSound();
        for(int i = 0;i<sausageNumbers.Length;i++)
        {
            char value = sausages.ToString("000")[i];
            valToNumber(value,sausageNumbers[i]);
        }
    }
    void updateScore()
    {
        score = data.score;
        for(int i = 0;i<scoreNumbers.Length;i++)
        {
            char value = score.ToString("00000000")[i];
            valToNumber(value,scoreNumbers[i]);
        }
    }
    void timeDisplay()
    {
        int timeDisp = 0;
        if(timeDisplayMode==1) timeDisp = Random.Range(0,6000);
        else if(timeDisplayMode==2)
        {
            string realTime = minutes.ToString("00")+seconds.ToString("00");
            //print(realTime);
            timeDisplayMode = 0;
            timeDisp = int.Parse(realTime);
            if(data.timeClock<oldTime&&!data.cheated)
            {
                anim.SetTrigger("new time");
                data.playSoundStatic(89);
            }
        }
        for(int i = 0;i<timeNumbers.Length;i++)
        {
            char value = timeDisp.ToString("0000")[i];
            valToNumber(value,timeNumbers[i]);
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        dataS = GameObject.Find("DataShare").GetComponent<dataShare>();
        data = GameObject.Find("_GM").GetComponent<GameData>();
        minutes = Mathf.Floor(data.timeClock/60);
        seconds = (data.timeClock%60);
        if(data.mode ==1)
        {
            transform.GetChild(0).GetComponent<Image>().sprite =numbers[11];
            Image i = transform.GetChild(0).GetChild(0).GetComponent<Image>();
            i.sprite = numbers[10];
            i.SetNativeSize();
            i.rectTransform.anchoredPosition = new Vector2(-131,71.25f);

        }
        print("TimeClock: "+data.timeClock+" Time: "+minutes+":"+seconds);
        coins = dataS.coins;
        //print(coins+" "+dataS.coins);
        sausages = dataS.sausages;
        totalCoins = Mathf.Clamp(coins+data.coins,0,999);
        totalSausages = Mathf.Clamp(sausages+data.levelSausages,0,999);
        score = data.score;
        for(int i = 0;i<coinNumbers.Length;i++)
        {
            coinNumbers[i] = transform.GetChild(1).GetChild(i).GetComponent<Image>();
            sausageNumbers[i] = transform.GetChild(2).GetChild(i).GetComponent<Image>();
        }
        for(int i = 0;i<scoreNumbers.Length;i++)
        {
            scoreNumbers[i] = transform.GetChild(3).GetChild(i).GetComponent<Image>();
        }
        for(int i = 1;i<=timeNumbers.Length;i++)
        {
            timeNumbers[i-1] = transform.GetChild(4).GetChild(i).GetComponent<Image>();
        }
        string currentLevelProgress = data.currentLevelProgress;
        int semicolonCount = 0,timerStartpoint = 0;
		string oldTimeString = "";
		bool containsFloppyData = false;
		for (int i = 0;i<currentLevelProgress.Length;i++)
		{
			if(semicolonCount>=1&&!containsFloppyData)
			{
				if(i<currentLevelProgress.Length-1
				&&currentLevelProgress[i+1]=='C'
				&&!containsFloppyData)
				{
					containsFloppyData = true;
					//print("Floppy data detected");
				}
			}
			if(semicolonCount==2&&!containsFloppyData)
			{
				//print("test2");
				oldTimeString+=currentLevelProgress[i];
			}
			else if(semicolonCount==3&&containsFloppyData)
			{
				//print("test3");
				oldTimeString+=currentLevelProgress[i];
			}

			if(currentLevelProgress[i]==';')
				semicolonCount++;
			
			if(semicolonCount==2&&timerStartpoint==0&&!containsFloppyData
			||semicolonCount==3&&timerStartpoint==0&&containsFloppyData)
			{
				timerStartpoint = i;
				//Debug.Log("Contains C: "+containsFloppyData+" ,Timer start point: "+timerStartpoint);
			}
		}
        if(oldTimeString!="")
		{
			int.TryParse(oldTimeString,out oldTime);
			//Debug.Log("Old time = "+oldTimeString+" and as int: "+oldTime);
		}
        updateCoins();
        updateSausages();
        updateScore();
    }
    void Update()
    {
        if(coinsToGlobal)
        {
            int coinsToAdd = 0;
            if(data.coins==0||coins==999)
            {
                coinsToGlobal = false;
                if(coins!=totalCoins) coins = totalCoins;
            }
            else if(data.coins>=200)
            {
                data.addCoin(-100,false);
                coinsToAdd = 100;
                coins+=coinsToAdd;
            }
            else if(data.coins>=20)
            {
                data.addCoin(-10,false);
                coinsToAdd = 10;
                coins+=coinsToAdd;
            }
            else
            {
                if(data.coins!=0)
                {
                    data.addCoin(-1,false);
                    coinsToAdd = 1;
                    coins+=coinsToAdd;
                }
            }
            coins = Mathf.Clamp(coins,0,999);
            data.coins = Mathf.Clamp(data.coins,0,500);
            updateCoins();
        }
        if(sausagesToGlobal)
        {
            int sausagesToAdd = 0;
            if(data.levelSausages==0||sausages==999)
            {
                sausagesToGlobal = false;
                if(sausages!=totalSausages) sausages = totalSausages;
                updateSausages();

            }
            else
            {
                data.levelSausages--;
                sausagesToAdd = 1;
            }
            sausages = Mathf.Clamp(sausages,0,999);
            data.levelSausages = Mathf.Clamp(data.levelSausages,0,999);
            sausages+=sausagesToAdd;
            updateSausages();
        }
        if(copyScore)
        {
            updateScore();
        }
        if(timeDisplayMode>0)
        {
            timeDisplay();
        }
    }
}
