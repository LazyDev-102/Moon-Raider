using UnityEngine;
using System.Collections;

public class AlarmBot : MonoBehaviour
{
    AIReact react;
    AISimpleWalk walk;
    bool wasReacting = false;
    float orgSpeed = 0;

    // Use this for initialization
    void Start()
    {
        react = GetComponent<AIReact>();
        walk = GetComponentInParent<AISimpleWalk>();

        orgSpeed = walk.Speed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(react.Reacting && !wasReacting)
        {
            wasReacting = true;

            if (walk != null)
            {
                walk.Speed = 1.5f * orgSpeed;

                EnemyController rigid = GetComponentInParent<EnemyController>();
                if (rigid != null)
                {
                    if (transform.position.x < react.point.x && rigid.Speed.x > 0
                        || transform.position.x > react.point.x && rigid.Speed.x < 0)
                    {
                        react.enabled = false;
                        walk.ChangeDirection();
                    }
                    else
                    {
                        wasReacting = false;
                    }
                }
            }
        }
    }
}
