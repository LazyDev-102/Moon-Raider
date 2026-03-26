//Made By TuningMania!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour {

	[Header("Variables:")]
	public int StartFrom=3;
	private int StartCount;
	[Tooltip("Delay between countdown in seconds")]
	public float Delay=1f;
	[Header("Details:")]
	public bool StartAuto = true;
	public string StartText = "GO!";
	[Header("UI Elements:")]
	public Text CountText;
	[Header("Audio:")]
	public AudioSource Source;
	public AudioClip CountClip;
	public AudioClip StartClip;


	void Start()
	{
		Reset();
	}

    public void Reset()
	{
		//set this int for a better result
		StartCount = StartFrom;

		CountText.fontSize = 128;

		//get audiosource if empty
		if(Source==null)
		{
		    Source=gameObject.GetComponent<AudioSource>();
		}

		//check if starts automatically is selected
		if (StartAuto == true)
		{
    		//this function starts the countdown
    		StartCoroutine(Countdown(StartFrom));
		}
	}

	public IEnumerator Countdown(int seconds)
	{

		while (StartFrom > 0) {
			//update UI
			CountText.text=StartFrom.ToString();
			//make sound
			Source.clip=CountClip;
			Source.Play ();
			// remove a step from countdown: for example from 3 to 2
			yield return new WaitForSeconds(Delay);
			StartFrom --;
		}

		if (StartText != "") {
			//show message if it's setted
			CountText.text = StartText;
			//make sound
			Source.clip = StartClip;
			Source.Play ();
			//wait to clean countdown
			yield return new WaitForSeconds(Delay);
			//remove countdown
			CountText.text = "";
		}
		else CountText.text = "";

		// count down is finished, start your void
		StartVoid();
	}

	//the void to start after countdown
	void StartVoid()
	{
		//insert here your void
		//GUIManager.Instance.TrackTime = true;
	}

	//if you want start your countdown from UI element
	public void StartManually(){
		//change to select the int which start
		StartFrom = StartCount;
		//start countdown
		StartCoroutine(Countdown(StartFrom));
	}

}
