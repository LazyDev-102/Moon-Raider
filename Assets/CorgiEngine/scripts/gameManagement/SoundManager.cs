using UnityEngine;
using System.Collections;
/// <summary>
/// This persistent singleton handles sound playing
/// </summary>

public class SoundManager : PersistentHumbleSingleton<SoundManager>
{	
	protected AudioSource _backgroundMusic, _fightMusic, _ambient;
	public AudioSource bossMusic;	

	/// true if the music is enabled	
	public bool MusicOn = true;

	/// true if the sound fx are enabled
	public bool SfxOn = true;

	/// the music volume
	[Range(0,1)]
	public float MusicVolume = 0.6f;

	[Range(0,1)]
	public float AmbientVolume = 0.6f;

	[Range(0,1)]
	public float FightVolume = 0.0f;

	public float DynamicFightVolume
	{
		get { return _fightMusic.volume; }
		set { _fightMusic.volume = value; }
	}
		
	/// the sound fx volume
	[Range(0,1)]
	public float SfxVolume = 1f;


    static public float PauseTimePosition = 0;

	/// <summary>
	/// Plays a background music.
	/// Only one background music can be active at a time.
	/// </summary>
	/// <param name="Clip">Your audio clip.</param>
	public virtual void PlayBackgroundMusic(AudioSource Music)
	{
		// if the music's been turned off, we do nothing and exit
		if (!MusicOn)
			return;
		// if we already had a background music playing, we stop it
		if (bossMusic != null)
			bossMusic.Stop();
		
		if (_backgroundMusic!=null)
			_backgroundMusic.Stop();

		// we set the background music clip
		_backgroundMusic = Music;
		// we set the music's volume
		_backgroundMusic.volume = 0;
		// we set the loop setting to true, the music will loop forever
		_backgroundMusic.loop = true;

        // set start time
        _backgroundMusic.time = SoundManager.PauseTimePosition;

        // we start playing the background music
        _backgroundMusic.Play();	
	}

	public virtual void PlayFightMusic(AudioSource Music)
	{
		// if the music's been turned off, we do nothing and exit
		if (!MusicOn)
			return;
		// if we already had a background music playing, we stop it
		if (_fightMusic!=null)
			_fightMusic.Stop();
		// we set the background music clip
		_fightMusic=Music;
		// we set the music's volume
		_fightMusic.volume = 0;
		// we set the loop setting to true, the music will loop forever
		_fightMusic.loop = true;

        // set start time
        _fightMusic.time = SoundManager.PauseTimePosition;

        // we start playing the background music
        _fightMusic.Play();		
	}


    public void SaveMusicPosition()
    {
        SoundManager.PauseTimePosition = _backgroundMusic.time;
    }


    public void ResetMusicPosition()
    {
        SoundManager.PauseTimePosition = 0;
    }


    public void FadeOut()
    {
        StartCoroutine(FadeOut(_backgroundMusic, 0.5f));
        StartCoroutine(FadeOut(_fightMusic, 0.5f));
        StartCoroutine(FadeOut(_ambient, 0.5f));
    }


    public void FadeIn()
    {
        StartCoroutine(FadeIn(_backgroundMusic, 0.5f, Instance.MusicVolume));
        StartCoroutine(FadeIn(_ambient, 0.5f, Instance.AmbientVolume));
    }


    IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
    }

    IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float max)
    {
        audioSource.volume = 0f;
        audioSource.Play();

        while (audioSource.volume < max)
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }

    public virtual void PlayAmbient(AudioSource Ambient)
	{
		// if the music's been turned off, we do nothing and exit
		if (!SfxOn)
			return;
		// if we already had a background music playing, we stop it
		if (_ambient!=null)
			_ambient.Stop();
		// we set the background music clip
		_ambient=Ambient;
		// we set the music's volume
		_ambient.volume = 0;
		// we set the loop setting to true, the music will loop forever
		_ambient.loop = true;
		// we start playing the background music
		_ambient.Play();		
	}

	public virtual void PlayRegularMusic()
	{
		if (bossMusic != null)
			bossMusic.Stop();

		if (_backgroundMusic != null)
			_backgroundMusic.Stop();

		if (_fightMusic != null)
			_fightMusic.Stop();

		PlayBackgroundMusic (_backgroundMusic);
		PlayFightMusic(_fightMusic);
	}

	public virtual void PlayBossMusic()
	{
		// if the music's been turned off, we do nothing and exit
		if (!SfxOn)
			return;
		
		// if we already had a background music playing, we stop it
		if (bossMusic != null)
			bossMusic.Stop();

		if (_backgroundMusic != null)
			_backgroundMusic.Stop();

		if (_fightMusic != null)
			_fightMusic.Stop();

		// we set the music's volume
		bossMusic.volume = 1.0f;

		// we set the loop setting to true, the music will loop forever
		bossMusic.loop = true;

		// we start playing the background music
		bossMusic.Play();		
	}

	public virtual void StopAllMusic()
	{
		if (bossMusic != null)
			bossMusic.Stop();

		if (_backgroundMusic != null)
			_backgroundMusic.Stop();

		if (_fightMusic != null)
			_fightMusic.Stop();
	}
	
	/// <summary>
	/// Plays a sound
	/// </summary>
	/// <returns>An audiosource</returns>
	/// <param name="Sfx">The sound clip you want to play.</param>
	/// <param name="Location">The location of the sound.</param>
	/// <param name="Volume">The volume of the sound.</param>
	public virtual AudioSource PlaySound(AudioClip Sfx, Vector3 Location, bool Loop = false, float Pitch = 1f)
	{
		if (!SfxOn || Sfx == null)
			return null;

		// we create a temporary game object to host our audio source
		GameObject temporaryAudioHost = new GameObject("TempAudio");

		// we set the temp audio's position
		temporaryAudioHost.transform.position = Location;

		// we add an audio source to that host
		AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource; 

		// we set that audio source clip to the one in paramaters
		audioSource.clip = Sfx; 

		// we set the audio source volume to the one in parameters
		audioSource.volume = Instance.SfxVolume;

		// Looping 
		audioSource.loop = Loop;

		// Pitch
		audioSource.pitch = Pitch;

		// we start playing the sound
		audioSource.Play();

		// we destroy the host after the clip has played
		if(!Loop)
			Destroy(temporaryAudioHost, Sfx.length);

		// we return the audiosource reference
		return audioSource;
	}
}
