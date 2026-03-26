using UnityEngine;
using System.Collections;

public class Gem : MonoBehaviour
{
	static Sprite[] lights;
	private bool _collecting = false;

	public float CollectSpeed = 1f;
    public bool RandomColor = false;

	private GameObject _target;
    private Animator _animator;

    public bool TracksPlayer = true;
    public bool Tracks = true;
    public bool IsStatic = false;


    public virtual void Awake()
	{
        _animator = GetComponent<Animator>();

        CircleCollider2D circle = GetComponent<CircleCollider2D>();

        if(circle != null)
            circle.enabled = IsStatic;
        
		if(lights == null)
			lights = Resources.LoadAll <Sprite> ("lights");

        if (RandomColor)
        {
            int type = Random.Range(1, 4);

            Gem gem = GetComponent<Gem>();
            gem.Init(type);

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            if (type == 1)
                renderer.color = Color.magenta;
            else if (type == 2)
                renderer.color = Color.cyan;
            else if (type == 3)
                renderer.color = Color.yellow;
        }

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        if (rigid)
        {
            rigid.gravityScale = 0;
            rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }


	public static void popIntoGems(GameObject gameObject, int count)
	{
		GameManager.Instance.Player.Hiccup (0.01f);

		var gemPrefab = Resources.Load ("Items/Gem") as GameObject;

		for (var n = 0; n < count; n++)
		{
            Vector3 pos = gameObject.transform.position;
            float x = (float)Random.Range(-20, 20) / 40;
            pos.x += x;

            int type = Random.Range(70, 75);
            GameObject gemObj = Instantiate(gemPrefab, pos, gameObject.transform.rotation);
            gemObj.name = "PopGem" + type;
			gemObj.transform.parent = gameObject.transform.parent;

			Gem gem = gemObj.GetComponent<Gem> ();
            gem.TracksPlayer = true;
            gem.IsStatic = false;
            gem.Init (type);

            Coin coin = gemObj.GetComponent<Coin>();
            coin.CollidesWithPlayer = true;

            gem.StartCoroutine(gem.Collect ());

			Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D> ();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1,5) < 3) 
				rigid.AddForce(new Vector2(60.0f, 2*Random.Range (3, 5)),  ForceMode2D.Impulse);
			else
				rigid.AddForce(new Vector2(-60.0f, 2*Random.Range (3, 5)),  ForceMode2D.Impulse);
		}
	}

	public static void popIntoShards(GameObject gameObject, int count)
	{
		GameManager.Instance.Player.Hiccup (0.0075f);

		var gemPrefab = Resources.Load ("Items/Shard") as GameObject;

		for (var n = 0; n < count; n++)
		{
			GameObject gemObj = GameObject.Instantiate (gemPrefab, gameObject.transform.position, gameObject.transform.rotation);
			gemObj.name = "Shard";
			gemObj.transform.parent = gameObject.transform.parent;

			Gem gem = gemObj.GetComponent<Gem> ();
            gem.TracksPlayer = true;
            gem.StartCoroutine(gem.Collect ());

			Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D> ();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1,5) < 3) 
				rigid.AddForce(new Vector2(Random.Range (1, 3), 2*Random.Range (1, 3)),  ForceMode2D.Impulse);
			else
				rigid.AddForce(new Vector2(-Random.Range (1, 3), 2*Random.Range (1, 3)),  ForceMode2D.Impulse);

			Gem.Sparkles (gameObject);
		}
	}

    public static void popIntoIceShards(GameObject gameObject, int count)
    {
        GameManager.Instance.Player.Hiccup(0.0075f);

        var gemPrefab = Resources.Load("Items/IceShard") as GameObject;

        for (var n = 0; n < count; n++)
        {
            GameObject gemObj = GameObject.Instantiate(gemPrefab, gameObject.transform.position, gameObject.transform.rotation);
            gemObj.name = "IceShard";
            gemObj.transform.parent = gameObject.transform.parent;

            Gem gem = gemObj.GetComponent<Gem>();
            gem.TracksPlayer = true;
            gem.StartCoroutine(gem.Collect());

            Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D>();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1, 5) < 3)
                rigid.AddForce(new Vector2(Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);
            else
                rigid.AddForce(new Vector2(-Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);

            Gem.Sparkles(gameObject);
        }
    }

    public static void popIntoPinkShards(GameObject gameObject, int count)
    {
        GameManager.Instance.Player.Hiccup(0.0075f);

        var gemPrefab = Resources.Load("Items/PinkShard") as GameObject;

        for (var n = 0; n < count; n++)
        {
            GameObject gemObj = GameObject.Instantiate(gemPrefab, gameObject.transform.position, gameObject.transform.rotation);
            gemObj.name = "Shard";
            gemObj.transform.parent = gameObject.transform.parent;

            Gem gem = gemObj.GetComponent<Gem>();
            gem.TracksPlayer = true;
            gem.StartCoroutine(gem.Collect());

            Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D>();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1, 5) < 3)
                rigid.AddForce(new Vector2(Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);
            else
                rigid.AddForce(new Vector2(-Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);

            Gem.Sparkles(gameObject);
        }
    }

    public static void popIntoPurpleShards(GameObject gameObject, int count)
    {
        GameManager.Instance.Player.Hiccup(0.0075f);

        var gemPrefab = Resources.Load("Items/PurpleShard") as GameObject;

        for (var n = 0; n < count; n++)
        {
            GameObject gemObj = GameObject.Instantiate(gemPrefab, gameObject.transform.position, gameObject.transform.rotation);
            gemObj.name = "Shard";
            gemObj.transform.parent = gameObject.transform.parent;

            Gem gem = gemObj.GetComponent<Gem>();
            gem.TracksPlayer = true;
            gem.StartCoroutine(gem.Collect());

            Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D>();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1, 5) < 3)
                rigid.AddForce(new Vector2(Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);
            else
                rigid.AddForce(new Vector2(-Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);

            Gem.Sparkles(gameObject);
        }
    }

    public static void popIntoMiniGems(GameObject gameObject, int count)
	{
        if (GameManager.Instance.Player == null)
            return;

		GameManager.Instance.Player.Hiccup (0.005f);

		var gemPrefab = Resources.Load ("Items/MiniGem") as GameObject;

		for (var n = 0; n < count; n++)
		{
			int type = Random.Range (1, 4);
			GameObject gemObj = GameObject.Instantiate (gemPrefab, gameObject.transform.position, gameObject.transform.rotation);
			gemObj.name = "MiniPopGem" + type;
			gemObj.transform.parent = gameObject.transform.parent;

			Gem gem = gemObj.GetComponent<Gem> ();
            gem.TracksPlayer = true;
            gem.Init (type);

			SpriteRenderer renderer = gemObj.GetComponent<SpriteRenderer> ();

			if (type == 1)
				renderer.color = Color.magenta;
			else if (type == 2)
				renderer.color = Color.cyan;
			else if (type == 3)
				renderer.color = Color.yellow;

            if (gameObject == GameManager.Instance.Player.gameObject)
            {
                Coin coin = gemObj.GetComponent<Coin>();

                if (coin != null)
                    coin.CollidesWithPlayer = false;
            }

            gem.StartCoroutine(gem.Collect ());

			Light light = gemObj.GetComponent<Light> ();
			light.color = renderer.color;

			SpriteRenderer fakeLight = gemObj.GetComponentsInChildren<SpriteRenderer> () [1];
			fakeLight.color = new Color (renderer.color.r, renderer.color.g, renderer.color.b, 0.25f);

			Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D> ();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1,5) < 3) 
				rigid.AddForce(new Vector2(Random.Range (1, 3), 2*Random.Range (1, 3)),  ForceMode2D.Impulse);
			else
				rigid.AddForce(new Vector2(-Random.Range (1, 3), 2*Random.Range (1, 3)),  ForceMode2D.Impulse);

			Gem.Sparkles (gameObject);
		}
	}

	public static void popIntoHealth(GameObject gameObject, int count)
	{
		GameManager.Instance.Player.Hiccup (0.0075f);

		var healthPrefab = Resources.Load ("Items/HealthBubble") as GameObject;

		for (var n = 0; n < count; n++)
		{
			GameObject gemObj = GameObject.Instantiate (healthPrefab, gameObject.transform.position, gameObject.transform.rotation);
			gemObj.name = "HealthBubble";
			gemObj.transform.parent = gameObject.transform.parent;

			Gem gem = gemObj.GetComponent<Gem> ();
            gem.TracksPlayer = true;
            gem.StartCoroutine(gem.Collect (0.25f, true));

			Rigidbody2D rigid = gemObj.GetComponent<Rigidbody2D> ();
            rigid.gravityScale = 1;
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (Random.Range(1,5) < 3) 
				rigid.AddForce(new Vector2(Random.Range (1, 3), 2*Random.Range (1, 3)),  ForceMode2D.Impulse);
			else
				rigid.AddForce(new Vector2(-Random.Range (1, 3), 2*Random.Range (1, 3)),  ForceMode2D.Impulse);

			Gem.Sparkles (gameObject);
		}
	}

	public static IEnumerator popIntoMagic(GameObject gameObject, float delay)
	{
		yield return new WaitForSeconds(delay);

		var magicPrefab = Resources.Load ("FX/MagicRing") as GameObject;

		Vector2[] angles = {
			new Vector2 (0, 1),
			new Vector2 (0, -1),
			new Vector2 (1, 0),
			new Vector2 (-1, 0),
			new Vector2 (1, 1),
			new Vector2 (1, -1),
			new Vector2 (-1, 1),
			new Vector2 (-1, -1)
		};

		for (var n = 0; n < 8; n++)
		{
			Vector3 offset = Vector3.zero;

			if (n == 0)
				offset = Vector3.up;
			else if (n == 1)
				offset = Vector3.down;
			else if (n == 2)
				offset = Vector3.right;
			else if (n == 3)
				offset = Vector3.left;
			
			GameObject magicObj = GameObject.Instantiate (magicPrefab, gameObject.transform.position + 2*offset, gameObject.transform.rotation);
			magicObj.name = "MagicRing";
			magicObj.transform.parent = gameObject.transform.parent;

			MagicRing mr = magicObj.GetComponent<MagicRing> ();
			mr.Direction = angles [n];
		}
	}

	public static void Sparkles(GameObject gameObject)
	{
		var sparklePrefab = Resources.Load ("FX/Sparkle") as GameObject;

		for (var o = 0; o < 2; o++) {
			GameObject sparkleObj = GameObject.Instantiate (sparklePrefab, gameObject.transform.position, gameObject.transform.rotation);
			sparkleObj.transform.parent = gameObject.transform.parent;

			Rigidbody2D rigid2 = sparkleObj.GetComponent<Rigidbody2D> ();

			if (Random.Range (1, 5) < 3)
				rigid2.AddForce (new Vector2 (Random.Range (1, 3), 2 * Random.Range (1, 3)), ForceMode2D.Impulse);
			else
				rigid2.AddForce (new Vector2 (-Random.Range (1, 3), 2 * Random.Range (1, 3)), ForceMode2D.Impulse);

			Destroy (sparkleObj, sparkleObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
		}
	}
		
	// Use this for initialization
	public void Init (int type)
	{
        //Debug.Log ("Init gem " + type);
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if(collider != null)
            collider.enabled = IsStatic;

        GameObject boss = GameObject.FindGameObjectWithTag("Boss");

        if (boss != null && !TracksPlayer)
        {
            if (boss.activeSelf)
                _target = boss;
        }

        if (_target == null && GameManager.Instance.Player != null)
        {
            _target = GameManager.Instance.Player.gameObject;

            if(GlobalVariables.TwoPlayer && GameManager.Instance.PlayerTwo != null)
            {
                float magnitude1 = (GameManager.Instance.Player.transform.position - transform.position).magnitude;
                float magnitude2 = (GameManager.Instance.PlayerTwo.transform.position - transform.position).magnitude;

                // Player two can heal self if close and not at 100% health
                if(magnitude2 < magnitude1 && GameManager.Instance.PlayerTwo.Health < 5)
                    _target = GameManager.Instance.PlayerTwo.gameObject;
            }
        }

        Animator gemAnimator = GetComponent<Animator> ();

		if (gemAnimator != null) 
		{
            bool hasLotsOfGems = (GameManager.Instance.Points > GlobalVariables.MaxGems / 2);
            int rubberValue = hasLotsOfGems ? 3 : 5;

            int gemType = type - 69;
			int value = IsStatic ? (15 - gemType) : rubberValue;

            if (value == 0)
                value = 1;

			Color gemColor = Color.white;

			// Dumb compatibility hack with existing levels
			if (gemType == 6) {
				gemType = 7;
				value = hasLotsOfGems ? 25 : 50;
			} else if (gemType == 7) {
				gemType = 6;
			} else if (gemType == -1) {
				gemType = 8;
				value = hasLotsOfGems ? 35 : 70;
			} else if (gemType == 0) {
				gemType = 9;
				value = hasLotsOfGems ? 45 : 90;
			}

			gemColor = getColor (gemType);

			gemAnimator.SetInteger ("Type", gemType);

			Coin coin = GetComponent<Coin> ();

			if (coin != null)
				coin.PointsToAdd = value;

			Light gemLight = GetComponent<Light> ();

			if (gemLight != null) {
				gemLight.color = gemColor;
				gemLight.intensity = 2.0f;
				gemLight.range = 12f;

				SpriteRenderer fakeGemLight = gameObject.GetComponentsInChildren<SpriteRenderer> () [1];
				fakeGemLight.color = new Color (gemColor.r, gemColor.g, gemColor.b, 0.25f);
			}
		}
	}

	public Color getColor(int gemType)
	{
		Color gemColor = Color.white;

		// Now get colors
		switch(gemType)
		{
		case 1:
			gemColor = Color.blue;
			break;
		case 2:
			gemColor = Color.green;
			break;
		case 3:
			gemColor = Color.yellow;
			break;
		case 4:
			gemColor = Color.blue;
			break;
		case 5:
			gemColor = Color.red;
			break;
		case 6:
			gemColor = Color.green;
			break;
		case 7:
			gemColor = Color.white;
			break;
		case 8:
			gemColor = Color.yellow;
			break;
		case 9:
			gemColor = Color.red;
			break;
		}

		return gemColor;
	}

	/// <summary>
	/// Coroutine used to make the character's sprite flicker (when hurt for example).
	/// </summary>
	public virtual IEnumerator Collect(float time = 1.0f, bool twoPlayer = false)
	{
        IsStatic = false;

        if (_target == null && GameManager.Instance.Player != null)
        {
            _target = GameManager.Instance.Player.gameObject;

            if(twoPlayer && GameManager.Instance.PlayerTwo != null)
            {
                float p1Health = (float)GameManager.Instance.Player.Health / (float)GameManager.Instance.Player.BehaviorParameters.MaxHealth;
                float p2Health = (float)GameManager.Instance.PlayerTwo.Health / 5.0f;

                if(p2Health < p1Health)
                    _target = GameManager.Instance.PlayerTwo.gameObject;
            }
        }

        Rigidbody2D rigid = GetComponent<Rigidbody2D> ();
		CircleCollider2D circle = GetComponent<CircleCollider2D> ();

		yield return new WaitForSeconds(time);

        Coin coin = GetComponent<Coin>();
        if (coin != null)
        {
            bool playerTwoColliding = false;

            if (GameManager.Instance.PlayerTwo != null)
                playerTwoColliding = (_target == GameManager.Instance.PlayerTwo.gameObject);

            coin.CollidesWithPlayer = (_target == GameManager.Instance.Player.gameObject || playerTwoColliding);
        }

        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
		circle.enabled = IsStatic;

		yield return new WaitForSeconds(0.1f);

		rigid.gravityScale = 0;
		rigid.constraints = RigidbodyConstraints2D.None;

		float r = Random.Range (1, 2) / 2;
		yield return new WaitForSeconds(r);

        circle.enabled = true;
        _collecting = true;
		rigid.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
	}

	// Update is called once per frame
	void Update ()
	{
        if (IsStatic)
            return;

        if(!Tracks)
        {
            Vector3 nt = transform.position;
            nt.x += 0.75f * Time.deltaTime;
            transform.position = nt;
            return;
        }

        if (!_target)
        {
            _target = GameManager.Instance.Player.gameObject;

            if (!_target)
                Destroy(gameObject);
            return;
        }

		Vector3 target = _target.transform.position;      
		Vector3 newTarget = transform.position;

		float d = Mathf.Max(Vector3.Distance (target, newTarget), 1);
        float a = Mathf.Min(7.5f, d*d);
		float toleranceX = 0.3f;
		float toleranceY = 0.3f;

		if (_collecting) 
		{
			if (newTarget.x < (target.x - toleranceX))
				newTarget.x += CollectSpeed * Time.deltaTime * a;
			else if(newTarget.x > (target.x + toleranceX))
				newTarget.x -= CollectSpeed * Time.deltaTime * a;

			if (newTarget.y < (target.y - toleranceY))
				newTarget.y += CollectSpeed * Time.deltaTime * a;
			else if(newTarget.y > (target.y + toleranceY))
				newTarget.y -= CollectSpeed * Time.deltaTime * a;

			transform.position = newTarget;
		}
	}
}

