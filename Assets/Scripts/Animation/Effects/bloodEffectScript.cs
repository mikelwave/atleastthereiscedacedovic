using UnityEngine;

//Script for player's blood wall slide and sticky jump effect
public class bloodEffectScript : MonoBehaviour
{
    public Sprite[] slide,impact;
    SimpleAnim2 anim2;

    // Start is called before the first frame update
    void Start()
    {
        anim2 = GetComponent<SimpleAnim2>();
        gameObject.SetActive(false);
    }

    // Sliding effect parameters
    public void setModeSlide()
    {
        anim2 = GetComponent<SimpleAnim2>();
        anim2.looping = true;
        anim2.disableOnEnd = false;
        anim2.sprites.Clear();
        anim2.waitBetweenFrames = 0.04f;
        for(int i = 0; i<slide.Length;i++)
        {
            anim2.sprites.Add(slide[i]);
        }
    }
    // Initialize the effect
    public void Initialize(Vector3 pos,int mode)
    {
        // Mode 0 = slide
        if(mode==0)
        {
            if(anim2.sprites[0] != slide[0])
            {
                anim2.sprites.Clear();
                for(int i = 0; i<slide.Length;i++)
                {
                    anim2.sprites.Add(slide[i]);
                }
            }
            anim2.looping = true;
            anim2.disableOnEnd = false;
        }
        // Mode 1 = impact
        else if(mode==1)
        {
            if(anim2.sprites[0] != impact[0])
            {
                anim2.sprites.Clear();
                for(int i = 0; i<slide.Length;i++)
                {
                    anim2.sprites.Add(impact[i]);
                }
            }
            anim2.looping = false;
            anim2.disableOnEnd = true;
        }

        // Assign position and enable.
        transform.position = pos;
        gameObject.SetActive(true);
        anim2.StartPlaying();
    }
}
