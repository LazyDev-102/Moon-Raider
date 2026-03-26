using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    public AudioClip ChargeSfx;
    public AudioClip SpeakSfx;

    public float OpenDistance = 400.0f;

    int stolenGemCount;
    private Health health;

	// Use this for initialization
	void Start ()
	{
        health = GetComponent<Health>();
    }

    public bool CheckDefeated()
    {
        if (LevelVariables.bossDefeated)
        {
            gameObject.SetActive(false);

            // If for some reason we don't pick up the melee upgrade after beating a boss, make sure it appears again
            if(GlobalVariables.WorldIndex == 1 && GlobalVariables.LevelIndex == 6 && !GlobalVariables.CanMelee)
            {
                var partPrefab = Resources.Load("Items/PartMelee") as GameObject;
                Instantiate(partPrefab, transform.position, Quaternion.identity);
            }
        }

        return LevelVariables.bossDefeated;
    }
	
	// Update is called once per frame
	void Update ()
	{
	
	}


    public void Say(int something = 0, int duration = 3)
    {
        if (SpeakSfx != null)
            SoundManager.Instance.PlaySound(SpeakSfx, transform.position);

        AISayThings sayThings = GetComponent<AISayThings>();

        if (sayThings != null)
            sayThings.SaySomething(something, duration);
    }


    public void SuckUpGems()
    {
        GameManager.Instance.Player.Drain();

        var gemPrefab = Resources.Load("Items/GemBoss") as GameObject;

        stolenGemCount = GameManager.Instance.Points;

        LevelVariables.stolenGems = stolenGemCount;
        GameManager.Instance.AddPoints(-stolenGemCount);

        int total = (int)stolenGemCount / 10;

        while (total > 10)
            total /= 10;

        for (var n = 0; n < total; n++)
        {
            Vector3 pos = GameManager.Instance.Player.transform.position;
            float x = (float)Random.Range(-20, 20) / 40;
            pos.x += x;

            int type = Random.Range(70, 75);
            GameObject gemObj = Instantiate(gemPrefab, pos, gameObject.transform.rotation);
            gemObj.name = "PopGem" + type;
            gemObj.transform.parent = gameObject.transform.parent;

            Coin coin = gemObj.GetComponent<Coin>();
            coin.CollidesWithPlayer = false;

            Gem gem = gemObj.GetComponent<Gem>();
            gem.TracksPlayer = false;
            gem.Init(type);

            gem.StartCoroutine(gem.Collect());

            Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D>();
            rigid.gravityScale = 0.5f;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1, 5) < 3)
                rigid.AddForce(new Vector2(3.0f, 2 * Random.Range(3, 5)), ForceMode2D.Impulse);
            else
                rigid.AddForce(new Vector2(-3.0f, 2 * Random.Range(3, 5)), ForceMode2D.Impulse);
        }
    }


    public void ReturnGems()
    {
        GameManager.Instance.AddPoints(LevelVariables.stolenGems);
    }


	public void CloseWalls()
	{
        StartCoroutine(GUIManager.Instance.SlideBarsIn());
        GameManager.Instance.FreezeCharacter();

        var maskLayer = 1 << LayerMask.NameToLayer ("Foreground");
		RaycastHit2D[] circles = Physics2D.CircleCastAll (transform.localPosition, 400.0f, Vector2.right, 0.0f, maskLayer);

		float order = 0.25f;
		for(int i = 0; i < circles.Length; i++)
		{
			var circle = circles [i];

			var wall = circle.collider.gameObject.GetComponent<BossWall> ();

			if (wall != null) {
				wall.Order = order;
				order += 0.25f;
				wall.Activate ();
			}
		}
	}


	public void OpenWalls()
	{
		var maskLayer = 1 << LayerMask.NameToLayer ("Platforms");
		RaycastHit2D[] circles = Physics2D.CircleCastAll (transform.localPosition, OpenDistance, Vector2.right, 0.0f, maskLayer);

		float order = 3.0f;
		for (int i = 0; i < circles.Length; i++) {
			var circle = circles [i];

			var wall = circle.collider.gameObject.GetComponent<BossWall> ();

			if (wall != null) {
				wall.Order = order;
				order += 0.125f;
				wall.Deactivate ();
			}
		}

		CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
		sceneCamera.StartCoroutine (sceneCamera.SpeedUp (2f));
        sceneCamera.LockX = false;

        LevelVariables.bossDefeated = true;
        ReturnGems();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Gem>() == null)
            return;

        if (health != null)
        {
            if (health.Flickering == false)
            {
                GetComponent<SpriteRenderer>().color = Color.yellow;
                health.StartCoroutine(health.Flicker());

                if (ChargeSfx != null)
                    SoundManager.Instance.PlaySound(ChargeSfx, transform.position);
            }
        }
    }
}

