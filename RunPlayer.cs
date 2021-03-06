using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using System;

public class RunPlayer : MonoBehaviour
{
	public SonataSongData Song;

	protected float SmoothAudioTime = 0f;
	protected bool AudioStopEventFired = false;
	protected bool WasPlaying = false;
	protected bool IsSongPlaying = false;
	public bool SongFinished = false;

	protected VideoPlayer movPlayer;
	public float videoTime;
	public float videoLength = 0;
	public float showSAT;
	//public float showLength;    //可去除
	//public float showLengthInBeats;    //可去除
	public float EndTriggerTime;	//因為 SmoothAudioTime 會自動修正無法累加至遊戲時間終點，所以需要一個不會自動修正的時鐘

	void Start()
	{
		movPlayer = GameObject.Find("Mov").GetComponent<VideoPlayer>();
		videoTime = (float)movPlayer.time;
		//videoLength = (float)movPlayer.clip.length;	//影片時間長度改為事先設定，不再讀取檔案長度
		//showLength = (float)movPlayer.clip.length; //可去除
		//showLengthInBeats = MyMath.SecondsToBeats(showLength, Song.BeatsPerMinute); //可去除
	}

	void Update()
	{
        try
        {
			videoTime = (float)movPlayer.time;
		}
        catch (System.Exception ex)
        {
			videoTime = 1111;
		}


		if (IsPlaying())
		{
			AudioStopEventFired = false;
			WasPlaying = true;
			UpdateSmoothAudioTime();
			//showLengthInBeats = MyMath.SecondsToBeats(showLength, Song.BeatsPerMinute); //可去除
		}

		showSAT = SmoothAudioTime;
	}

	protected void OnSongStopped()
	{
		
		if (!movPlayer.clip)
		{

			return;
		}

		//I want to check if the song has finished playing automatically.
		//Sometimes this is triggered when the song is at the end, 
		//and sometimes it has already been reset to the beginning of the song.
		/*
		if (movPlayer.time == movPlayer.clip.length
		 || (WasPlaying && movPlayer.time == 0))
		{
			IsSongPlaying = false;
			SongFinished = true;
		}
		*/

		IsSongPlaying = false;	//應該要寫 false，但為了讓 smoothaudiotime 繼續跑，先開 true
		SongFinished = true;    //讀取 movPlayer 的時間往往會有些微的秒數落後，造成無法順利結束，只好搬到外面
		GetComponent<DrumGameplay>().PlayAndPause = false;
		GetComponent<DrumGameplay>().StopSong = true;
	}

	protected void UpdateSmoothAudioTime()  //因為audio.time的單位時間比time.deltaTime還要更短，直接取audio.time音符的移動會不順暢
	{
		//Smooth audio time is used because the audio.time has smaller discreet steps and therefore the notes wont move
		//as smoothly. This uses Time.deltaTime to progress the audio time
		SmoothAudioTime += Time.deltaTime;
		EndTriggerTime += Time.deltaTime;

		if (EndTriggerTime >= videoLength) //播放畫面時間大於音檔長度時，遊戲結束
		{
			SmoothAudioTime = videoLength;
			OnSongStopped();
		}

		/*
		if (SmoothAudioTime >= videoLength) //播放畫面時間大於音檔長度時，遊戲結束
		{
			SmoothAudioTime = videoLength;
			OnSongStopped();

		因為 SmoothAudioTime 只要一偏移，就會啟動修正機制，造成歌曲結束後無法累加 SmoothAudioTime 至遊戲結束所需門檻值

		}
		*/

		//Sometimes the audio jumps or lags, this checks if the smooth audio time is off and corrects it
		//making the notes jump or lag along with the audio track


		if (IsSmoothAudioTimeOff())
		{
			CorrectSmoothAudioTime();
		}
	}

	protected bool IsSmoothAudioTimeOff()    //確認是否播放秒數與音檔秒數的差距大於 0.1 秒，有的話會加速或延遲遊戲畫面播放秒數來修正
	{
		//Negative SmoothAudioTime means the songs playback is delayed
		if (SmoothAudioTime < 0)
		{
			return false;
		}

		//Dont check this at the end of the song	//從音檔長度倒數三秒開始就不再檢查，pitch 有調整的話這個參數也要修正才能以正常速度播放遊戲畫面
		if (SmoothAudioTime > videoLength - 3f)
		{
			return false;
		}

		//Check if my smooth time and the actual audio time are of by 0.1
		return Mathf.Abs(SmoothAudioTime - videoTime) > 0.1f;    //想玩慢動作版的話，畫面修正秒數一定要縮短，不然畫面回溯超可怕，但是會加倍運算量
	}

	protected void CorrectSmoothAudioTime() //強制讓畫面播放秒數與音檔播放秒數相等
	{
		SmoothAudioTime = videoTime;
	}

	public void Play()  //不算很懂
	{
		
		SongFinished = false; // 按下播放鈕，表示歌曲還沒播完
		IsSongPlaying = true;
		videoTime = (float)movPlayer.time;
		//videoLength = (float)movPlayer.clip.length;	//影片時間長度改為事先設定，不再讀取檔案長度

		movPlayer.Play();	//影片不播放

		if (SmoothAudioTime < 0)    //負的秒數超過 offset 秒數的話，遊戲會直接開始
		{
			//StartCoroutine(PlayDelayed(Mathf.Abs(SmoothAudioTime)));    //這個秒數直接決定了遊戲畫面要延遲幾秒才下音樂，我先拿掉
			SmoothAudioTime = videoTime;
		}
		else
		{
			SmoothAudioTime = videoTime;
		}
	}

	/* 原本程式碼有，但目前用不到
	protected IEnumerator PlayDelayed(float delay)
	{
		yield return new WaitForSeconds(delay);
	}
	*/

	public void Pause()
	{
		movPlayer.Pause();  //影片暫停
		IsSongPlaying = false;
		SongFinished = false;
	}

	public void Stop()
	{
		
		//movPlayer.Stop();   //影片不停止
		movPlayer.time = 0;	//影片時間軸歸零
		SmoothAudioTime = 0;    //畫面時間軸歸零
		EndTriggerTime = 0; //累加時鐘歸零


		WasPlaying = false;
		IsSongPlaying = false;
		SongFinished = true;
	}

	public bool IsPlaying()
	{
		return IsSongPlaying;
	}

	public void SetSong(SonataSongData song)
	{
		Song = song;
		SmoothAudioTime = MyMath.BeatsToSeconds(-Song.AudioStartBeatOffset, Song.BeatsPerMinute);   //開頭播放前的節拍數轉為秒數取負值，功能未知
	}

	public float GetCurrentBeat(bool songDataEditor = false)    //現在到底是第幾拍：先將播放時間秒數取出轉換為拍數，再扣掉開頭沒聲音的節拍數
	{
		return MyMath.SecondsToBeats(SmoothAudioTime, Song.BeatsPerMinute) - Song.AudioStartBeatOffset;
	}

	public float GetSmoothAudioTime()
    {
		return SmoothAudioTime;
	}


}
