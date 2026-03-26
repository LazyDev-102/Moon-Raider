using UnityEngine;
using System.Collections;
/// <summary>
/// Coin manager
/// </summary>
public class Coin : MonoBehaviour, IPlayerRespawnListener
{
    public int Special = 0;
    public int SaveIndex = -1;


    /// The effect to instantiate when the coin is hit
    public GameObject Effect;
    public AudioClip Sound;

    /// The amount of points to add when collected
    public int PointsToAdd = 0;
    public bool CollidesWithPlayer = true;

    /// <summary>
    /// Triggered when something collides with the coin
    /// </summary>
    /// <param name="collider">Other.</param>
    public virtual void OnTriggerEnter2D(Collider2D collider)
    {
        CharacterBehavior player = collider.GetComponent<CharacterBehavior>();

        // if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
        if ((player != null && !CollidesWithPlayer) || (player == null && CollidesWithPlayer))
        {
            return;
        }

        Boss boss = collider.GetComponent<Boss>();

        if (player == null && boss == null)
            return;

		if (Sound != null)
        {
            int pts = (int)PointsToAdd;
            if (pts > 20)
                pts /= 10;

			float pitch = 1 + pts / 100;
			SoundManager.Instance.PlaySound (Sound, transform.position, false, pitch);
		}

		// We pass the specified amount of points to the game manager
		if (CollidesWithPlayer)
		{
            if(player.BehaviorState.MeleeEnergized)
				GameManager.Instance.AddPointsInstant(PointsToAdd);
            else
			    GameManager.Instance.AddPoints(PointsToAdd);

            if (Special > 0)
            {
                GameManager.Instance.AddSpecialPoints(Special);

                if (GameManager.Instance.SpecialPoints == 1)
                    GUIManager.Instance.StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.SpecialGems));
                else if (GameManager.Instance.SpecialPoints == 200)
                    GUIManager.Instance.StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.SpecialGemsMax));
            }

            if (GlobalVariables.ForceLevelNumber < 1000 && SaveIndex >= 0)
                LevelVariables.specialGemState[SaveIndex] = true;
        }

		// adds an instance of the effect at the coin's position
		if (Effect != null) {
			var fx = Instantiate (Effect, transform.position, transform.rotation);
			Destroy (fx, fx.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).length);
		}

		// we desactivate the gameobject
		gameObject.SetActive(false);
	}

	/// <summary>
	/// When the player respawns, we reinstate the object
	/// </summary>
	/// <param name="checkpoint">Checkpoint.</param>
	/// <param name="player">Player.</param>
	public virtual void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, CharacterBehavior player)
	{
		gameObject.SetActive(true);
	}
}
