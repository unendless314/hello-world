using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[Serializable]
public class Note
{
	public float Time;  //單位是BPM而不是秒，也就是位於整首歌中的第幾拍，非常重要
	public int StringIndex; //在哪條弦上
	public float Length = 0;    //短音通通秒數為零，長音才有秒數
}

public class SonataSongData : ScriptableObject
{
	public string Name;
	public string Band;

	public float BeatsPerMinute;    //這首歌的BPM數多少，越多越快
	public float AudioStartBeatOffset;  //開頭沒聲音的節拍數

	public AudioClip BackgroundTrack;

	[HideInInspector]
	public List<Note> Notes = new List<Note>();

	public SonataSongData()
	{

	}

	public int GetNoteIndex(float time, int stringIndex)    //不懂但似乎很重要，預設是取 -1，然後前面的音符跳過
	{                                                       //第幾拍的第幾弦上是不是有音符，有的話，回傳該音符所在的節拍數屬性
		for (int i = 0; i < Notes.Count; ++i)
		{
			if (Notes[i].Time < time)
			{
				continue;
			}

			if (Notes[i].Time == time && Notes[i].StringIndex == stringIndex)
			{
				return i;
			}

			if (Notes[i].Time > time)
			{
				return -1;
			}
		}
		return -1;
	}

	public void RemoveNote(int index)   //應該是移除音符集合中的元素
	{
		if (index >= 0 && index < Notes.Count)
		{
			Notes.Remove(Notes[index]);
		}
	}

	public void RemoveNote(float time, int stringIndex)    //overload，應該是移除音符集合中的元素
	{
		int index = GetNoteIndex(time, stringIndex);

		if (index != -1)    //預設期望值為 -1 就不會被移除
		{
			RemoveNote(index);
		}
	}

	public int AddNote(float time, int stringIndex, float length = 0f)    //不懂但應該重要
	{
		if (time > GetLengthInBeats())
		{
			return -1;
		}

		//我猜，如果追加的音符元素的節拍數屬性超過整首歌曲的節拍總數時，追加音符會失敗

		Note newNote = new Note();

		newNote.Time = time;
		newNote.StringIndex = stringIndex;
		newNote.Length = length;

		//Find correct position in the list so that the list remains ordered	//不懂但應該重要
		for (var i = 0; i < Notes.Count; ++i)                                   //插入新增的音符?
		{
			if (Notes[i].Time > time)
			{
				Notes.Insert(i, newNote);
				return i;
			}
		}

		//If note wasn't inserted in the list, it will be added at the end	
		Notes.Add(newNote);           //如果沒有 insert 就會被 add，索引值當然是更新後陣列長度 -1
		return Notes.Count - 1;
	}

	public float GetLengthInSeconds()   //取得AudioClip的時間秒數，否則為0
	{
		if (BackgroundTrack)
		{
			return BackgroundTrack.length;
		}

		return 0;
	}

	public float GetLengthInBeats() //將AudioClip的時間秒數轉換為節拍數量
	{
		return GetLengthInSeconds() * BeatsPerMinute / 60;
	}
}
