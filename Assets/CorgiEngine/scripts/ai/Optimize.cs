using UnityEngine;
using System.Collections;

public class Optimize : MonoBehaviour
{
    protected Animator _animator;

    EnemyBehavior _enemyBehavior;
    EnemyController _enemyController;
    AILook _aiLook;
    AISimpleWalk _aiSimpleWalk;
    AISimpleFly _aiSimpleFly;
    AIReact _aiReact;
    Health _health;
    SpriteRenderer _sprite;
    GiveDamageToPlayer _damage;
    AIShootOnSight _shoot;
    Stompable _stompable;
    Guard _guard;
    CircleCollider2D _circle;
    Moth _moth;
    SpriteExploder _exploder;


    protected virtual void Awake()
    {
        _health = GetComponent<Health>();
        _animator = GetComponent<Animator>();
        _enemyBehavior = GetComponent<EnemyBehavior>();
        _enemyController = GetComponent<EnemyController>();
        _aiLook = GetComponent<AILook>();
        _aiSimpleWalk = GetComponent<AISimpleWalk>();
        _aiSimpleFly = GetComponent<AISimpleFly>();
        _aiReact = GetComponent<AIReact>();
        _sprite = GetComponent<SpriteRenderer>();
        _damage = GetComponent<GiveDamageToPlayer>();
        _shoot = GetComponent<AIShootOnSight>();
        _stompable = GetComponent<Stompable>();
        _guard = GetComponent<Guard>();
        _circle = GetComponent<CircleCollider2D>();
        _moth = GetComponent<Moth>();
        _exploder = GetComponent<SpriteExploder>();
    }


    protected virtual void Start()
    {
        SetState(false);
    }


    public IEnumerator SetStateDelayed(float delay, bool state)
    {
        yield return new WaitForSeconds(delay);

        SetState(state);
    }

    public void SetState(bool state)
    {
        // Don't disable if it's in the middle of dying
        
        if (_health != null)
        {
            if (_health.CurrentHealth <= 0 && state)
            {
               SetState(false);
            }
        }
        
        if (_animator != null)
            _animator.enabled = state;
        
        if (_enemyBehavior != null)
            _enemyBehavior.enabled = state;
        
        if (_enemyController != null)
            _enemyController.enabled = state;
        
        if (_aiLook != null)
            _aiLook.enabled = state;

        if (_aiSimpleWalk != null)
        {
            _aiSimpleWalk.SetNeedsToThink(state);
            _aiSimpleWalk.enabled = state;
        }
        
        if (_aiSimpleFly != null)
            _aiSimpleFly.enabled = state;

        if (_aiReact != null)
            _aiReact.enabled = state;

        //if (_sprite != null)
        //    _sprite.color = state ? Color.white : Color.clear;

        if (_damage != null)
            _damage.enabled = state;

        if (_shoot != null)
            _shoot.enabled = state;

        if (_stompable != null)
            _stompable.enabled = state;

        if (_guard != null)
            _guard.enabled = state;

        if (_circle != null)
            _circle.enabled = state;

        if (_moth != null)
            _moth.enabled = state;

        if (_exploder != null)
            _exploder.enabled = state;

        gameObject.isStatic = !state;
    }


    void OnBecameVisible()
    {
        if (_health != null)
        {
            if (_health.CurrentHealth <= 0)
            {
                SetState(false);
                enabled = false;
                return;
            }
        }

        SetState(true);
    }


    void OnBecameInvisible()
    {
        SetState(false);
    }
}
