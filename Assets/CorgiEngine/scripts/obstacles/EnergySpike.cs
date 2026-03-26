using UnityEngine;
using System.Collections;

public class EnergySpike : MonoBehaviour
{
    public AudioClip PopSfx;
    PolygonCollider2D _collider;
    Animator _animator;
    GiveDamageToPlayer _damage;

    // Use this for initialization
    void Start()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _damage = GetComponent<GiveDamageToPlayer>();
        _damage.HurtsPlayer = false;

        _animator = GetComponent<Animator>();

        gameObject.layer = LayerMask.NameToLayer("Safe");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual IEnumerator Popup(float duration)
    {
        yield return new WaitForSeconds(duration);

        gameObject.layer = LayerMask.NameToLayer("Foreground");
        _damage.HurtsPlayer = true;
        _animator.SetBool("Pop", true);

        if (PopSfx != null && Random.Range(0, 3) < 1)
            SoundManager.Instance.PlaySound(PopSfx, transform.position);

        yield return new WaitForSeconds(0.375f);

        _animator.SetBool("Pop", false);
        gameObject.layer = LayerMask.NameToLayer("Safe");
    }
}
