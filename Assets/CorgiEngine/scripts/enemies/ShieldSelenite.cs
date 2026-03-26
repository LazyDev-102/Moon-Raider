using UnityEngine;
using System.Collections;

public class ShieldSelenite : MonoBehaviour
{
    private Health _health;
    private AISimpleWalk _walk;
    private SpriteRenderer _sprite;
    private AIShootOnSight _ai;

    // Use this for initialization
    void Awake()
    {
        _health = GetComponent<Health>();
        _walk = GetComponent<AISimpleWalk>();
        _sprite = GetComponent<SpriteRenderer>();
        _ai = GetComponent<AIShootOnSight>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.Player == null)
            return;

        float px = GameManager.Instance.Player.transform.position.x;
        float ps = GameManager.Instance.Player.transform.localScale.x;
        float x = transform.position.x;
        float s = transform.localScale.x;

        if ((px > x && ps == -1 && s == 1) || (px < x && ps == 1 && s == -1))
        {
            _walk.Disable();
            _health.MinDamageThreshold = 5;
        }
        else
        {
            if (!_ai.isShooting)
            {
                if(_walk.Speed == 0)
                    _walk.Walk();
                _health.MinDamageThreshold = 1;
            }
        }
    }
}
