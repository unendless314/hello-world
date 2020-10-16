using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DrumGameplay : MonoBehaviour
{
	public GameObject NotePrefab1, NotePrefab2, NotePrefab3, NotePrefab4, NotePrefab5, NotePrefab6, NotePrefab7, NotePrefab8;
	public Transform NoteStart1, NoteStart2, NoteStart3, NoteStart4, NoteStart5, NoteStart6, NoteStart7, NoteStart8;
	public Transform NoteEnd1, NoteEnd2, NoteEnd3, NoteEnd4, NoteEnd5, NoteEnd6, NoteEnd7, NoteEnd8;

	public SonataSongData[] Playlist;
	public int ChooseSongNumber;
	public bool PlayAndPause;
	public bool StopSong;
	//protected Color[] Colors;   //音符不需要上顏色

	//References to important objects or components
	protected RunPlayer Player;	//測試期間常常要改
	protected List<GameObject> NoteObjects;


	//Use this for initialization
	void Start()
	{
		//Init references to external objects/components

		Player = GameObject.Find("SheetsManager").GetComponent<RunPlayer>();

		NoteObjects = new List<GameObject>();   //音符物件

		/*	音符不需要上顏色
		Colors = new Color[7];

		Colors[0] = new Color(0, 1, 1, 1);
		Colors[1] = new Color(0, 0, 1, 1);
		Colors[2] = new Color(0, 1, 0, 1);
		Colors[3] = new Color(1, 0, 1, 1);
		Colors[4] = new Color(1, 0, 0, 1);
		Colors[5] = new Color(1, 0.92f, 0.016f, 1);
		Colors[6] = new Color(0.5f, 0.5f, 0.5f, 1);

		Colors[7] = new Color(1, 1, 1, 1);
		*/

		PlayAndPause = false;
		StopSong = false;
	}

    void Update()
	{
		if (Player.IsPlaying())
		{
			UpdateNotes();

            if (!PlayAndPause)
            {
				Player.Pause(); //正在播音樂，但取消勾勾，表示暫停播放
			}
		}
		else //沒有播音樂
		{
			if (PlayAndPause)
			{
                if (Player.videoTime > 0 && Player.showSAT > 0)
                {
					Player.Play();  //繼續剛才的播放
				}
                else
                {
					//沒有播音樂，且選取打勾，且秒數為 0，表示重新開始播放
					GetComponent<DrumGameplay>().StartPlaying((ChooseSongNumber));  //要第幾首歌就是從這裡傳
				}
			}
		}

		if (StopSong)   //不管有沒有播音樂，選取勾勾就表示音樂停止並結束
		{
			Player.Stop();

            try
            {
				for (int i = 0; i < Player.Song.Notes.Count; i++)	//音符物件只刪除一次，之後都顯示 Debug 訊息
				{
					Destroy(NoteObjects[i]);
				}
				NoteObjects.Clear();

			}
            catch (Exception ex)
            {
				Debug.Log("音符物件已刪除");
            }
		}
	}


    public void StartPlaying(int playlistIndex)
	{
		SonataSongData[] playlist = GetComponent<DrumGameplay>().GetPlaylist();
		Player.SetSong(Playlist[playlistIndex]);
		CreateNoteObjects();    //這裡不可以瞬間生成，那順序調換的話呢?

		Player.Play();
		//CreateNoteObjects();	//這裡不可以瞬間生成，那順序調換的話呢?
	}

	protected void CreateNoteObjects()
	{
		NoteObjects.Clear();

		for (int i = 0; i < Player.Song.Notes.Count; ++i)   //Player.Song.Notes.Count
		{
			GameObject note = InstantiateNoteFromPrefab(Player.Song.Notes[i].StringIndex);

				//Hide object on start, they will be shown - when appropriate - in the UpdateNotes routine
				//note.GetComponent<Renderer>().enabled = false;

#if UNITY_4_0
				note.SetActive( false );
#else
				note.active = false;
#endif
				NoteObjects.Add(note);
		}
	}

	public SonataSongData[] GetPlaylist()
	{
		return Playlist;
	}

	protected void UpdateNotes() 
	{
		for (int i = 0; i < NoteObjects.Count; ++i)
		{
			UpdateNotePosition(i);

			/* 暫時用不到 2020/10/05
			if (IsNoteHit(i))
			{
				HideNote(i);    //打中音符，音符就會隱藏但未消失
			}
			*/

			if (WasNoteMissed(i))
			{
				HideNote(i);
			}
		}
	}

	protected void HideNote(int index)  //把音符隱藏起來
	{
		GameObject NoteChild = NoteObjects[index].transform.GetChild(0).gameObject;
		NoteChild.GetComponent<Renderer>().enabled = false;
		//NoteObjects[index].GetComponent<Renderer>().enabled = false;


		//NoteObjects[index].transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = false;

	}

	protected bool IsNoteHit(int index)
	{
		Note note = Player.Song.Notes[index];
		GameObject NoteChild = NoteObjects[index].transform.GetChild(0).gameObject;

		//When the renderer is disabled, this note was already hit before
		if (NoteChild.GetComponent<Renderer>().enabled == false)   //被打過的音符不能再被打中
		{
			return false;
		}

		//Check if this note is in the hit zone
		if (IsInHitZone(index)) //如果音符在可打擊範圍內
		{
			//StringHasNote[note.StringIndex] = true;	
			return true;
		}

		//The note is not in the hit zone, therefore cannot be hit
		return false;
	}

	protected bool WasNoteMissed(int index) //看懂
	{
		Note note = Player.Song.Notes[index];
		GameObject NoteChild = NoteObjects[index].transform.GetChild(0).gameObject; ;

		int stringIndex = note.StringIndex; 

		if (IsInHitZone(index)) //如果音符在可打擊範圍內
		{
			//StringHasNote[note.StringIndex] = true;	我寫的沒用到
		}

		//If position.z is greater than 0, this note can still be hit
		if (NoteObjects[index].transform.position.z > (GetHitZoneEnd(stringIndex)))  //卡的距離比 GetHitZoneEnd() 多一些些
		{
			return false;
		}

		//If the renderer is disabled, this note was hit
		if (NoteChild.GetComponent<Renderer>().enabled == false)
		{
			return true;	//看位置就是從這裡看
		}

		return true;
	}

	protected void UpdateNotePosition(int index)
	{
		Note note = Player.Song.Notes[index];
		GameObject NoteChild = NoteObjects[index].transform.GetChild(0).gameObject;
		float noteTime = note.Time;
		float playerBeat = Player.GetCurrentBeat();

        if (Player.videoTime < 0.3f)
        {
			return;
			/*
			 * 這就是遊戲開始時音符抖動的原因，遊戲初期要避免音符秀出來
			 * 因為程式從 Video 擷取時間會比較慢
			 * 但是 Update 已經先累加了 SmoothAudioTime
			 * 電腦發現 SmoothAudioTime 領先 Video 時間太多
			 * 所以又把 SmoothAudioTime 歸零
			 * 連續的 領先-歸零 循環在畫面上看起來就像是音符前後震動
			 * 這跟瞬間產生的音符的數量無關
			 */
		}

		//If the note is farther away then 6 beats, its not visible on the neck and we dont have to update it
		if (noteTime < playerBeat + 6)    //該音符的節拍屬性比目前播放中的節拍進度慢超過6拍時，就不再顯示音符
		{
			//If the note is not active, it is visible on the neck for the first time
#if UNITY_4_0
			if( !NoteObjects[ index ].activeSelf )
#else
			if (!NoteObjects[index].activeSelf) //可以改成activeSelf
#endif
			{
				//Activate and show the note
#if UNITY_4_0
				NoteObjects[ index ].SetActive( true );
#else
				NoteObjects[index].SetActive (true); //可以改成activeSelf
#endif
				NoteChild.GetComponent<Renderer>().enabled = true;

			}

			if(noteTime < playerBeat - 1)	//播放器的時間已經超過音符時間 1 拍時，不再更新音符位置
            {
				return;
            }

			//Calculate how far the note has progressed on the neck
			float progress = (noteTime - playerBeat - 7) / 6f;   //吉他遊戲是差 0.5 拍，我改成差 7 拍
																  //這裡面的參數是特別調整過的，更改的話視覺效果會很怪異
			//Update its position
			Vector3 position = NoteObjects[index].transform.position;
			
			position.y = progress * GetYDisplacement() + GetYOffset(Player.Song.Notes[index].StringIndex);	//
			position.z = progress * GetZDisplacement() + GetZOffset(Player.Song.Notes[index].StringIndex);  //
			NoteObjects[index].transform.position = position;

			//這裡面的參數是特別調整過的，更改的話視覺效果會很怪異
		}
	}

    protected bool IsInHitZone(int index) //是否音符在可打擊區域內
	{
		GameObject noteObj = NoteObjects[index];
		Note noteData = Player.Song.Notes[index];
		int stringIndex = noteData.StringIndex;

		return noteObj.transform.position.z < GetHitZoneBeginning(stringIndex)
			&& noteObj.transform.position.z > GetHitZoneEnd(stringIndex);
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
				return NoteStart1.position.y; //參考值 1.49389f + 0.1f
			case 1:
				return NoteStart2.position.y;   //參考值 1.886292f
			case 2:
				return NoteStart3.position.y;    //參考值 1.197582f + 0.1f
			case 3:
				return NoteStart4.position.y;   //參考值 1.357637f
			case 4:
				return NoteStart5.position.y;   //參考值 1.351201f
			case 5:
				return NoteStart6.position.y;    //參考值 1.220011f + 0.1f
			case 6:
				return NoteStart7.position.y;   //參考值 1.731068f
			case 7:
				return NoteStart8.position.y;   //參考值 0.12f
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
				return NoteStart1.position.z;	//參考值 -1.275f
			case 1:
				return NoteStart2.position.z;  //參考值 -0.95f
			case 2:
				return NoteStart3.position.z; //參考值 -1.275f
			case 3:
				return NoteStart4.position.z;  //參考值 -1.15f
			case 4:
				return NoteStart5.position.z;  //參考值 -1.15f
			case 5:
				return NoteStart6.position.z; //參考值 -1.275f
			case 6:
				return NoteStart7.position.z; //參考值 -1.275f
			case 7:
				return NoteStart8.position.z;   //參考值 -1.4f
			default:
				return 0;
		}
	}

	protected GameObject InstantiateNoteFromPrefab(int stringIndex)    //了解
	{
		GameObject note = Instantiate(GetNotePrefab(stringIndex)
									 , GetStartPosition(stringIndex)
									 , GetStartRotation(stringIndex)
									 );

		GameObject NoteChild = note.transform.GetChild(0).gameObject;
		NoteChild.GetComponent<Renderer>().enabled = true;

		//note.GetComponent<Renderer>().material.color = Colors[stringIndex];   //音符不需要上顏色

		return note;
	}

    private GameObject GetNotePrefab(int stringIndex)
    {
		switch (stringIndex)
		{
			case 0:
				return NotePrefab1;
			case 1:
				return NotePrefab2;
			case 2:
				return NotePrefab3;
			case 3:
				return NotePrefab4;
			case 4:
				return NotePrefab5;
			case 5:
				return NotePrefab6;
			case 6:
				return NotePrefab7;
			case 7:
				return NotePrefab8;
			default:
				return NotePrefab1;
		}
    }

    protected Vector3 GetStartPosition(int stringIndex) //了解，呼叫音符的起始座標
	{
		switch (stringIndex)
		{
			//事實證明了，在哪裡產生音符根本不重要，因為起點位置其實是由 update position 來決定的

			case 0:
				return NoteStart1.position; //new Vector3(-0.8881167f, 1.915157f, 2.402217f)
			case 1:
				return NoteStart2.position; //new Vector3(-0.7173551f, 2.307558f, 2.930837f)
			case 2:
				return NoteStart3.position; //new Vector3(-0.532649f, 1.618849f, 2.453747f)
			case 3:
				return NoteStart4.position; //new Vector3(-0.1783119f, 1.778903f, 3.094725f)
			case 4:
				return NoteStart5.position; //new Vector3(0.1466231f, 1.772468f, 3.111839f)
			case 5:
				return NoteStart6.position; //new Vector3(0.5819693f, 1.641277f, 2.504729f)
			case 6:
				return NoteStart7.position; //new Vector3(0.851892f, 2.152334f, 2.714823f)
			case 7:
				return NoteStart8.position; //new Vector3(0.0003564757f, 0.12f, -1.4f)
			default:
				return new Vector3(0f, 0f, 0f);
		}
	}

	protected Quaternion GetStartRotation(int stringIndex)	//我真的不明白為什麼要角度值要正反互換，反正能用就好
    {
		return Quaternion.identity;

		/*	暫時用不到
		switch (stringIndex)
		{
			case 0:
				return Quaternion.identity;    //Quaternion.Euler(0, 0, -54.063f);
			case 1:
				return Quaternion.identity;    //Quaternion.Euler(0, 0, -64.037f);
			case 2:
				return Quaternion.identity;    //Quaternion.Euler(0, 0, -25.313f);
			case 3:
				return Quaternion.identity;    //Quaternion.Euler(0, 0, -19.574f);
			case 4:
				return Quaternion.identity; //Quaternion.Euler(0, 0, 21.351f);
			case 5:
				return Quaternion.identity; //Quaternion.Euler(0, 0, 21.568f);
			case 6:
				return Quaternion.identity; //Quaternion.Euler(0, 0, 56.247f);

			case 7:
				return Quaternion.identity; //Quaternion.Euler(0, 0, 0);
			default:
				return Quaternion.identity;
		}
		*/
	}

	protected float GetHitZoneBeginning(int stringIndex)   // 不同弦上的音符開始可以被打到的位置不同
	{
        switch (stringIndex)
        {
			case 0:   
				return -0.5f;
			case 1:   
				return -0.5f;
			case 2:   
				return -0.5f;
			case 3:   
				return -0.5f;
			case 4:   
				return -0.5f;
			case 5:   
				return -0.5f;
			case 6:   
				return -0.5f;
			case 7:
				return -0.5f;
			default:    //預設值，推測用不到，寫保險的
				return -0.5f;
		}	
	}

	protected float GetHitZoneEnd(int stringIndex) // 不同弦上的音符最後可以被打到的位置不同
	{
		switch (stringIndex)
		{
			case 0:  
				return NoteEnd1.position.z;
			case 1:   
				return NoteEnd2.position.z;
			case 2:  
				return NoteEnd3.position.z;
			case 3:   
				return NoteEnd4.position.z;
			case 4:  
				return NoteEnd5.position.z;
			case 5:   
				return NoteEnd6.position.z;
			case 6:   
				return NoteEnd7.position.z;
			case 7:
				return NoteEnd8.position.z;
			default:    
				return -2f;
		}
	}
}
