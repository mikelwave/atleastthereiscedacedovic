using UnityEngine;

public class unparentOnPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(transform.parent.parent!=null)
        {
            transform.SetParent(transform.parent.parent);
        }
        else transform.SetParent(null);
    }
}
