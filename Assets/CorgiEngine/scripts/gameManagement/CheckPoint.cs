using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Checkpoint class. Will make the player respawn at this point if it dies.
/// </summary>
public class CheckPoint : MonoBehaviour 
{
    protected List<IPlayerRespawnListener> _listeners;

    /// <summary>
    /// Initializes the list of listeners
    /// </summary>
    protected virtual void Awake () 
	{
		_listeners = new List<IPlayerRespawnListener>();
	}
	
	/// <summary>
	/// Called when the player hits the checkpoint
	/// </summary>
	public virtual void PlayerHitCheckPoint()
	{
		// what happens when the player hits the checkpoint
	}

    protected virtual IEnumerator PlayerHitCheckPointCo(int bonus)
	{
		yield break;
	}
	
	/// <summary>
	/// Called when the player leaves the checkpoint
	/// </summary>
	public virtual void PlayerLeftCheckPoint()
	{
		// what happens when the player leaves the checkpoint
	}
	
	/// <summary>
	/// Spawns the player at the checkpoint.
	/// </summary>
	/// <param name="player">Player.</param>
	public virtual void SpawnPlayer(CharacterBehavior player, Transform location, float offset = 0)
	{
        player.RespawnAt(location, transform.position.x < 0);
		
		foreach(var listener in _listeners)
		{
			listener.onPlayerRespawnInThisCheckpoint(this,player);
		}
	}
	
	public virtual void AssignObjectToCheckPoint (IPlayerRespawnListener listener) 
	{
		_listeners.Add(listener);
	}
}
