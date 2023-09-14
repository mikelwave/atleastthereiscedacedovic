using UnityEngine;

[ExecuteInEditMode]
public class setRotation : MonoBehaviour
{
    [SerializeField]
    [Range (0,360)]
    private float rotation = 0;

    #if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if(!Application.isPlaying)
        {
            transform.eulerAngles = new Vector3(0,0,rotation);
        }
    }
    #endif
}
