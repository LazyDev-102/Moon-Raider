using UnityEngine;
using System.Collections;
/// <summary>
/// Weapon parameters
/// </summary>
public class Weapon : MonoBehaviour 
{
	/// the projectile type the weapon shoots
	public Projectile[] Projectile;

    public int ProjectileCount = 1;

	/// the firing frequency
	public float FireRate;
	/// center of rotation of the gun
	public GameObject GunRotationCenter;
	/// the particle system to instantiate every time the weapon shoots
	public ParticleSystem GunFlames;
	/// the shells the weapon emits
	public ParticleSystem GunShells;	
	/// the initial projectile firing position
	public Transform ProjectileFireLocation;
    // offset for swimming
    public Transform ProjectileFireSwimLocation;
    // offst for aiming up
    public Transform ProjectileFireUpLocation;

    public int NumberOfJumps = 2;
    public bool SuppressRunning = false;
    public bool SuppressWallClling = false;

    protected virtual void Start()
	{
		SetGunFlamesEmission (false);
		SetGunShellsEmission (false);
	}
	
	public virtual void SetGunFlamesEmission(bool state)
	{
		//if(GunFlames != null)
		//	GunFlames.enableEmission=state;	
	}
	
	public virtual void SetGunShellsEmission(bool state)
	{
		//if(GunShells != null)
		//	GunShells.enableEmission=state;	
	}
}
