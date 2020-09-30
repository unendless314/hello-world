using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DrumGameplay : MonoBehaviour
{
	public GameObject NotePrefab;
	public SonataSongData[] Playlist;

	//References to important objects or components
	protected SonataControlInput ControlInput;
	protected SonataPlayer Player;
	protected List<GameObject> NoteObjects;
	protected Color[] Colors;   //了解

	//Game state variables
	protected float Score = 0f;
	protected float Multiplier = 1f;
	protected float Streak = 0;
	protected float MaxStreak = 0;
	protected float NumNotesHit = 0;
	protected float NumNotesMissed = 0;

	protected bool[] HasHitNoteOnStringIndexThisFrame;  //按弦矩陣隨時更新成false

	//Use this for initialization
	void Start()
	{
		//Init references to external objects/components
		ControlInput = GameObject.Find("Music Visualizator").GetComponent<SonataControlInput>();    //測試期間要改
		//ControlInput = GameObject.Find("Mov").GetComponent<SonataControlInput>();    //測試期間常常要改
		Player = GetComponent<SonataPlayer>();
		NoteObjects = new List<GameObject>();   //音符物件
		HasHitNoteOnStringIndexThisFrame = new bool[7]; //先改7(5)看看
		Colors = new Color[7];

		Colors[0] = new Color(0, 1, 1, 1);
		Colors[1] = new Color(0, 0, 1, 1);
		Colors[2] = new Color(0, 1, 0, 1);
		Colors[3] = new Color(1, 0, 1, 1);
		Colors[4] = new Color(1, 0, 0, 1);
		Colors[5] = new Color(1, 0.92f, 0.016f, 1);
		Colors[6] = new Color(0.5f, 0.5f, 0.5f, 1);

		//永遠只會抓第一首歌，程式碼要修改
		SonataSongData[] playlist = GetComponent<DrumGameplay>().GetPlaylist();
		GetComponent<DrumGameplay>().StartPlaying(0);

	}
	/* 還沒用到 start coroutine
    private void MusicStartOffsetTime()
    {
        if (ControlInput == GameObject.Find("Music Visualizator").GetComponent<SonataControlInput>())
        {
			StartCoroutine(StartCountdown(3.5f));
        }

		if (ControlInput == GameObject.Find("Mov").GetComponent<SonataControlInput>())
		{
			StartCoroutine(StartCountdown(0f));
		}
    }
	*/

	protected IEnumerator StartCountdown(float BeatCountDown)
    {
		yield return new WaitForSeconds(MyMath.BeatsToSeconds(BeatCountDown, Player.Song.BeatsPerMinute));
	}

	void Update()
	{
		if (Player.IsPlaying())
		{
			//No note has been hit int this frame yet
			ResetHasHitNoteOnStringIndexArray();    //按弦矩陣隨時更新成false

			UpdateNeckTextureOffset();  //功能不明，註解掉也不影響遊戲進行
			UpdateNotes();  //懂一半吧?

			UpdateGuiScore();
			UpdateGuiMultiplier();
		}
	}


	public void StartPlaying(int playlistIndex)
	{
		ResetGameStateValues();

		Player.SetSong(Playlist[playlistIndex]);    //***

		Player.Play();

		//MusicStartOffsetTime();
		CreateNoteObjects();    //產生音符物體

	}

	public void StopPlaying()
	{
		DestroyNoteObjects();

		StopAllCoroutines();
	}


	protected IEnumerator StartAudio(float delay)
	{
		yield return new WaitForSeconds(delay);

		Player.Play();
	}

	protected void CreateNoteObjects()
	{
		NoteObjects.Clear();    //音符陣列先清空

		for (int i = 0; i < Player.Song.Notes.Count; ++i)
		{
			//if( Player.Song.Notes[ i ].StringIndex != 0 && Player.Song.Notes[ i ].StringIndex != 4 )
			{
				//Create note and trail objects
				GameObject note = InstantiateNoteFromPrefab(Player.Song.Notes[i].StringIndex);

				//Hide object on start, they will be shown - when appropriate - in the UpdateNotes routine
				note.GetComponent<Renderer>().enabled = false;

#if UNITY_4_0
				note.SetActive( false );
#else
				note.active = false;
#endif

				NoteObjects.Add(note);
			}
		}
	}

	protected void DestroyNoteObjects()
	{
		for (int i = 0; i < NoteObjects.Count; ++i)
		{
			Destroy(NoteObjects[i]);
		}

		NoteObjects.Clear();
	}

	public SonataSongData[] GetPlaylist()
	{
		return Playlist;
	}

	protected void ResetHasHitNoteOnStringIndexArray()  //每一次update都會把這個按弦矩陣設為false
	{
		for (int i = 0; i < 7; ++i) //歸零陣列要改成 7(5)
		{
			HasHitNoteOnStringIndexThisFrame[i] = false;
		}
	}


	protected void UpdateNotes()    //懂一半吧?
	{
		for (int i = 0; i < NoteObjects.Count; ++i)
		{
			UpdateNotePosition(i);  //看得懂

			if (IsNoteHit(i))
			{
				HideNote(i);    //打中音符，音符就會隱藏但未消失

				Score += 10f * Multiplier;
				Streak++;
				NumNotesHit++;

				if (Streak > MaxStreak)
				{
					MaxStreak = Streak;
				}
			}

			if (WasNoteMissed(i))
			{
				HideNote(i);

				Streak = 0;
				Multiplier = 1;

				NumNotesMissed++;
			}
		}
	}

	protected void HideNote(int index)  //把音符隱藏起來
	{
		NoteObjects[index].GetComponent<Renderer>().enabled = false;
	}

	protected bool IsNoteHit(int index)
	{
		Note note = Player.Song.Notes[index];

		Debug.Log("我難道只有" + ControlInput.WasButtonJustPressed(note.StringIndex));
		//If no button is pressed on this notes string, it cannot be hit	//按一下陣列不是true的話，就不算打到音符
		if (!ControlInput.WasButtonJustPressed(note.StringIndex))	//這裡有問題，一直回傳 false (2020/09/30)
		{
			return false;
		}
		Debug.Log("我有進來" + index);	//沒進到這裡

		//If a note was already hit on this string during this frame, dont hit this one aswell
		if (HasHitNoteOnStringIndexThisFrame[note.StringIndex])  //隨時刷新的按弦矩陣如果原本就是true，
																 //表示這瞬間有個Note被點掉了，同一條弦上的
																 //其他Note不可以也被消除掉
		{
			return false;
		}

		//When the renderer is disabled, this note was already hit before
		if (NoteObjects[index].GetComponent<Renderer>().enabled == false)   //被打過的音符不能再被打中
		{
			return false;
		}

		//Check if this note is in the hit zone
		if (IsInHitZone(NoteObjects[index])) //如果音符在可打擊範圍內
		{
			//Set this flag so no two notes are hit with the same button press
			HasHitNoteOnStringIndexThisFrame[note.StringIndex] = true;
			Debug.Log("打中了" + index);

			return true;
		}

		//The note is not in the hit zone, therefore cannot be hit
		return false;
	}

	protected bool WasNoteMissed(int index) //看懂
	{
		//If position.z is greater than 0, this note can still be hit
		if (NoteObjects[index].transform.position.z > -2)  //卡的距離和 GetHitZoneEnd() 不一致
		{
			return false;
		}

		//If the renderer is disabled, this note was hit
		if (NoteObjects[index].GetComponent<Renderer>().enabled == false)
		{
			return false;
		}

		//Yea, this note was missed
		Debug.Log("漏掉了" + index);
		return true;
	}

	protected void ResetGameStateValues()
	{
		Score = 0;
		Streak = 0;
		MaxStreak = 0;
		Multiplier = 1;
		NumNotesMissed = 0;
		NumNotesHit = 0;
	}

	protected void UpdateNotePosition(int index)
	{
		Note note = Player.Song.Notes[index];
		
		//If the note is farther away then 6 beats, its not visible on the neck and we dont have to update it
		if (note.Time < Player.GetCurrentBeat() + 6)    //該音符的節拍屬性比目前播放中的節拍進度慢超過6拍時，就不再顯示音符
		{
			//If the note is not active, it is visible on the neck for the first time
#if UNITY_4_0
			if( !NoteObjects[ index ].activeSelf )
#else
			if (!NoteObjects[index].active) //可以改成activeSelf
#endif
			{
				//Activate and show the note
#if UNITY_4_0
				NoteObjects[ index ].SetActive( true );
#else
				NoteObjects[index].active = true; //可以改成activeSelf
#endif
				NoteObjects[index].GetComponent<Renderer>().enabled = true;

			}

			//Calculate how far the note has progressed on the neck
			float progress = (note.Time - Player.GetCurrentBeat() - 1f) / 6f;	//吉他遊戲是差 0.5 拍，我改成差 1 拍
			//這裡面的參數是特別調整過的，更改的話視覺效果會很怪異

			//Update its position
			Vector3 position = NoteObjects[index].transform.position;
			position.y = progress * GetYDisplacement() + GetYOffset(Player.Song.Notes[index].StringIndex);	//
			position.z = progress * GetZDisplacement() + GetZOffset(Player.Song.Notes[index].StringIndex);  //
			NoteObjects[index].transform.position = position;

			//這裡面的參數是特別調整過的，更改的話視覺效果會很怪異
		}

		else
        {

        }
	}

	protected void UpdateGuiScore()
	{
		//GameObject.Find("GUI Score").GetComponent<GUIText>().text = Mathf.Floor(Score).ToString();
		float GUIScore = Mathf.Floor(Score);
		Debug.Log("MyScore: " + GUIScore);
	}

	protected void UpdateGuiMultiplier()
	{
		Multiplier = Mathf.Ceil(Streak / 10);
		Multiplier = Mathf.Clamp(Multiplier, 1, 10);

		//GameObject.Find("GUI Multiplier").GetComponent<GUIText>().text = "x" + Mathf.Floor(Multiplier).ToString();

		float MyMultiplier = Mathf.Floor(Multiplier);
		Debug.Log("MyMultiplier: " + MyMultiplier);
	}

	protected void UpdateNeckTextureOffset()    //功能不明，而且全部註解掉也不影響遊戲進行
	{
	}

	protected bool IsInHitZone(GameObject note) //是否音符在可打擊區域內
	{
		int XValue = Mathf.FloorToInt((note.transform.position.x * 10)); //已經沒辦法取音符的 index 值了，只好從 x 座標來判斷音符在哪條弦上
		Debug.Log("X 座標為" + XValue);
		Debug.Log("開始範圍" + GetHitZoneBeginning(XValue));	//根本沒進來判斷式
		Debug.Log("結束範圍" + GetHitZoneEnd(XValue));  //根本沒進來判斷式

		return note.transform.position.z < GetHitZoneBeginning(XValue)
			&& note.transform.position.z > GetHitZoneEnd(XValue);
	}

	protected float GetGuitarNeckLength()   //軌道全長 (斜邊) 沒用到
	{
		return 4.1309f;
	}

	protected float GetYDisplacement()   //Y 的總位移量
	{
		return 0.421266f;
	}

	protected float GetYOffset(int StringIndex)   //軌道是懸空的，所以要加上 Y 初始值
	{
        switch (StringIndex)
        {
			case 0:
				//return 0;
				//return (1.915157f + 1.49389f)/2;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
				//return 1.915157f;	//最高點 Y 高度
				return 1.49389f + 0.1f;	//我真的不知道該怎麼修 = =
			case 1:
				//return 0;
				//return (2.307558f + 1.886292f)/2;
				//return 2.307558f;	//最高點 Y 高度
				return 1.886292f;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
			case 2:
				//return 0f;
				//return (1.618849f + 1.197582f) / 2;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
				//return 1.618849f;	//最高點 Y 高度
				return 1.197582f + 0.1f;	//我真的不知道該怎麼修 = =
			case 3:
				//return 0f;
				//return (1.778903f + 1.357637f) / 2;
				//return 1.778903f;	//最高點 Y 高度
				return 1.357637f;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
			case 4:
				//return 0f;
				//return (1.772468f + 1.351201f) / 2;
				//return 1.772468f;	//最高點 Y 高度
				return 1.351201f;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
			case 5:
				//return 0f;
				//return (1.641277f + 1.220011f) / 2;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
				//return 1.641277f;	//最高點 Y 高度
				return 1.220011f + 0.1f;    //我真的不知道該怎麼修 = =
			case 6:
				//return 0f;
				//return (2.152334f + 1.731068f) / 2;	//我以為加上最高點 Y 高度就能補正，結果失敗只好先取最高與最低點平均值
				//return 2.152334f;	//最高點 Y 高度
				return 1.731068f;
			default:
				return 0;
		}
	}

	protected float GetZDisplacement()   //Z 的總位移量
	{
		return 4.109441f;
	}

	protected float GetZOffset(int StringIndex)   //音符 Z 位置的補正值
	{
		switch (StringIndex)
		{
			case 0:
				//return 0f;
				//return -2.202f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -1.125f;
			case 1:
				//return 0f;
				//return -2.059f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -0.85f;
			case 2:
				//return 0f;
				//return -1.376f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -1.125f;
			case 3:
				//return 0f;
				//return -1.74f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -1.0f;
			case 4:
				//return 0f;
				//return -1.74f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -1.0f;
			case 5:
				//return 0f;
				//return -1.404f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -1.125f;
			case 6:
				//return 0f;
				//return -1.619f;	//用物件的 Z 座標補正不準，創一個平面對齊位置
				return -1.125f;
			default:
				return 0;
		}
	}

	public float GetScore() //了解
	{
		return Score;
	}

	public float GetMaximumStreak()
	{
		return MaxStreak;
	}

	public float GetNumNotesHit()   //了解
	{
		return NumNotesHit;
	}

	public float GetNumNotesMissed()    //了解
	{
		return NumNotesMissed;
	}

	protected Vector3 GetStartPosition(int stringIndex) //了解，呼叫音符的起始座標
	{
        switch (stringIndex)
        {
			case 0:
				return new Vector3(-0.8881167f, 1.915157f, 2.402217f);
			case 1:
				return new Vector3(-0.7173551f, 2.307558f, 2.930837f);
			case 2:
				return new Vector3(-0.532649f, 1.618849f, 2.453747f);
			case 3:
				return new Vector3(-0.1783119f, 1.778903f, 3.094725f);
			case 4:
				return new Vector3(0.1466231f, 1.772468f, 3.111839f);
			case 5:
				return new Vector3(0.5819693f, 1.641277f, 2.504729f);
			case 6:
				return new Vector3(0.851892f, 2.152334f, 2.714823f);
			default:
				return new Vector3(0f, 0f, 0f);
		}
	}

	protected GameObject InstantiateNoteFromPrefab(int stringIndex)    //了解
	{
		GameObject note = Instantiate(NotePrefab
									 , GetStartPosition(stringIndex)
									 , Quaternion.identity
									 ) as GameObject;

		note.GetComponent<Renderer>().material.color = Colors[stringIndex];   //預設音符為紅色，上弦前要改顏色
		//測試看看0926  note.transform.Rotate(new Vector3(-90, 0, 0));  //預設音符是站著，要轉方向

		return note;
	}

	protected float GetHitZoneBeginning(int XValue)   // 不同弦上的音符開始可以被打到的位置不同
	{
        switch (XValue)
        {
			case -9:   //第一根弦 X 座標為 -0.8881167，乘 10 倍取地板值
				return 10.4f;
			case -8:   //第二根弦 X 座標為 -0.7173551，乘 10 倍取地板值
				return 8.4f;
			case -6:   //第三根弦 X 座標為 -0.532649，乘 10 倍取地板值
				return 6.4f;
			case -2:   //第四根弦 X 座標為 -0.1783119，乘 10 倍取地板值
				return 4.4f;
			case 1:   //第五根弦 X 座標為 0.1466231，乘 10 倍取地板值
				return 2.4f;
			case 5:   //第六根弦 X 座標為 0.5819693，乘 10 倍取地板值
				return 2.4f;
			case 8:   //第七根弦 X 座標為 0.851892，乘 10 倍取地板值
				return 2.4f;
			default:    //預設值，推測用不到，寫保險的
				return 2.4f;
		}	
		//Adjust this if you want to enable the player to hit the note earlier
		//return 2.4f;
	}

	protected float GetHitZoneEnd(int XValue) // 不同弦上的音符最後可以被打到的位置不同
	{
		switch (XValue)
		{
			case -9:   //第一根弦 X 座標為 -0.8881167，乘 10 倍取地板值
				return -2f;
			case -8:   //第二根弦 X 座標為 -0.7173551，乘 10 倍取地板值
				return -2f;
			case -6:   //第三根弦 X 座標為 -0.532649，乘 10 倍取地板值
				return -2f;
			case -2:   //第四根弦 X 座標為 -0.1783119，乘 10 倍取地板值
				return -2f;
			case 1:   //第五根弦 X 座標為 0.1466231，乘 10 倍取地板值
				return -2f;
			case 5:   //第六根弦 X 座標為 0.5819693，乘 10 倍取地板值
				return -2f;
			case 8:   //第七根弦 X 座標為 0.851892，乘 10 倍取地板值
				return -2f;
			default:    //預設值，推測用不到，寫保險的
				return -2f;
		}
		//Adjust this if you want to enable the player to hit the note later
		//return -2f;
	}

}

