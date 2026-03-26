using UnityEngine;
using System.Collections;

/// <summary>
/// Add this class to a GameObject to have it play a background music when instanciated.
/// Careful : only one background music will be played at a time.
/// </summary>
public class BackgroundMusic : PersistentHumbleSingleton<BackgroundMusic>
{
	public AudioClip MusicClip;
	public AudioClip FightClip;
	public AudioClip AmbientClip;
	public AudioClip BossClip;
    
	protected AudioSource _musicSource;
	protected AudioSource _fightSource;
	protected AudioSource _ambientSource;
	protected AudioSource _bossSource;

	public FightMusic fightMusic;

    /// <summary>
    /// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
    /// </summary>
    protected virtual void Start ()
	{
		fightMusic = gameObject.AddComponent<FightMusic> ();

		_musicSource = gameObject.AddComponent<AudioSource> () as AudioSource;	
		_musicSource.playOnAwake = false;
		_musicSource.spatialBlend = 0;
		_musicSource.rolloffMode = AudioRolloffMode.Logarithmic;
		_musicSource.loop = true;	
	
		_musicSource.clip = MusicClip;

		_fightSource = gameObject.AddComponent<AudioSource> () as AudioSource;	
		_fightSource.playOnAwake = false;
		_fightSource.spatialBlend = 0;
		_fightSource.rolloffMode = AudioRolloffMode.Logarithmic;
		_fightSource.loop = true;	

		_fightSource.clip = FightClip;

		_ambientSource = gameObject.AddComponent<AudioSource> () as AudioSource;	
		_ambientSource.playOnAwake = false;
		_ambientSource.spatialBlend = 0;
		_ambientSource.rolloffMode = AudioRolloffMode.Logarithmic;
		_ambientSource.loop = true;	

		_ambientSource.clip = AmbientClip;

		_bossSource = gameObject.AddComponent<AudioSource> () as AudioSource;	
		_bossSource.playOnAwake = false;
		_bossSource.spatialBlend = 0;
		_bossSource.rolloffMode = AudioRolloffMode.Logarithmic;
		_bossSource.loop = true;	

		_bossSource.clip = BossClip;

		SoundManager.Instance.PlayBackgroundMusic(_musicSource);
		SoundManager.Instance.PlayFightMusic(_fightSource);
		SoundManager.Instance.PlayAmbient(_ambientSource);
		SoundManager.Instance.bossMusic = _bossSource;
	}
}
