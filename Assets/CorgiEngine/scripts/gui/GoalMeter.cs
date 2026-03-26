using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GoalMeter : MonoBehaviour
{
    public Image Frame;
    public Image Digit0;
    public Image Digit1;
    public Image Digit2;

    public AudioClip IncrementSound;

    private Sprite[] sprites;

    // Use this for initialization
    void Start()
    {
        sprites = Resources.LoadAll<Sprite>("digits");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlaySound()
    {
        if (IncrementSound != null)
            SoundManager.Instance.PlaySound(IncrementSound, transform.position);
    }

    public virtual IEnumerator Flicker(Color color)
    {
        Frame.color = color;
        yield return new WaitForSeconds(0.18f);
        Frame.color = Color.white;
        yield return new WaitForSeconds(0.18f);
        Frame.color = color;
        yield return new WaitForSeconds(0.18f);
        Frame.color = Color.white;
    }

    public virtual void DisplayPoints(bool shouldFlicker)
    {
        if (GameManager.Instance.Player == null)
        {
            Digit0.sprite = sprites[0];
            Digit1.sprite = sprites[0];
            Digit2.sprite = sprites[0];
            return;
        }

        int roundedPoints = GameManager.Instance.SpecialPoints;
        if (roundedPoints > GameManager.Instance.Player.BehaviorParameters.MaxSpecialGems)
            roundedPoints = GameManager.Instance.Player.BehaviorParameters.MaxSpecialGems;

        if (roundedPoints < 1)
        {
            Digit0.sprite = sprites[0];
            Digit1.sprite = sprites[0];
            Digit2.sprite = sprites[0];
        }
        else if (roundedPoints < 10)
        {
            Digit0.sprite = sprites[0];
            Digit1.sprite = sprites[0];
            Digit2.sprite = sprites[roundedPoints];
        }
        else if (roundedPoints < 100)
        {
            int ones = roundedPoints % 10;
            int tens = (roundedPoints - ones) / 10;

            Digit0.sprite = sprites[0];
            Digit1.sprite = sprites[tens];
            Digit2.sprite = sprites[ones];
        }
        else
        {
            int ones = roundedPoints % 10;
            int tens = ((roundedPoints - ones) / 10) % 10;
            int hundreds = (roundedPoints - 10*tens - ones) / 100;

            Digit0.sprite = sprites[hundreds];
            Digit1.sprite = sprites[tens];
            Digit2.sprite = sprites[ones];
        }

        if (GameManager.Instance.Player != null)
        {
            float max = (float)GameManager.Instance.Player.BehaviorParameters.MaxSpecialGems;
            if (roundedPoints > 0.9f* max)
            {
                Digit0.color = Color.green;
                Digit1.color = Color.green;
                Digit2.color = Color.green;
            }
            else if (roundedPoints < 0.25f * max)
            {
                Digit0.color = Color.red;
                Digit1.color = Color.red;
                Digit2.color = Color.red;
            }
            else if (roundedPoints >= 0.25f* max && roundedPoints < 0.5f* max)
            {
                Digit0.color = Color.yellow;
                Digit1.color = Color.yellow;
                Digit2.color = Color.yellow;
            }
            else
            {
                Digit0.color = Color.white;
                Digit1.color = Color.white;
                Digit2.color = Color.white;
            }

            if(shouldFlicker)
                StartCoroutine(Flicker(Digit0.color));
        }
    }
}
