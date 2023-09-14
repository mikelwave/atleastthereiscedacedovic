using UnityEngine;

public class TransformInheriter : MonoBehaviour
{
    public bool active = false;
    public bool unparentOnStart = false;
    public bool selfFollows = false;
    public Transform master;
    Transform[] children;
    public Vector3 offset;
    EnemyOffScreenDisabler eneOff;
    // Start is called before the first frame update
    void Start()
    {
        eneOff = master.GetComponent<EnemyOffScreenDisabler>();
        if(master==null)
        master = transform.GetChild(0);
        if(!selfFollows)
        {
            children = new Transform[transform.childCount-1];
            for (int i = 1; i<transform.childCount;i++)
            {
                //print(i);
                children[i-1] = transform.GetChild(i);
            }
        }
        if(unparentOnStart)
        {
            if(master.parent!=null)
            transform.SetParent(master.parent);
            else transform.SetParent(null);
        }
    }
    public void setActive(bool en)
    {
        active = en;
    }
    // Update is called once per frame
    void Update()
    {
        if(eneOff!=null)
        active = eneOff.visible;
        if(active)
        {
            if(!selfFollows)
            {
                for (int i = 0; i<children.Length;i++)
                {
                if(master==null) break;
                children[i].position = master.position+offset;
                children[i].localScale = master.localScale;
                }
                if(master==null)
                {
                    for (int i = 0; i<children.Length;i++)
                    {
                        children[i].parent = transform.parent;
                    }
                    Destroy(gameObject);
                }
            }
            else
            {
                if(master!=null)
                {
                    transform.position = master.position+offset;
                    transform.localScale = master.localScale;
                }
                else this.enabled = false;
            }
        }
    }
}
