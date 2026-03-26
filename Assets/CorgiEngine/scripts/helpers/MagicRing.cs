using UnityEngine;
using System.Collections;

public class MagicRing : MonoBehaviour
{
	public Vector2 Direction;
	public float Speed = 1;

	private SpriteRenderer _renderer;
	private Shader _shaderGUItext;
	private Shader _shaderSpritesDefault;

	// Use this for initialization
	void Start ()
	{
		_renderer = GetComponent<SpriteRenderer> ();
		_shaderGUItext = Shader.Find("GUI/Text Shader");
		_shaderSpritesDefault = Shader.Find("Sprites/Default");

		StartCoroutine(Flicker ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		float randX = Speed * Time.deltaTime;
		float randY = Speed * Time.deltaTime;

		Vector2 newPosition = new Vector2 (randX*Direction.x, randY*Direction.y);
		transform.Translate (newPosition, Space.World);
	}

	public IEnumerator Flicker()
	{
		for (var n = 0; n < 10; n++) {
			_renderer.material.shader = _shaderGUItext;
			yield return new WaitForSeconds (0.1f);
			_renderer.material.shader = _shaderSpritesDefault;
			yield return new WaitForSeconds (0.1f);
		}
	}
}

