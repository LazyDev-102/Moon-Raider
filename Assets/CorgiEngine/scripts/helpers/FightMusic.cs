using UnityEngine;
using System.Collections;

enum MusicState
{
	Up = 1,
	Down = 2
}

public class FightMusic : MonoBehaviour
{
	Coroutine stopper;
	private MusicState _musicState = MusicState.Down;

	// Use this for initialization
	void Start ()
	{
	
	}

	public void Play()
	{
		_musicState = MusicState.Up;
		if (stopper != null) {
			StopCoroutine (stopper);
		}
		stopper = StartCoroutine (Stop (6.0f));
	}

	public virtual IEnumerator Stop(float duration)
	{
		yield return new WaitForSeconds (duration);
		_musicState = MusicState.Down;
	}

	// Update is called once per frame
	void Update ()
	{
		if(_musicState == MusicState.Up)
			SoundManager.Instance.DynamicFightVolume = Mathf.Lerp (SoundManager.Instance.DynamicFightVolume, SoundManager.Instance.MusicVolume + 0.1f, 10*Time.deltaTime); 
		else if(_musicState == MusicState.Down)
			SoundManager.Instance.DynamicFightVolume = Mathf.Lerp (SoundManager.Instance.DynamicFightVolume, 0.0f, 5*Time.deltaTime); 
	}
}

