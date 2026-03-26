using UnityEngine;
using System.Collections;

public class Herb : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		StartCoroutine (Uproot ());
	}

	protected virtual IEnumerator Uproot()
	{
		yield return new WaitForSeconds(0.3f); 

		Animator _animator = GetComponent<Animator> ();
		CorgiTools.UpdateAnimatorBool(_animator,"Uproot",true);

		StartCoroutine (Gone ());
	}

	protected virtual IEnumerator Gone()
	{
		yield return new WaitForSeconds(0.517f); 

		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}

