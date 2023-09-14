using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class muteCreditsMusic : MonoBehaviour
{
    TextMeshPro tm;
    AudioSource auS;
    bool muted = false;
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
    Coroutine fadeText;
    IEnumerator IFadeText(bool fadeIn,bool repeat)
    {
        Color target = tm.color;
        target.a = fadeIn?1:0;
        float progress = 0;
        yield return new WaitUntil(()=>Time.timeScale!=0);
        while(progress<1&&tm.color.a<1)
        {
            progress+=Time.deltaTime;
            tm.color = Color.Lerp(tm.color,target,progress);
            yield return 0;
        }
        if(!repeat)yield break;
        yield return new WaitForSeconds(2f);
        playFade(!fadeIn,false);
    }
    void playFade(bool fadeIn,bool repeat)
    {
        if(fadeText!=null)StopCoroutine(fadeText);

        fadeText = StartCoroutine(IFadeText(fadeIn,repeat));
    }
    // Start is called before the first frame update
    void Start()
    {
        tm = GetComponent<TextMeshPro>();
        tm.text = "Mute: "+getBackKeyName();
        tm.color = new Color(1,1,1,0);
        if(dataShare.totalCompletedLevels<35)
        {
            this.enabled = false;
            return;
        }
        auS = GameObject.Find("Music").GetComponent<AudioSource>();
        playFade(true,true);
    }

    // Update is called once per frame
    void Update()
    {
        if(SuperInput.GetKeyDown("Start"))
        {
            muted = !muted;
            auS.volume = muted?0:0.7f;
            tm.text = (muted?"Unmute: ":"Mute: ")+getBackKeyName();
            playFade(true,true);
        }
    }
}
