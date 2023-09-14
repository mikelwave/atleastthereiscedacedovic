using System.Collections;
using UnityEngine;

// Script for a simple color fading effect on sprites
public class colorFade : MonoBehaviour
{
    public SpriteRenderer[] s = new SpriteRenderer[1];
    Coroutine fadeCor;
    public float speed = 1;
    [Range(0,1)]
    public float startTransparency = 1;

    public bool startTransparent = true, playOnAwake = false, disableOnEnd = false;
    bool loaded = false;

    // Fading coroutine
    IEnumerator fading(float target)
    {
        float startFade = s[0].color.a;
        float progress = 0;
        Color c = s[0].color;
        while(progress<1)
        {
            if(Time.timeScale!=0)
            {
                progress=Mathf.Clamp(progress+=Time.deltaTime*speed,0,1);
                for(int i = 0;i<s.Length;i++)
                s[i].color = new Color(c.r,c.b,c.g,Mathf.Lerp(startFade,target,progress));
            }
            yield return 0;
        }
        fadeCor = null;
        if(disableOnEnd)gameObject.SetActive(false);

    }
    // Start is called before the first frame update
    void Start()
    {
        if(!loaded)
        {
            if(s.Length==0||s[0]==null)
            {
                s = new SpriteRenderer[1];
                s[0] = GetComponent<SpriteRenderer>();
            }
            if(startTransparent)
            {
                Color c = s[0].color;
                for(int i = 0;i<s.Length;i++)
                s[i].color = new Color(c.r,c.b,c.g,0);
            }
            loaded = true;
        }
        
    }
    // Set the alpha manually
    public void setAlpha(float a)
    {
        Color c = s[0].color;
        s[0].color = new Color(c.r,c.b,c.g,a);
    }

    // Called when the object is enabled
    void OnEnable()
    {
        if(!loaded)Start();
        if(playOnAwake)
        {
            if(!startTransparent)
            {
                Color c = s[0].color;
                for(int i = 0;i<s.Length;i++)
                s[i].color = new Color(c.r,c.b,c.g,startTransparency);
            }
            if(s[0].color.a!=0)
            triggerFade(0);
            else triggerFade(startTransparency);
        }
    }

    // Called when the object is disabled
    void OnDisable()
    {
        if(fadeCor!=null)StopCoroutine(fadeCor);
    }

    // Trigger the fade effect
    public void triggerFade(float target)
    {
        if(!playOnAwake&&!startTransparent&&target==0)
        {
            Color c = s[0].color;
            for(int i = 0;i<s.Length;i++)
                s[i].color = new Color(c.r,c.b,c.g,startTransparency);
        }
        if(fadeCor!=null)StopCoroutine(fadeCor);
        fadeCor = StartCoroutine(fading(target));
    }
}
