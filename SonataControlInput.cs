using UnityEngine;
using System.Collections;

public class SonataControlInput : MonoBehaviour
{

	public const int NumStrings = 7; //先改成 7(5)

	//Stores if the button is held down
	protected bool[] ButtonsPressed;    //一直按陣列

	//Stores if the button was just pressed in this frame
	protected bool[] ButtonsJustPressed;    //按一下陣列

	//The five button objects in the scene
	protected GameObject[] StringButtons;   //鼓們陣列

	void Start()    //了解
	{
		ButtonsPressed = new bool[NumStrings];
		ButtonsJustPressed = new bool[NumStrings];

		for (int i = 0; i < NumStrings; ++i)
		{
			ButtonsPressed[i] = false;
			ButtonsJustPressed[i] = false;
		}

		SaveReferencesToStringButtons();    //了解
	}

	void Update()
	{
		ResetButtonsJustPressedArray(); //了解
	}

	void SaveReferencesToStringButtons()    //了解
	{
		StringButtons = new GameObject[NumStrings];

		StringButtons[0] = GameObject.Find("Gong1");
		StringButtons[1] = GameObject.Find("Gong2");
		StringButtons[2] = GameObject.Find("Drum1");
		StringButtons[3] = GameObject.Find("Drum2");
		StringButtons[4] = GameObject.Find("Drum3");
		StringButtons[5] = GameObject.Find("Drum4");
		StringButtons[6] = GameObject.Find("Gong3");
		//每顆按鍵都塞入按鍵陣列中
	}

	protected void ResetButtonsJustPressedArray()   //只有按一下會彈回初始位置，壓著按鍵不會初始化
	{
		for (int i = 0; i < NumStrings; ++i)
		{
			ButtonsJustPressed[i] = false;
		}
	}

	protected int GetNumButtonsPressed()    //了解，pressed 是計數器，五條弦同時按就累加到五，回傳同時壓著不放的弦數量
	{
		int pressed = 0;

		for (int i = 0; i < NumStrings; ++i)
		{
			if (ButtonsPressed[i])
			{
				pressed++;
			}
		}

		return pressed;
	}

	public bool IsButtonPressed(int index)  //檢查某一條弦是否正被壓著
	{
		return ButtonsPressed[index];
	}

	public bool WasButtonJustPressed(int index)    //檢查某一條弦是否剛剛被按了一下
	{
		return ButtonsJustPressed[index];
	}

	public void OnStringChange(int stringIndex, bool pressed)
	{
		if (pressed == IsButtonPressed(stringIndex))    //檢查狀態是否一致，但我不知道有什麼功能	我的 IsButtonPressed 不會刷新成 false
		{
			return;
		}

		Vector3 stringButtonPosition = StringButtons[stringIndex].transform.position;   //測試很好看，之後要拔掉

		if (pressed)    //如果有壓的話
		{
			if (GetNumButtonsPressed() < 2) //而且壓著的按鈕數量小於2的話	
			{
				stringButtonPosition.y = 2.0f;  //測試很好看，之後要拔掉

				ButtonsPressed[stringIndex] = true;   //按一下陣列和一直按陣列都要更新
				ButtonsJustPressed[stringIndex] = true;

				Debug.Log("我在想" + ButtonsPressed[stringIndex]);
				Debug.Log("有沒有" + ButtonsJustPressed[stringIndex]);
			}
		}
		else
		{
			stringButtonPosition.y = 1.0f;	//測試很好看，之後要拔掉
			//Update key state
			ButtonsPressed[stringIndex] = false;  //只更新一直按陣列，因為按一下陣列在update中就會刷新成false

			Debug.Log("然後呢" + ButtonsPressed[stringIndex]);
			Debug.Log("是否為" + ButtonsJustPressed[stringIndex]);
		}
		StringButtons[stringIndex].transform.position = stringButtonPosition;   //測試很好看，之後要拔掉
	}

	public GameObject GetStringButton(int index)    //告訴它弦的編號，它就能給你那條弦的按鈕
	{
		return StringButtons[index];
	}
}

