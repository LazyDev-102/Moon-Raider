using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class Part : MonoBehaviour
{
	public float FallSpeed = 0.75f;
    public AudioClip Sound;
    public GameObject Effect;

    public enum UpgradeMode { MaxHealth, MaxGems, MaxWeapon, EnableMelee };
    public UpgradeMode UpgradeType = UpgradeMode.MaxHealth;

    // private stuff
    protected Vector2 _upPosition, _downPosition, _orgPosition, _newPosition;

	bool down = true;


    protected void Start()
    {
        _orgPosition = transform.position;
        resetUpDown();

        if (UpgradeType != UpgradeMode.EnableMelee)
        {
            var hiddenPrefab = Resources.Load("Obstacles/HiddenArea" + GlobalVariables.WorldIndex) as GameObject;
            var hidden = Instantiate(hiddenPrefab, Vector3.zero, Quaternion.identity);
            hidden.transform.parent = transform.parent;

            float x = transform.position.x;
            float y = transform.position.y;

            switch (GlobalVariables.WorldIndex)
            {
                case 1:
                    hidden.transform.position = new Vector3(x + 4, y + 4, 7f);
                    break;
                case 2:
                    hidden.transform.position = new Vector3(x + 6, y + 4, 7f);
                    break;
                case 3:
                    hidden.transform.position = new Vector3(x + 8, y + 2, 7f);
                    break;
                case 4:
                    hidden.transform.position = new Vector3(x + 6, y + 4, 7f);
                    break;
                case 5:
                    hidden.transform.position = new Vector3(x + 6, y + 4, 7f);
                    break;
                case 6:
                    hidden.transform.position = new Vector3(x, y + 4, 10f);
                    break;
                case 7:
                    hidden.transform.position = new Vector3(x, y + 4, 10f);
                    break;
                case 8:
                    hidden.transform.position = new Vector3(x + 2, y, 10f);
                    break;
                case 9:
                    hidden.transform.position = new Vector3(x + 4, y + 2, 10f);
                    break;
                case 10:
                    hidden.transform.position = new Vector3(x + 4, y + 2, 10f);
                    break;
            }
        }
    }


    private void resetUpDown ()
	{
		FallSpeed = 0.75f;
		_upPosition = transform.position + 0.2f*Vector3.up;
		_downPosition = transform.position + 0.2f*Vector3.down;
	}


	/// <summary>
	/// This is called every frame.
	/// </summary>
	protected virtual void FixedUpdate()
	{
		if (down) {
			_newPosition = new Vector2 (0, -FallSpeed * Time.deltaTime);

			if (transform.position.y < _downPosition.y) {
				down = false;
			}
		} else {
			_newPosition = new Vector2 (0, FallSpeed * Time.deltaTime);

			if (transform.position.y > _upPosition.y) {
				down = true;
			}
		}

		transform.Translate(_newPosition,Space.World);
	}


	/// <summary>
	/// Triggered when a CorgiController touches the platform
	/// </summary>
	/// <param name="controller">The corgi controller that collides with the platform.</param>
	public virtual void OnTriggerEnter2D(Collider2D collider)
	{
        if (LevelVariables.partFound)
            return;

		CorgiController controller = collider.GetComponent<CorgiController>();

		if (controller == null)
			return;

        if(Sound != null)
            SoundManager.Instance.PlaySound(Sound, transform.position, false);

        LevelVariables.partFound = true;
        GameManager.Instance.Player.BehaviorState.MeleeEnergized = false;

        if (UpgradeType == UpgradeMode.MaxHealth)
        {
            GameManager.Instance.Player.BehaviorParameters.MaxHealth += 1;
            GameManager.Instance.Player.Health = GameManager.Instance.Player.BehaviorParameters.MaxHealth;

            if (GameManager.Instance.PlayerTwo != null)
                GameManager.Instance.PlayerTwo.Health = 5;

            GUIManager.Instance.StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.MoreHealth));
        }
        else if (UpgradeType == UpgradeMode.MaxGems)
        {
            GameManager.Instance.Player.BehaviorParameters.MaxGems += 100;
            GameManager.Instance.AddPoints(50);

            GUIManager.Instance.StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.MoreGems));
        }
        else if (UpgradeType == UpgradeMode.MaxWeapon)
        {
            GameManager.Instance.Player.BehaviorParameters.MaxWeapon += 1;

            int maxWeapon = GameManager.Instance.Player.shoot.LastWeaponIndex;

            if (GameManager.Instance.Player.BehaviorParameters.MaxWeapon > maxWeapon)
                GameManager.Instance.Player.BehaviorParameters.MaxWeapon = maxWeapon;

            GameManager.Instance.Player.shoot.WeaponIndex = GameManager.Instance.Player.BehaviorParameters.MaxWeapon;
            GameManager.Instance.Player.shoot.MaxWeaponIndex = GameManager.Instance.Player.shoot.WeaponIndex;
            GameManager.Instance.Player.shoot.ChangeWeapon(GameManager.Instance.Player.shoot.Weapons[GameManager.Instance.Player.shoot.MaxWeaponIndex]);

            GUIManager.Instance.StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.MoreDamage));
        }
        else if (UpgradeType == UpgradeMode.EnableMelee)
        {
            GameManager.Instance.Player.BehaviorState.CanMelee = true;

            if(GameManager.Instance.PlayerTwo != null)
                GameManager.Instance.PlayerTwo.BehaviorState.CanMelee = true;

            GUIManager.Instance.StartCoroutine(GUIManager.Instance.ChatBar.ChatStart(1f, ChatBox.SpeakMode.NewMelee));
        }

        GameManager.Instance.Player.StartCoroutine(GameManager.Instance.Player.Powerup());

        // adds an instance of the effect at the coin's position
        if (Effect != null)
        {
            var fx = Instantiate(Effect, transform.position, transform.rotation);
            Destroy(fx, fx.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }

        AnalyticsEvent.Custom("upgrade_found", new Dictionary<string, object>
        {
            { "level", GlobalVariables.LevelIndex },
            { "world", GlobalVariables.WorldIndex },
            { "type", UpgradeType.ToString() },
            { "time_elapsed", Time.timeSinceLevelLoad }
        });

        string achievement = "mr.achievements.upgradefound" + GlobalVariables.WorldIndex;
        LevelManager.Instance.SaveAchievement(achievement);

        // we desactivate the gameobject
        gameObject.SetActive(false);
    }
}

