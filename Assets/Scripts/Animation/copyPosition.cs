using UnityEngine;

public class copyPosition : MonoBehaviour
{
    public Transform target;
    public bool followX = true, followY = false;
    Vector3 offset;
    Vector3 targetPos;
    // Start is called before the first frame update
    void Start()
    {
        float offsetX = 0, offsetY = 0;
        if(followX)
        offsetX = Mathf.Abs(transform.position.x)-Mathf.Abs(target.position.x);
        if(followY)
        offsetY = Mathf.Abs(transform.position.y)-Mathf.Abs(target.position.y);
        offset = new Vector3(offsetX,offsetY,0);
    }
    void findTarget()
    {
        float X = 0,Y = 0;
        if(followX)
        X = target.position.x;
        else X = transform.position.x;
        if(followY)
        Y = target.position.y;
        else Y = transform.position.y;

        targetPos = new Vector3(X,Y,transform.position.z);
    }
    // Update is called once per frame
    void Update()
    {
        findTarget();
        transform.position = targetPos+offset;
    }
}
