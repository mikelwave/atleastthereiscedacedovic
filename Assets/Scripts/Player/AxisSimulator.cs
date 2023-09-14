using UnityEngine;

public class AxisSimulator : MonoBehaviour {
	public Vector2 axisRange = new Vector2(-1,1);
	public Vector2 axisRangeY = new Vector2(-1,1);
	public float axisPosX;
	public float axisPosY;
	public float externalForce;
	public float axisAdder = 0.1f;
	public float axisAdderY = 0.1f;
	public bool workInStoppedTime = true;
	public bool acceptXInputs = true,acceptYInputs = true;

	public bool acceptFakeInputs = false;
	public float artificialX = 0;
	public float artificialY = 0;

	public float horAxis = 0;
	public float verAxis = 0;
	public string usedControllerHorAxis = "JoystickHor1";
	public string usedControllerVerAxis = "JoystickVer1";
	RebindableData rebind;
	GamepadEffects gamepadEffects;
	public float iceDivider = 2;
	public float currentDivider = 1;
	public float savedNormalDivider = 1;
	public InputReader reader;
	string dirPrefix = "";
	public bool Run=false;
	bool Left = false;
	bool Right = false;
	bool readInputs = false;
	public bool artificial = false;
	public bool lockLeft,lockRight = false;
	//dataShare DataS;
	public void setRange(float range)
	{
		axisRange = new Vector2(-range,range);
	}
	public void setRangeY(float range)
	{
		axisRangeY = new Vector2(-range,range);
	}
	void Start()
	{
		savedNormalDivider = currentDivider;
		rebind = GameObject.Find("Rebindable Manager").GetComponent<RebindableData>();
		reader = GameObject.Find("Rebindable Manager").GetComponent<InputReader>();
		gamepadEffects = rebind.GetComponent<GamepadEffects>();
		//DataS = GameObject.Find("DataShare").GetComponent<dataShare>();

	}
	void OnDisable()
	{
		//print("axis disabled");
	}
	void checkForAltInput()
	{
		if(SuperInput.GetKey("Alt Up")||SuperInput.GetKey("Alt Down")||SuperInput.GetKey("Alt Left")||SuperInput.GetKey("Alt Right"))
		{
			if(dirPrefix!= "Alt ")
			{
				dirPrefix = "Alt ";
			}
		}
		else if(SuperInput.GetKey("Up")||SuperInput.GetKey("Down")||SuperInput.GetKey("Left")||SuperInput.GetKey("Right"))
		{
			if(dirPrefix!= "")
			{
				dirPrefix = "";
			}

		}
	}
	void setRun()
	{
		if(SuperInput.GetKey("Run"))
		{
			Run=true;
			//print("p");
		}

		if(!SuperInput.GetKey("Run")) 
		{
			Run=false;
			//print("r");
		}
	}
	void setLeftRight()
	{
		//print("setleftright");
		//print("Right: "+SuperInput.GetKey(dirPrefix+"Right")+", Left: "+SuperInput.GetKey(dirPrefix+"Left"));
		if(SuperInput.GetKey(dirPrefix+"Right")&&SuperInput.GetKey(dirPrefix+"Left")){Right = false;Left = false;}
		else if(SuperInput.GetKey(dirPrefix+"Right")){Right = !lockRight? true:false; Left = false;//print("Right pressed");	//DataS.Right=Right; DataS.Left = Left;
		}
		else if(SuperInput.GetKey(dirPrefix+"Left")){Right = false; Left = !lockLeft? true:false;//print("Left pressed");	//DataS.Left=Left;
		}

		if(SuperInput.GetKeyUp(dirPrefix+"Right")){Right = false;//print("Up Right");	//DataS.Right=Right;
		}
		if(SuperInput.GetKeyUp(dirPrefix+"Left")){Left = false;//print("Up Left");	//DataS.Left=Left;
		}

	}
	void registerAxisValues()
	{
		if(SuperInput.GetKey(dirPrefix+"Up")) verAxis=1;
		else if(SuperInput.GetKey(dirPrefix+"Down")) verAxis=-1;
		else verAxis = 0;

		if(Right)horAxis=!lockRight ? 1:0;
		else if(Left)horAxis=!lockLeft ? -1:0;
		else
		{
			if(horAxis!=0)
			{
				horAxis = 0;
			}
		}
		//print(horAxis+" "+verAxis);
	}
	// Update is called once per frame
	void Update ()
	{
		if(!artificial)
		{
			if(readInputs)
			{
			//if(SuperInput.GetKey("Jump"))
			//print("t");

			//if(SuperInput.GetKey("Run"))
			//print("r");
			setLeftRight();
			setRun();
			}
			checkForAltInput();

			registerAxisValues();
		}

		if(Time.timeScale!=0 || workInStoppedTime)
		{
		//from 0 to 1
		if(Right&&axisPosX<axisRange.y&&acceptXInputs &&!acceptFakeInputs|| acceptFakeInputs && artificialX==1&&axisPosX<axisRange.y)
		{
			//Debug.Log("1");
			axisPosX+=axisAdder/currentDivider;
				if(axisPosX>axisRange.y)
					axisPosX = axisRange.y;
		}
		//from 1 to 0
		if ((!Right && axisPosX > 0f && !acceptFakeInputs && acceptXInputs) || (!acceptXInputs && axisPosX > 0f && !acceptFakeInputs) || (acceptFakeInputs && artificialX == 0f && axisPosX > 0f))
			{
				axisPosX -= axisAdder / currentDivider;
				if (axisPosX < 0f)
				{
					axisPosX = 0f;
				}
			}
			if ((Left && axisPosX > axisRange.x && acceptXInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialX == -1f && axisPosX > axisRange.x))
			{
				axisPosX -= axisAdder / currentDivider;
				if (axisPosX < axisRange.x)
				{
					axisPosX = axisRange.x;
				}
			}
			if ((!Left && axisPosX < 0f && !acceptFakeInputs) || (!acceptXInputs && axisPosX < 0f && !acceptFakeInputs) || (acceptFakeInputs && artificialX == 0f && axisPosX < 0f))
			{
				axisPosX += axisAdder / currentDivider;
				if (axisPosX > 0f)
				{
					axisPosX = 0f;
				}
			}
			if ((Right && axisPosX > axisRange.y && acceptXInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialX == 1f && axisPosX > axisRange.y))
			{
				axisPosX -= axisAdder / currentDivider;
				if (axisPosX < axisRange.y)
				{
					axisPosX = axisRange.y;
				}
			}
			if ((Left && axisPosX < axisRange.x && acceptXInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialX == -1f && axisPosX < axisRange.x))
			{
				axisPosX += axisAdder / currentDivider;
				if (axisPosX > axisRange.y)
				{
					axisPosX = axisRange.y;
				}
			}
			if ((SuperInput.GetKey(dirPrefix + "Up") && axisPosY < axisRangeY.y && acceptYInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialY == 1f && axisPosY < axisRangeY.y))
			{
				axisPosY += axisAdder / currentDivider;
				if (axisPosY > axisRangeY.y)
				{
					axisPosY = axisRangeY.y;
				}
			}
			if ((!SuperInput.GetKey(dirPrefix + "Up") && axisPosY > 0f && !acceptFakeInputs) || (!acceptYInputs && axisPosY > 0f && !acceptFakeInputs) || (acceptFakeInputs && artificialY == 0f && axisPosY > 0f))
			{
				axisPosY -= axisAdderY;
				if (axisPosY < 0f)
				{
					axisPosY = 0f;
				}
			}
			if ((SuperInput.GetKey(dirPrefix + "Down") && axisPosY > axisRangeY.x && acceptYInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialY == -1f && axisPosY > axisRangeY.x))
			{
				axisPosY -= axisAdderY;
				if (axisPosY < axisRangeY.x)
				{
					axisPosY = axisRangeY.x;
				}
			}
			if ((!SuperInput.GetKey(dirPrefix + "Down") && axisPosY < 0f && !acceptFakeInputs) || (!acceptYInputs && axisPosY < 0f && !acceptFakeInputs) || (acceptFakeInputs && artificialY == 0f && axisPosY < 0f))
			{
				axisPosY += axisAdderY;
				if (axisPosY > 0f)
				{
					axisPosY = 0f;
				}
			}
			if ((SuperInput.GetKey(dirPrefix + "Up") && axisPosY > axisRangeY.y && acceptYInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialY == 1f && axisPosY > axisRangeY.y))
			{
				axisPosY -= axisAdderY;
				if (axisPosY < axisRangeY.y)
				{
					axisPosY = axisRangeY.y;
				}
			}
			if ((SuperInput.GetKey(dirPrefix + "Down") && axisPosY < axisRangeY.x && acceptYInputs && !acceptFakeInputs) || (acceptFakeInputs && artificialY == -1f && axisPosY < axisRangeY.x))
			{
				axisPosY += axisAdderY;
				if (axisPosY > axisRangeY.y)
				{
					axisPosY = axisRangeY.y;
				}
			}
		}
		if (!artificial)
		{
			if (reader.controllerType != 0 && !gamepadEffects.usesController && reader.controllerType != 5)
			{
				MonoBehaviour.print("Uses gamepad: " + reader.controllerType);
				gamepadEffects.usesController = true;
			}
			else if ((reader.controllerType == 0 && gamepadEffects.usesController) || (reader.controllerType == 5 && gamepadEffects.usesController))
			{
				MonoBehaviour.print("Uses keyboard: " + reader.controllerType);
				gamepadEffects.usesController = false;
			}
			if (!readInputs)
			{
				readInputs = true;
			}
		}
	}
}