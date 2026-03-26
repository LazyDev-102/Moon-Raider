using UnityEngine;
using System.Collections;

public class RotatingPlatform : MonoBehaviour
{
    public bool Clockwise = true;
    public LayerMask PivotLayer;
    public float Radius = 2;
    public float Speed = 1;

    public Vector3 Direction = Vector3.zero;
    Vector3[] targetPosition;
    int targetState = 0;

    // Use this for initialization
    void Start()
    {
        targetPosition = new Vector3[4];

        // Find pivot point in middle and my position relative to it
        RaycastHit2D circle = Physics2D.CircleCast(transform.position, Radius, Vector2.right, 0.0f, PivotLayer);

        if (circle)
        {
            Vector3 pivotPos = circle.collider.gameObject.transform.position;

            print("Pivot pos is " + pivotPos);

            // NE to SE
            targetPosition[0] = pivotPos + Radius * (Vector3.right + Vector3.down);

            // SE to SW
            targetPosition[1] = pivotPos + Radius * (Vector3.left + Vector3.down);

            // SW to NW
            targetPosition[2] = pivotPos + Radius * (Vector3.left + Vector3.up);

            // NW to NE
            targetPosition[3] = pivotPos + Radius * (Vector3.right + Vector3.up);

            // E
            if (pivotPos.x < transform.position.x)
            {
                targetState = 0;
            }
            // W
            else if (pivotPos.x > transform.position.x)
            {
                targetState = 2;
            }
            else
            {
                // S
                if (pivotPos.y < transform.position.y)
                {
                    targetState = 1;
                }
                // N
                else
                {
                    targetState = 3;
                }
            }
        }

        transform.position = new Vector3(targetPosition[targetState].x, targetPosition[targetState].y, transform.position.z);

        Debug.Log("Target state is " + targetState);
    }


    protected virtual void FixedUpdate()
    {
        Vector3 newPosition = Vector3.zero;

        // NE to SE
        if (targetState == 0)
        {
            Direction = Vector3.down;

            if (transform.position.y > targetPosition[targetState].y)
                newPosition = new Vector2(0, -Speed * Time.deltaTime);
            else
            {
                transform.position = new Vector3(transform.position.x, targetPosition[targetState].y, transform.position.z);
                targetState = 1;
                return;
            }
        }
        // SE to SW
        else if (targetState == 1)
        {
            Direction = Vector3.left;

            if (transform.position.x > targetPosition[targetState].x)
                newPosition = new Vector2(-Speed * Time.deltaTime, 0);
            else
            {
                transform.position = new Vector3(targetPosition[targetState].x, transform.position.y, transform.position.z);
                targetState = 2;
                return;
            }
        }
        // SW to NW
        else if (targetState == 2)
        {
            Direction = Vector3.up;

            if (transform.position.y < targetPosition[targetState].y)
                newPosition = new Vector2(0, Speed * Time.deltaTime);
            else
            {
                transform.position = new Vector3(transform.position.x, targetPosition[targetState].y, transform.position.z);
                targetState = 3;
                return;
            }
        }
        // NW to NE
        else if (targetState == 3)
        {
            Direction = Vector3.right;

            if (transform.position.x < targetPosition[targetState].x)
                newPosition = new Vector2(Speed * Time.deltaTime, 0);
            else
            {
                transform.position = new Vector3(targetPosition[targetState].x, transform.position.y, transform.position.z);
                targetState = 0;
                return;
            }
        }

        transform.Translate(newPosition, Space.World);
    }
}
