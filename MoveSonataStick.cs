using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSonataStick : MonoBehaviour
{
    public Transform stickReturnPosition, drumPosition1, drumPosition2, drumPosition3, drumPosition4, drumPosition5, drumPosition6, drumPosition7;
    public int stickSpeed;

    protected KeyCode[] StringKeys;
    SonataControlInput ControlInput;

    // Start is called before the first frame update
    void Start()
    {
        ControlInput = GameObject.Find("Music Visualizator").GetComponent<SonataControlInput>();    //測試期間應該會常常換

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

        /*  棍子先不要動
        if (Input.GetKey(KeyCode.Alpha1))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition1.position, stickSpeed * Time.deltaTime);
            if(transform.position == drumPosition1.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition2.position, stickSpeed * Time.deltaTime);
            if (transform.position == drumPosition2.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition3.position, stickSpeed * Time.deltaTime);
            if (transform.position == drumPosition3.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }
        
        if (Input.GetKey(KeyCode.Alpha4))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition4.position, stickSpeed * Time.deltaTime);
            if (transform.position == drumPosition4.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }

        if (Input.GetKey(KeyCode.Alpha5))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition5.position, stickSpeed * Time.deltaTime);
            if (transform.position == drumPosition5.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }

        if (Input.GetKey(KeyCode.Alpha6))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition6.position, stickSpeed * Time.deltaTime);
            if (transform.position == drumPosition6.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }

        if (Input.GetKey(KeyCode.Alpha7))
        {
            transform.position = Vector3.MoveTowards(transform.position, drumPosition7.position, stickSpeed * Time.deltaTime);
            if (transform.position == drumPosition7.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, stickReturnPosition.position, 2 * stickSpeed * Time.deltaTime);
            }
        }
        */
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
