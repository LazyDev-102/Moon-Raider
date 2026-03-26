using UnityEngine;
using System.Collections;

public class AutoDamage : MonoBehaviour
{
    public float DamageWait = 1;
    public int Damage = 1;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(TakeDamage(DamageWait));
    }

    // Update is called once per frame
    void Update()
    {

    }


    public virtual IEnumerator TakeDamage(float duration)
    {
        yield return new WaitForSeconds(duration);

        Health health = GetComponent<Health>();

        if (health != null)
            health.TakeDamage(Damage, gameObject, false);
    }
}
