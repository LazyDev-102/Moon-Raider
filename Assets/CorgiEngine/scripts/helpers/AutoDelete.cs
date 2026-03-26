using UnityEngine;
using System.Collections;

public class AutoDelete : MonoBehaviour
{
    public GameObject DestroyAnimation;
    public AudioClip DestroySound;
    public float Cycles = 1;

    // Use this for initialization
    void Start()
    {
        if (Cycles < 1)
            Cycles = 1;

        float t = Random.Range(0.75f, 1.5f);

        Animator animator = GetComponent<Animator>();

        if (animator)
            t = animator.GetCurrentAnimatorStateInfo(0).length;

        if (DestroyAnimation != null)
            StartCoroutine(DestroyEffect(Cycles*t - 0.1f));

        //Debug.Log("Destroying myself in " + t);
        Destroy(gameObject, Cycles*t);
    }


    IEnumerator DestroyEffect(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (DestroySound != null)
            SoundManager.Instance.PlaySound(DestroySound, transform.position);

        var burstEffect = Instantiate(DestroyAnimation, transform.position, transform.rotation);

        Destroy(burstEffect, burstEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
