using System.Collections;
using UnityEngine;
using TMPro;

public class gameSaveIcon : MonoBehaviour
{
    public dataShare DataS;
    TextMeshPro t;
    string showText = "Game Saved!";
    public void assignData(dataShare d)
    {
        DataS = d;
        t = GetComponent<TextMeshPro>();
    }
    public void setText(string tx)
    {
        showText = tx;
    }
    public void startMeasure()
    {
        StartCoroutine(appear());
    }
    public void display()
    {
        StartCoroutine(appear2());
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(GameObject.Find("Main Camera").transform);
        transform.localPosition = new Vector3(11.88f,-6.26f,1);
    }
    IEnumerator appear()
    {
        t.text = "Saving...";
        for(int i = 0;i<5;i++)
        {
            yield return 0;
        }
        yield return new WaitUntil(()=>!DataS.saving);
        t.text = showText;
        int waitFrames = 90;
        while(waitFrames>0)
        {
            waitFrames--;
            yield return 0;
        }
        float progress = 0;
        Color targColor = new Color(1,1,1,0),startColor = t.color;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*2f;
            t.color = Color.Lerp(startColor,targColor,progress);
            yield return 0;
        }
        Destroy(gameObject);
    }
    IEnumerator appear2()
    {
        t.text = showText;
        int waitFrames = 90;
        while(waitFrames>0)
        {
            waitFrames--;
            yield return 0;
        }
        float progress = 0;
        Color targColor = new Color(1,1,1,0),startColor = t.color;
        while(progress<1)
        {
            progress+=Time.unscaledDeltaTime*2f;
            t.color = Color.Lerp(startColor,targColor,progress);
            yield return 0;
        }
        Destroy(gameObject);
    }
}
