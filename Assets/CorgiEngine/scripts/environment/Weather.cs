using UnityEngine;
using System.Collections;

public class Weather : MonoBehaviour
{

	public float HorizontalScrollSpeed = 0;
	public float VerticalScrollSpeed = 0;

	private bool scroll = true;

	TiledSpriteRenderer _renderer;

	void Start()
	{
//		_renderer = GetComponent<SpriteRenderer> ();
	}

	public void FixedUpdate()
	{
		if (scroll)
		{
			float verticalOffset = Time.time * VerticalScrollSpeed;
			float horizontalOffset = Time.time * HorizontalScrollSpeed;

//			_renderer.material.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);
		}
	}

	public void DoActivateTrigger()
	{
		scroll = !scroll;
	}
}

