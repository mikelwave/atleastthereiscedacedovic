using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class puzzleMasterScript : MonoBehaviour
{
    #region main
    public string puzzleSequence = "2";
    public string currentSequence = "";
    int currentLength = 0;
    bool activated = false;
    [HideInInspector]
    public bool failed = false;
    List<puzzleBlockScript> puzzleBlocks;
    GameData data;
    Vector3 exitDoorPos;
    Transform[] doorEntrances = new Transform[2];
    public Sprite[] cardSprites;
    // Start is called before the first frame update
    void Start()
    {
        data = GameObject.Find("_GM").GetComponent<GameData>();
        exitDoorPos = transform.GetChild(0).position;
        puzzleBlocks = new List<puzzleBlockScript>();
        doorEntrances[0] = transform.GetChild(1).GetChild(0);
        doorEntrances[1] = transform.GetChild(2).GetChild(0);
        for(int i = 0;i<transform.childCount;i++)
        {
            if(transform.GetChild(i).GetComponent<puzzleBlockScript>()!=null)
            {
                puzzleBlocks.Add(transform.GetChild(i).GetComponent<puzzleBlockScript>());
            }
        }
    }
    public void setCardSprite(SpriteRenderer render)
    {
        render.sprite = cardSprites[Mathf.Clamp(currentSequence.Length,0,cardSprites.Length-1)];
    }
    public void activatePuzzle()
    {
        if(!activated)
        {
            doorEntrances[0].position = exitDoorPos;
            activated = true;
            ActivateEventTriggered();
        }
        //reset puzzle
        else
        {
            ResetEventTriggered();
            //print("Puzzle reset");
            failed = false;
            currentSequence = "";
            currentLength = 0;
            foreach(puzzleBlockScript puzzleScript in puzzleBlocks)
            {
                puzzleScript.resetBlock();
            }
            ActivateEventTriggered();
        }
    }
    public void hitSound()
    {
        data.playSoundStatic(1);
    }
    public bool addToSequence(string addChar)
    {
        data.playSoundStatic(1);
        currentLength++;
        //print(addChar);
        currentSequence = currentSequence.Insert(currentLength-1,addChar);
        //print("Sequence: "+puzzleSequence+" Current: "+currentSequence+" Length: "+(currentLength));
        ////print((currentLength-1)+" "+currentSequence.Length+" "+puzzleSequence.Length);
        if(currentLength-1!=puzzleSequence.Length&&currentSequence[currentLength-1]==puzzleSequence[currentLength-1])
        {
            UpdateEventTriggered();
            if(currentLength==puzzleSequence.Length)
            {
                data.playSoundStatic(85);
                //print("Puzzle solved.");
                doorEntrances[0].gameObject.SetActive(false);
                doorEntrances[1].position = exitDoorPos;
                foreach(puzzleBlockScript puzzleScript in puzzleBlocks)
                {
                    puzzleScript.disableBlock(false);
                }
            }
            else data.playSoundStatic(83);
            return true;
        }
        else
        {
            data.playSoundStatic(84);
            failed = true;
            foreach(puzzleBlockScript puzzleScript in puzzleBlocks)
            {
                puzzleScript.disableBlock(true);
            }
            return false;
        }
    }
    #endregion
    	//my event
     [Serializable]
     public class MainEvent : UnityEvent { }
 
     [SerializeField]
     private MainEvent mainEvent = new MainEvent(),Event2 = new MainEvent(),Event3 = new MainEvent();
     
     public MainEvent onMainEvent { get { return mainEvent; } set { mainEvent = value; } }
     public MainEvent onEvent2 { get { return Event2; } set { Event2 = value; } }
     public MainEvent onEvent3 { get { return Event3; } set { Event3 = value; } }
 
     public void ActivateEventTriggered()
     {
         onMainEvent.Invoke();
     }
     public void ResetEventTriggered()
     {
         Event3.Invoke();
     }
     public void UpdateEventTriggered()
     {
         Event2.Invoke();
     }
}
