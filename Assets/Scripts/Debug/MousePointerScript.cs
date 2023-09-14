using UnityEngine;

public class MousePointerScript : MonoBehaviour {
	Camera Cam;
	public Camera quadCam;
	Vector3 p = new Vector3();
	public Vector2 offset = new Vector2(-0.5f,-0.5f);
	float division;
	public bool debug = false;
	// Use this for initialization
	void Start () {
		Cam = Camera.main;
		if(quadCam == null)
		quadCam = GameObject.Find("QuadCamera").GetComponent<Camera>();
	}
	
	void OnGUI()
    {
		if(quadCam!=null){
        Event   e = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = e.mousePosition.x/division;
        mousePos.y = Cam.pixelHeight - e.mousePosition.y/division;

        p = Cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Cam.nearClipPlane));
		if(debug)
			{
        	GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        	GUILayout.Label("Screen pixels: " + Cam.pixelWidth + ":" + Cam.pixelHeight);
        	GUILayout.Label("Mouse position: " + mousePos);
        	GUILayout.Label("World position: " + p.ToString("F3"));
        	GUILayout.EndArea();
			}
		}
    }
	void Update()
	{
		if(quadCam!=null){
		division = quadCam.pixelHeight/Cam.pixelHeight;
		transform.position = new Vector3(p.x+offset.x,p.y+offset.y,transform.position.z);
		}
		else
		{
			Debug.LogError("No Quad Camera assigned.");
		}
	}
}
