using UnityEngine;
using System.Collections;


public class Lunar : MonoBehaviour
{
    AISayThings sayThings;
    AIReact react;
    bool wasReacting = false;

    public bool SecretFound = false;
    public float FallSpeed = 0.25f;

    // private stuff
    protected Vector2 _upPosition, _downPosition, _orgPosition, _newPosition;

    bool down = true;


    // Use this for initialization
    void Start()
    {
        _orgPosition = transform.position;
        resetUpDown();

        sayThings = GetComponent<AISayThings>();
        react = GetComponent<AIReact>();

       StartCoroutine(CheckForWall());
    }


    // If the wall was already found, clear me out
    public IEnumerator CheckForWall()
    {
        yield return new WaitForSeconds(0.1f);

        LayerMask maskLayer = 1 << LayerMask.NameToLayer("Projectiles");
        RaycastHit2D circle = Physics2D.CircleCast(transform.localPosition, 12, Vector2.right, 0.0f, maskLayer);

        if (!circle)
        {
            Destroy(gameObject);
        }
    }


    private void resetUpDown()
    {
        FallSpeed = 0.75f;
        _upPosition = transform.position + 0.2f * Vector3.up;
        _downPosition = transform.position + 0.2f * Vector3.down;
    }

    // Update is called once per frame
    void Update()
    {
        if (react.Reacting && !wasReacting)
        {
            wasReacting = true;
            StartCoroutine(GiveHint(3));
        }
    }

    protected virtual void FixedUpdate()
    {
        if (down)
        {
            _newPosition = new Vector2(0, -FallSpeed * Time.deltaTime);

            if (transform.position.y < _downPosition.y)
            {
                down = false;
            }
        }
        else
        {
            _newPosition = new Vector2(0, FallSpeed * Time.deltaTime);

            if (transform.position.y > _upPosition.y)
            {
                down = true;
            }
        }

        transform.Translate(_newPosition, Space.World);
    }


    public IEnumerator GiveHint(float duration)
    {
        yield return new WaitForSeconds(0.1f);

        if (SecretFound)
        {
            sayThings.SaySomething(3, 3);
        }
        else
        {
            sayThings.SaySomething(0, 3);

            yield return new WaitForSeconds(duration);

            sayThings.SaySomething(1, 3);

            yield return new WaitForSeconds(duration);

            sayThings.SaySomething(2, 3);
        }

        yield return new WaitForSeconds(2*duration);

        wasReacting = false;
    }
}
