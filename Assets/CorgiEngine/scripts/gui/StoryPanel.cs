using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoryPanel : MonoBehaviour
{
    public StoryPanelLayer[] Layer;

    public float AnimationSpeed;
    public int AnimationFrames;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < Layer.Length; i++)
        {
            Layer[i].StartPosition = Layer[i].Image.rectTransform.anchoredPosition;
            Layer[i].StartScale = Layer[i].Image.rectTransform.localScale;

            StartCoroutine(MoveLayer(i, 0.4f));
        }
    }


    public IEnumerator MoveLayer(int l, float delay)
    {
        yield return new WaitForSeconds(delay);

//        if (SlideUpSound != null)
//            SoundManager.Instance.PlaySound(SlideUpSound, transform.position);

        for (int i = 0; i < AnimationFrames; i++)
        {
            Layer[l].Image.rectTransform.anchoredPosition = Vector3.Lerp(Layer[l].StartPosition, Layer[l].TargetPosition, i * AnimationSpeed);
            Layer[l].Image.rectTransform.localScale = Vector3.Lerp(Layer[l].StartScale, Layer[l].TargetScale, i * AnimationSpeed);
            yield return new WaitForSeconds(AnimationSpeed);
        }
    }
}
