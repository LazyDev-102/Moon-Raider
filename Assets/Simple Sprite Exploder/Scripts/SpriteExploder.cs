using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SpriteExploder : MonoBehaviour {

    [SerializeField]
    private GameObject pixelPrefab;
    public GameObject PixelPrefab { get { return pixelPrefab; } set { pixelPrefab = value; } }
    [SerializeField]
    private bool hasGeneratedSprite;
    public bool HasGeneratedSprite { get { return hasGeneratedSprite; } private set { hasGeneratedSprite = value; } }
    [SerializeField]
    private bool explodeFromCenter;
    public bool ExplodeFromCenter { get { return explodeFromCenter; } set { explodeFromCenter = value; } }
    [SerializeField]
    private bool explodeOnClick;
    public bool ExplodeOnClick { get { return explodeOnClick; } set { explodeOnClick = value; } }
    [SerializeField]
    private bool explodeOnCollision;
    public bool ExplodeOnCollision { get { return explodeOnCollision; } set { explodeOnCollision = value; } }
    
    [SerializeField]
    private float minimumExplosionVelocity;
    public float MinimumExplosionVelocity { get { return minimumExplosionVelocity; } set { minimumExplosionVelocity = value; } }
    [SerializeField]
    private float explosionForce;
    public float ExplosionForce { get { return explosionForce; } set { explosionForce = value; } }
    [SerializeField]
    private float explosionRandomness;
    public float ExplosionRandomness { get { return explosionRandomness; } set { explosionRandomness = value; } }
    [SerializeField]
    private float pixelLifespan;
    public float PixelLifespan { get { return pixelLifespan; } set { pixelLifespan = value; } }
    [SerializeField]
    private float pixelLifespanRandomness;
    public float PixelLifespanRandomness { get { return pixelLifespanRandomness; } set { pixelLifespanRandomness = value; } }
   
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Rigidbody2D _rigidbody;
    private int spriteWidth;
    private int spriteHeight;
    private bool explodeFromCollision;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void GenerateSprite()
    {
		Sprite spriteToExplode = GetComponent<SpriteRenderer>().sprite;      

		var croppedTexture = new Texture2D((int)spriteToExplode.rect.width, (int)spriteToExplode.rect.height);
		var pixels = spriteToExplode.texture.GetPixels((int)spriteToExplode.textureRect.x,
		                                               (int)spriteToExplode.textureRect.y,
		                                               (int)spriteToExplode.textureRect.width,
		                                               (int)spriteToExplode.textureRect.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
      
		spriteWidth = croppedTexture.width;
		spriteHeight = croppedTexture.height;
      
        Sprite pixelSprite = pixelPrefab.GetComponent<SpriteRenderer>().sprite;

        if (transform.Find("ExplosionPixels") != null)
            DestroyImmediate(transform.Find("ExplosionPixels").gameObject);

        GameObject explosionParent = new GameObject();
        explosionParent.transform.parent = transform;
        explosionParent.transform.localPosition = Vector3.zero;
        explosionParent.transform.localScale = new Vector2(1.05f, 1.05f);

        Collider2D col = GetComponent<Collider2D>();
        if (col.GetType() == typeof(BoxCollider2D))
            explosionParent.AddComponent<BoxCollider2D>(GetComponent<BoxCollider2D>());
        else if (col.GetType() == typeof(CircleCollider2D))
            explosionParent.AddComponent<CircleCollider2D>(GetComponent<CircleCollider2D>());
        else if (col.GetType() == typeof(PolygonCollider2D))
            explosionParent.AddComponent<PolygonCollider2D>(GetComponent<PolygonCollider2D>());
        else if (col.GetType() == typeof(CapsuleCollider2D))
            explosionParent.AddComponent<CapsuleCollider2D>(GetComponent<CapsuleCollider2D>());
        explosionParent.GetComponent<Collider2D>().isTrigger = true;

        explosionParent.name = "ExplosionPixels";

        int DensityX = 4;
        int DensityY = 4;

		for (int i = 0; i < spriteHeight; i += DensityX)
        {
			for (int j = 0; j < spriteWidth; j += DensityY)
            {
				if (System.Math.Abs(croppedTexture.GetPixel(j, i).a) > 0.01f)
                {
                    GameObject g = Instantiate(pixelPrefab);
                    g.layer = 2;
                    Transform pixel = g.transform;
                    pixel.parent = transform;
                    pixel.localPosition = new Vector2(j, i) / spriteToExplode.pixelsPerUnit;
                    pixel.localPosition -= new Vector3((spriteWidth - pixelSprite.texture.width) / 2.0f, (spriteHeight - pixelSprite.texture.width) / 2.0f) / spriteToExplode.pixelsPerUnit;
					Color pixelColor = new Color(croppedTexture.GetPixel(j, i).r, croppedTexture.GetPixel(j, i).g, croppedTexture.GetPixel(j, i).b, 1);
                    pixel.GetComponent<SpriteRenderer>().color = pixelColor;
                    pixel.parent = explosionParent.transform;
                    pixel.gameObject.SetActive(false);
                }
            }
        }

        hasGeneratedSprite = true;
    }

    public void ExplodeSprite()
    {
        _spriteRenderer.enabled = false;
        _collider.enabled = false;
        transform.Find("ExplosionPixels").GetComponent<Collider2D>().enabled = false;
        _rigidbody.isKinematic = true; 

        Transform ExplosionParent = transform.Find("ExplosionPixels");

        Vector2 explosionCenter = Vector2.zero;
            
        if (explodeFromCenter)
            explosionCenter = ExplosionParent.position;
        else
            explosionCenter = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for (int i = 0; i < ExplosionParent.childCount; i++)
        {
            Transform currentPixel = ExplosionParent.GetChild(i);
            currentPixel.gameObject.SetActive(true);

            Vector2 dir = (Vector2)currentPixel.position - explosionCenter; 
            float dist = dir.magnitude;

            float explosionForceWithRandomness = 0;
            if (explosionRandomness != 0)
                explosionForceWithRandomness = Random.Range(explosionForce - explosionRandomness, explosionForce + explosionRandomness);
            else
                explosionForceWithRandomness = explosionForce;

            Rigidbody2D pixelRigidbody = currentPixel.GetComponent<Rigidbody2D>();
            pixelRigidbody.AddForce(Mathf.Lerp(0, explosionForceWithRandomness, (1 - dist)) * dir, ForceMode2D.Impulse);

            StartCoroutine(TurnOffPixel(currentPixel.gameObject, Random.Range(pixelLifespan - pixelLifespanRandomness, pixelLifespan + pixelLifespanRandomness)));
        }
    }

    public void ExplodeSprite(Vector2 explosionCenter)
    {
        _spriteRenderer.enabled = false;
        _collider.enabled = false;
        transform.Find("ExplosionPixels").GetComponent<Collider2D>().enabled = false;
        _rigidbody.isKinematic = true;

        Transform ExplosionParent = transform.Find("ExplosionPixels");

        for (int i = 0; i < ExplosionParent.childCount; i++)
        {
            Transform currentPixel = ExplosionParent.GetChild(i);
            currentPixel.gameObject.SetActive(true);

            Vector2 dir = (Vector2)currentPixel.position - explosionCenter;
            float dist = dir.magnitude;

            float explosionForceWithRandomness = 0;
            if (explosionRandomness != 0)
                explosionForceWithRandomness = Random.Range(explosionForce - explosionRandomness, explosionForce + explosionRandomness);
            else
                explosionForceWithRandomness = explosionForce;

            Rigidbody2D pixelRigidbody = currentPixel.GetComponent<Rigidbody2D>();
            pixelRigidbody.AddForce(Mathf.Lerp(0, explosionForceWithRandomness, (1 - dist)) * dir, ForceMode2D.Impulse);

            StartCoroutine(TurnOffPixel(currentPixel.gameObject, Random.Range(pixelLifespan - pixelLifespanRandomness, pixelLifespan + pixelLifespanRandomness)));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (explodeOnCollision)
        {
            if (_rigidbody.velocity.sqrMagnitude > minimumExplosionVelocity * minimumExplosionVelocity)
            {
                explodeFromCollision = true;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (explodeFromCollision)
        {
            if (explodeFromCenter)
            {
                ExplodeSprite();
            }
            else
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    ExplodeSprite(contact.point);
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (explodeOnClick)
            ExplodeSprite();
    }

    private IEnumerator TurnOffPixel(GameObject g, float time)
    {
        yield return new WaitForSeconds(time);
        g.SetActive(false);
    }
    
}