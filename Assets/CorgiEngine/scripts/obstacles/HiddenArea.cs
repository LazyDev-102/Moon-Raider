using UnityEngine;
using System.Collections;

public class HiddenArea : MonoBehaviour
{

	public AudioClip FadeSfx;

	private Shader _shaderGUItext;

    bool triggered = false;

	// Use this for initialization
	void Start ()
	{
		// _shaderGUItext = Shader.Find("Sprites/Default");
		_shaderGUItext = Shader.Find("GUI/Text Shader");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void OnTriggerEnter2D(Collider2D collider)
	{
		if (triggered || collider.GetComponent<CharacterBehavior>() == null)
			return;

        triggered = true;

        FadeOut ();
	}

	void FadeOut()
	{
		if (FadeSfx != null)
			SoundManager.Instance.PlaySound(FadeSfx, transform.position);

        foreach (Transform child in transform)
        {
            //var shader = child.gameObject.GetComponent<_2dxFX_AL_Normal> ();
            //shader.enabled = false;

            //child is your child transform
            var sprite = child.gameObject.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.material.shader = _shaderGUItext;
                StartCoroutine(CorgiTools.FadeSprite(sprite, 0.25f, new Color(1f, 1f, 1f, 0f)));
            }
        }

        // Find a lunar and turn off (if he's there)
        var maskLayer = 1 << LayerMask.NameToLayer("Enemies");
		RaycastHit2D circle = Physics2D.CircleCast(transform.position, 12, Vector2.right, 12.0f, maskLayer);

		if (circle)
		{
			Lunar l = circle.collider.gameObject.GetComponent<Lunar>();
			if (l != null)
				l.SecretFound = true;
		}
	}
}

