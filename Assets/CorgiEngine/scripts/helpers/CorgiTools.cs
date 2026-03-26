using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Various static methods used throughout the Corgi Engine.
/// </summary>

public static class CorgiTools 
{

	/// <summary>
	/// Draws a debug ray and does the actual raycast
	/// </summary>
	/// <returns>The cast.</returns>
	/// <param name="rayOriginPoint">Ray origin point.</param>
	/// <param name="rayDirection">Ray direction.</param>
	/// <param name="rayDistance">Ray distance.</param>
	/// <param name="mask">Mask.</param>
	/// <param name="debug">If set to <c>true</c> debug.</param>
	/// <param name="color">Color.</param>
	public static RaycastHit2D CorgiRayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask,bool debug,Color color)
	{			
		Debug.DrawRay( rayOriginPoint, rayDirection*rayDistance, color );
		return Physics2D.Raycast(rayOriginPoint,rayDirection,rayDistance,mask);		
	}

	public static RaycastHit2D[] CorgiRaycastAll(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask,bool debug,Color color)
	{			
		Debug.DrawRay( rayOriginPoint, rayDirection*rayDistance, color );
		return Physics2D.RaycastAll(rayOriginPoint,rayDirection,rayDistance,mask);		
	}


	/// <summary>
	/// Outputs the message object to the console, prefixed with the current timestamp
	/// </summary>
	/// <param name="message">Message.</param>
	public static void DebugLogTime(object message)
	{
		Debug.Log (Time.time + " " + message);

	}
	
	/// <summary>
	/// Fades the specified image to the target opacity and duration.
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="opacity">Opacity.</param>
	/// <param name="duration">Duration.</param>
	public static IEnumerator FadeImage(Image target, float duration, Color color)
	{		
		if (target == null)
			yield break;
			
		float alpha = target.color.a;

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
		{
			if (target == null)
				yield break;
			Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
			target.color = newColor;
			yield return null;
		}
	}

	public static IEnumerator Move(GameObject target, float duration, Vector3 targetPos)
	{		
		if (target == null)
			yield break;

		Vector3 orgPos = target.transform.localPosition;

		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
		{
			if (target == null)
				yield break;

			float newX = Mathf.SmoothStep (orgPos.x, targetPos.x, t);
			float newY = Mathf.SmoothStep (orgPos.y, targetPos.y, t);
			float newZ = Mathf.SmoothStep (orgPos.z, targetPos.z, t);

			target.transform.localPosition = new Vector3 (newX, newY, newZ);
			yield return null;
		}
	}

	public static IEnumerator MoveImage(Image target, float duration, Vector3 targetPos)
	{		
		if (target == null)
			yield break;

		Vector3 orgPos = target.rectTransform.anchoredPosition;

		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
		{
			if (target == null)
				yield break;

			float newX = Mathf.SmoothStep (orgPos.x, targetPos.x, t);
			float newY = Mathf.SmoothStep (orgPos.y, targetPos.y, t);
			float newZ = Mathf.SmoothStep (orgPos.z, targetPos.z, t);

			target.rectTransform.anchoredPosition = new Vector3 (newX, newY, newZ);
			yield return null;
		}
	}

	public static IEnumerator MoveRect(RectTransform target, float duration, Vector2 targetPos)
	{		
		if (target == null)
			yield break;

		Vector2 orgPos = target.anchoredPosition;

		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
		{
			if (target == null)
				yield break;

			float newX = Mathf.SmoothStep (orgPos.x, targetPos.x, t);
			float newY = Mathf.SmoothStep (orgPos.y, targetPos.y, t);

			target.anchoredPosition = new Vector2 (newX, newY);

			yield return null;
		}
	}

	/// <summary>
	/// Fades the specified image to the target opacity and duration.
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="opacity">Opacity.</param>
	/// <param name="duration">Duration.</param>
	public static IEnumerator FadeText(Text target, float duration, Color color)
	{
		if (target==null)
			yield break;
			
		float alpha = target.color.a;
		
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
		{
			if (target==null)
				yield break;
			Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
			target.color=newColor;
			yield return null;
		}
	}
	/// <summary>
	/// Fades the specified image to the target opacity and duration.
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="opacity">Opacity.</param>
	/// <param name="duration">Duration.</param>
	public static IEnumerator FadeSprite(SpriteRenderer target, float duration, Color color)
	{
		if (target==null)
			yield break;
	
		float alpha = target.material.color.a;		
		
		float t=0f;
		while (t<1.0f)
		{
			if (target==null)
				yield break;
								
			Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
			target.material.color=newColor;
			
			t += Time.deltaTime / duration;
			
			yield return null;
				
		}
		Color finalColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha,color.a,t));
		target.material.color=finalColor;
	}
	
	/// <summary>
	/// Moves an object from point A to point B in a given time
	/// </summary>
	/// <param name="movingObject">Moving object.</param>
	/// <param name="pointA">Point a.</param>
	/// <param name="pointB">Point b.</param>
	/// <param name="time">Time.</param>
	public static IEnumerator MoveFromTo(GameObject movingObject,Vector3 pointA, Vector3 pointB, float time, float approximationDistance)
	{
		float t = 0f;
        
        float distance = Vector3.Distance(movingObject.transform.position, pointB);

		while (distance >= approximationDistance)
		{
            distance = Vector3.Distance(movingObject.transform.position, pointB);
			t += Time.deltaTime / time; 
			movingObject.transform.position = Vector3.Lerp(pointA, pointB, t);
			yield return 0;
		}
        yield break;
	}
	
	public static void UpdateAnimatorBool(Animator animator, string parameterName,bool value)
	{
		if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Bool))
			animator.SetBool (parameterName, value);
	}
	public static void UpdateAnimatorFloat(Animator animator, string parameterName,float value)
	{
		if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Float))
			animator.SetFloat(parameterName,value);
	}
	public static void UpdateAnimatorInteger(Animator animator, string parameterName,int value)
	{
		if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Int))
			animator.SetInteger(parameterName,value);
	}
	
}
