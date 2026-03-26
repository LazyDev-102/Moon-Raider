using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteExploder))]
public class SpriteExploderEditor : Editor
{

    public override void OnInspectorGUI()
    {
        SpriteExploder spriteExploder = (SpriteExploder)target;

        spriteExploder.PixelPrefab = (GameObject)EditorGUILayout.ObjectField("Pixel Prefab", spriteExploder.PixelPrefab, typeof(GameObject), true);
        
        EditorGUILayout.BeginVertical();

        string genText = "";
        string genButtonText = "";

        if (spriteExploder.HasGeneratedSprite && spriteExploder.transform.Find("ExplosionPixels") != null)
        {
            genText = "You have generated your explosion pixels.";
            genButtonText = "Regenerate Pixels";
        }
        else
        {
            genText = "Press the button below to generate the explosion pixels.";
            genButtonText = "Generate Pixels";
        }

        if (spriteExploder.PixelPrefab == null)
            genText = "Please assign a pixel prefab to generate the explosion pixels.";

        EditorGUILayout.LabelField(genText);

        if (spriteExploder.PixelPrefab != null)
        {
            if (GUILayout.Button(genButtonText))
            {
                spriteExploder.GenerateSprite();
            }
        }

        EditorGUILayout.EndVertical();

        if (spriteExploder.HasGeneratedSprite && spriteExploder.transform.Find("ExplosionPixels") != null)
        {
            spriteExploder.ExplodeFromCenter = EditorGUILayout.Toggle("Explode From Center", spriteExploder.ExplodeFromCenter);
            spriteExploder.ExplodeOnClick = EditorGUILayout.Toggle("Explode On Click", spriteExploder.ExplodeOnClick);
            spriteExploder.ExplodeOnCollision = EditorGUILayout.Toggle("Explode On Collision", spriteExploder.ExplodeOnCollision);

            if (spriteExploder.ExplodeOnCollision)
            {
                spriteExploder.MinimumExplosionVelocity = EditorGUILayout.FloatField("Minimum Explosion Velocity", spriteExploder.MinimumExplosionVelocity);
            }

            spriteExploder.ExplosionForce = EditorGUILayout.FloatField("Explosion Force", spriteExploder.ExplosionForce);
            if (spriteExploder.ExplosionForce < 0)
                spriteExploder.ExplosionForce = 0;
            spriteExploder.ExplosionRandomness = EditorGUILayout.Slider("Explosion Randomness", spriteExploder.ExplosionRandomness, 0, spriteExploder.ExplosionForce);
            if (spriteExploder.ExplosionRandomness < 0)
                spriteExploder.ExplosionRandomness = 0;
            if (spriteExploder.ExplosionRandomness > spriteExploder.ExplosionForce)
                spriteExploder.ExplosionRandomness = spriteExploder.ExplosionForce;


            spriteExploder.PixelLifespan = EditorGUILayout.FloatField("Pixel Lifespan", spriteExploder.PixelLifespan);
            if (spriteExploder.PixelLifespan < 0)
                spriteExploder.PixelLifespan = 0;
            spriteExploder.PixelLifespanRandomness = EditorGUILayout.Slider("Pixel Lifespan Randomness", spriteExploder.PixelLifespanRandomness, 0, spriteExploder.PixelLifespan);
            if (spriteExploder.PixelLifespanRandomness < 0)
                spriteExploder.PixelLifespanRandomness = 0;
            if (spriteExploder.PixelLifespanRandomness > spriteExploder.PixelLifespan)
                spriteExploder.PixelLifespanRandomness = spriteExploder.PixelLifespan;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }

}
