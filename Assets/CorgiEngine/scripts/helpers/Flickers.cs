using UnityEngine;
using System.Collections;

public class Flickers : MonoBehaviour
{
	private SpriteRenderer _renderer;
	private Shader _shaderGUItext;
	private Shader _shaderSpritesDefault;

	public int FlickerCount = 4;
	public float FlickerSpeed = 0.02f;
    public bool Flickering = false;

    // Use this for initialization
    void Start ()
	{
		_renderer = GetComponentInParent<SpriteRenderer> ();

		_shaderGUItext = Shader.Find("GUI/Text Shader");
        _shaderSpritesDefault = Shader.Find("Sprites/Default");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Flicker()
	{
		StartCoroutine (DoFlicker ());
	}
	
	protected virtual IEnumerator DoFlicker()
	{
        Flickering = true;

        for (var n = 0; n < FlickerCount; n++) 
        {
			_renderer.material.shader = _shaderGUItext;
			yield return new WaitForSeconds (FlickerSpeed);
			_renderer.material.shader = _shaderSpritesDefault;
			yield return new WaitForSeconds (FlickerSpeed);
		}

		_renderer.material.shader = _shaderSpritesDefault;

        Flickering = false;
    }
}

