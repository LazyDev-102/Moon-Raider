using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour
{
	float minFlickerIntensity = 0.5f;
	float maxFlickerIntensity = 2.0f;
	float flickerSpeed = 0.07f;
	float randomIntensity = 3.5f;
    float intensity = 1;

    //private Light tlight;
    SpriteRenderer lightRenderer;

	// Use this for initialization
	public virtual void Awake()
	{
		// Some real light
		//tlight = gameObject.GetComponent<Light> ();
		lightRenderer = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];

		StartCoroutine (Flicker (flickerSpeed));
	}

	public void Update()
	{intensity = Mathf.Lerp(intensity, randomIntensity, 10 * Time.deltaTime);
        //tlight.intensity = intensity;
        lightRenderer.color = new Color(lightRenderer.color.r, lightRenderer.color.g, lightRenderer.color.b, 0.5f* intensity / maxFlickerIntensity);
	}

	public virtual IEnumerator Flicker(float duration)
	{
		yield return new WaitForSeconds (duration);

		randomIntensity = Random.Range (minFlickerIntensity, maxFlickerIntensity);

		StartCoroutine (Flicker (flickerSpeed));
	}
}



