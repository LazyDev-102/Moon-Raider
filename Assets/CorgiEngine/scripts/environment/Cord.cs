using UnityEngine;
using System.Collections;

public class Cord : MonoBehaviour
{
	private SpriteRenderer cableOn;
	private SpriteRenderer cableOff;
    private Animator _animator;

    public bool powered = true;

    public int ReferenceFrame = 0;
    public bool Energized = false;

    // Use this for initialization
    public virtual void Awake()
    {
		cableOn = GetComponentsInChildren<SpriteRenderer> () [0];
		cableOff = GetComponentsInChildren<SpriteRenderer> () [1];

        _animator = GetComponent<Animator>();
    }


    // 0 = down, 1 = right, 2 = up, 3 = left
    public void SetPulse(int direction)
    {
        Energized = true;

        //Debug.Log(ReferenceFrame + " " + direction);

        // down
        if (direction == 0)
        {
            switch (ReferenceFrame)
            {
                case 154: // └
                    _animator.Play("Reverse");
                    break;
            }
        }
        // right
        else if (direction == 1)
        {
            switch (ReferenceFrame)
            {
                case 152:  // ┘
                    _animator.Play("Reverse");
                    break;
            }
        }
        // up
        else if (direction == 2)
        {
            switch (ReferenceFrame)
            {
                case 150: // |
                    _animator.Play("Reverse");
                    break;

                case 155: // ┓
                    _animator.Play("Reverse");
                    break;
            }
        }
        // left
        else if (direction == 3)
        {
            switch (ReferenceFrame)
            {
                case 151: // _
                    _animator.Play("Reverse");
                    break;

                case 153: // ┌
                    _animator.Play("Reverse");
                    break;
            }
        }
    }


	public void Flicker()
	{
        // Remove me from scans
        //gameObject.layer = 15;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;

		float tick = 0.083f;

        StartCoroutine(Off(4 * tick));
        StartCoroutine(On(5 * tick));
        StartCoroutine(Off(6 * tick));
    }

	public virtual IEnumerator On(float duration)
	{
		yield return new WaitForSeconds (duration);
	
		cableOff.enabled = false;
		cableOn.enabled = true;
	}

	public virtual IEnumerator Off(float duration)
	{
		yield return new WaitForSeconds (duration);

		cableOff.enabled = true;
		cableOn.enabled = false;
	}

    public void InstantOff()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        cableOff.enabled = true;
        cableOn.enabled = false;
    }

	// Update is called once per frame
	void Update ()
	{
	
	}
}

