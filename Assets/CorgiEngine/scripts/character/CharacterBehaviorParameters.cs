using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// The various parameters related to the CharacterBehavior class.
/// </summary>

[Serializable]
public class CharacterBehaviorParameters 
{
	[Header("Control Type")]
	/// If set to true, acceleration / deceleration will take place when moving / stopping
	public bool SmoothMovement=true;
	
	[Header("Animation Type")]
	/// Set this to false if you want to implement your own animation system
	public bool UseDefaultMecanim = true;

	[Header("Jump")]
	/// defines how high the character can jump
	public float JumpHeight = 3.025f;
	/// the minimum time in the air allowed when jumping - this is used for pressure controlled jumps
	public float JumpMinimumAirTime = 0.1f;
	/// the maximum number of jumps allowed (0 : no jump, 1 : normal jump, 2 : double jump, etc...)
	public int NumberOfJumps=3;
	public enum JumpBehavior
	{
		CanJumpOnGround,
		CanJumpAnywhere,
		CantJump,
		CanJumpAnywhereAnyNumberOfTimes
	}
	/// basic rules for jumps : where can the player jump ?
	public JumpBehavior JumpRestrictions;
	/// if true, the jump duration/height will be proportional to the duration of the button's press
	public bool JumpIsProportionalToThePressTime = true;
    /// the force applied to the character when the jump button is released
    public float JumpButtonReleaseForce = 12f;

    [Space(10)]	
	[Header("Speed")]
	/// basic movement speed
	public float MovementSpeed = 8f;
	/// the speed of the character when it's crouching
	public float CrouchSpeed = 4f;
	/// the speed of the character when it's walking
	public float WalkSpeed = 8f;
	/// the speed of the character when it's running
	public float RunSpeed = 16f;
	/// the speed of the character when climbing a ladder
	public float LadderSpeed = 2f;
	/// the speed of the character when swimming
	public float SwimmingSpeed = 7f;
	
	[Space(10)]	
	[Header("Dash")]
	/// the duration of dash (in seconds)
	public float DashDuration = 0.15f;
	/// the force of the dash
	public float DashForce = 5f;	
	/// the duration of the cooldown between 2 dashes (in seconds)
	public float DashCooldown = 2f;	
	
	[Space(10)]	
	[Header("Walljump")]
	/// the force of a walljump
	public float WallJumpForce = 3f;
	/// the slow factor when wall clinging
	public float WallClingingSlowFactor = 0.6f;

	[Space(10)]	
	[Header("Swimming")]
	/// the slow factor when wall clinging
	public float SwimmingSlowFallFactor = 0.6f;
	public float SwimmingSlowJumpFactor = 0.6f;
		
	[Space(10)]	
	[Header("Health")]
	/// the maximum health of the character
	public int MaxHealth = 90;
	public int MaxGems = 599;
    public int MaxSpecialGems = 200;
    public int MaxWeapon = 0;

	[Space(10)]	
	[Header("Pickup")]
	/// the duration of dash (in seconds)
	public float PickupDuration = 0.960f;
}
