using UnityEngine;

public class CameraBounds : MonoBehaviour {
	MGCameraController cameraControl;
	public bool used = false;

	void Start()
	{
		cameraControl = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		if(GetComponent<Collider2D>()!=null)
		GetComponent<Collider2D>().isTrigger = true;
	}
	void OnEnable()
	{
		if(cameraControl==null)
			cameraControl = GameObject.Find("Main Camera").GetComponent<MGCameraController>();
		cameraControl.lockLeft = true;
		cameraControl.lockRight = true;
		cameraControl.lockUp = true;
		cameraControl.lockDown = true;
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "pointLeft"&&used)
		{
		if(!cameraControl.lockRight)
		cameraControl.scrollingRight = true;

		if(!cameraControl.hardLockLeft)
		cameraControl.lockLeft = false;
		}
		if (other.name == "pointRight"&&used)
		{
		if(!cameraControl.lockLeft)
		cameraControl.scrollingRight = false;
		if(!cameraControl.hardLockRight)
		cameraControl.lockRight = false;
		}
		if (other.name == "pointUp"&&used)
		{
			if(!cameraControl.hardLockUp)
			cameraControl.lockUp = false;
		}
		if (other.name == "pointDown"&&used)
		{
			if(!cameraControl.hardLockDown)
			cameraControl.lockDown = false;
		}
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.name == "pointLeft"&&used)
		{
			cameraControl.lockLeft = true;
			cameraControl.targetPoint = new Vector3(cameraControl.transform.position.x,cameraControl.targetPoint.y,cameraControl.targetPoint.z);
			cameraControl.assignValues();
			cameraControl.resetVelocity(true);
		}
		if (other.name == "pointRight"&&used)
		{
			cameraControl.lockRight = true;
			cameraControl.targetPoint = new Vector3(cameraControl.transform.position.x,cameraControl.targetPoint.y,cameraControl.targetPoint.z);
			cameraControl.assignValues();
			cameraControl.resetVelocity(true);
		}
		if (other.name == "pointUp"&&used)
		{
			cameraControl.lockUp = true;
			cameraControl.resetVelocity(false);
		}
		if (other.name == "pointDown"&&used)
		{
			cameraControl.lockDown = true;
			cameraControl.resetVelocity(false);
		}
	}
}
