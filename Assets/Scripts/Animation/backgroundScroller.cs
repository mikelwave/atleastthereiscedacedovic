using UnityEngine;

public class backgroundScroller : MonoBehaviour
{
    public float Speed;
    public bool vertical = false;
    float width,absPos;
    Transform t;
    // Start is called before the first frame update
    void Start()
    {
        GameObject O = new GameObject();
        O.transform.SetParent(transform);
        O.transform.localPosition = Vector3.zero;
        SpriteRenderer r = O.AddComponent<SpriteRenderer>();
        SpriteRenderer r0 = GetComponent<SpriteRenderer>();
        O.layer = gameObject.layer;
        t = O.transform;
        r.color = r0.color;
        r.sprite = r0.sprite;
        r.material = r0.material;
        r.drawMode = r0.drawMode;
        r.size = r0.size;
        r.sortingLayerID = r0.sortingLayerID;
        r.sortingOrder = r0.sortingOrder;
        r.tileMode = r0.tileMode;
        if(!vertical)
        width = (r.sprite.rect.width)/32;
        else width = (r.sprite.rect.height)/32;
        t.localScale = Vector3.one;
        t.name = "ScrollBG";
        //print("Width: "+width);
        Destroy(r0);
    }

    // Update is called once per frame
    void Update()
    {
        absPos = Mathf.Repeat(absPos+(Speed*Time.deltaTime),width);
        if(!vertical)
        t.localPosition=new Vector3(absPos,0,0);
        else t.localPosition=new Vector3(0,absPos,0);
    }
}
