using UnityEngine;
using System.Collections;

public class WallOpener : MonoBehaviour
{
    private bool Opened = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CharacterBehavior>() == null)
            return;

        if(!Opened)
        {
            Opened = true;

            var maskLayer = 1 << LayerMask.NameToLayer("Platforms");
            RaycastHit2D[] circles = Physics2D.CircleCastAll(transform.localPosition, 400.0f, Vector2.right, 0.0f, maskLayer);

            float order = 3.0f;
            for (int i = 0; i < circles.Length; i++)
            {
                var circle = circles[i];

                var wall = circle.collider.gameObject.GetComponent<BossWall>();

                if (wall != null)
                {
                    wall.Order = order;
                    order += 0.125f;
                    wall.Deactivate();
                }
            }

            CameraController sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            sceneCamera.StartCoroutine(sceneCamera.SpeedUp(2f));
            sceneCamera.LockX = false;
        }
    }
}
