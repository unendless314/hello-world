using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardControl : MonoBehaviour
{
    protected KeyCode[] StringKeys;
    SonataControlInput ControlInput;

    // Start is called before the first frame update
    void Start()
    {
        ControlInput = GetComponent<SonataControlInput>();    //測試期間應該會常常換

        UpdateStringKeyArray();
    }

    protected void UpdateStringKeyArray()   //了解，其實StringButton的Key就是KeyCode
    {
        StringKeys = new KeyCode[SonataControlInput.NumStrings];
        StringKeys[0] = KeyCode.Alpha1;
        StringKeys[1] = KeyCode.Alpha2;
        StringKeys[2] = KeyCode.Alpha3;
        StringKeys[3] = KeyCode.Alpha4;
        StringKeys[4] = KeyCode.Alpha5;
        StringKeys[5] = KeyCode.Alpha6;
        StringKeys[6] = KeyCode.Alpha7;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < SonataControlInput.NumStrings; ++i)
        {
            CheckKeyCode(StringKeys[i], i);
        }
    }

    void CheckKeyCode(KeyCode code, int stringIndex)
    {
        if (Input.GetKeyDown(code))
        {
            ControlInput.OnStringChange(stringIndex, true); //按弦為true
        }
        if (Input.GetKeyUp(code))
        {
            ControlInput.OnStringChange(stringIndex, false);  //放弦為false
        }

        /*
		if( Input.GetKey( code ) && ControlInput.IsButtonPressed( stringIndex ) == false )
		{
			ControlInput.OnStringChange( stringIndex, true );   //不懂先跳過，很少執行到
			Debug.Log("我有被執行!");

			沒有按按鈕 Input.GetKey( code ) == false，
			也沒偵測到一直按的話 IsButtonPressed( stringIndex ) == false，
			一直按 IsButtonPressed( stringIndex ) 會被更新為 true，
			下一個 frame 就破壞條件不會再進來
			無程式碼也可正常遊戲

        }
		*/
    }
}
