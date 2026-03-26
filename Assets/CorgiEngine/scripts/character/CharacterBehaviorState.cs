using UnityEngine;
using System.Collections;

/// <summary>
/// The various states you can use to check if your character is doing something at the current frame
/// by Renaud Forestié
/// </summary>

public class CharacterBehaviorState
{
    /// can the character jump right now ?
    public bool CanJump { get; set; }
    /// can the character shoot right now ?
    public bool CanShoot { get; set; }
    /// can the character use its melee attack right now ?
    public bool CanMelee { get; set; }
    /// can the character dash right now ?
    public bool CanDash { get; set; }
    /// can the character move freely right now ?
    public bool CanMoveFreely { get; set; }
    /// can the character jetpack ?
    public bool CanJetpack { get; set; }
    /// the number of jumps left to the character
    public int NumberOfJumpsLeft;
    /// true if the character is dashing right now
    public bool Dashing { get; set; }
    /// true if the character is running right now
    public bool Running { get; set; }
    /// true if the character is crouching right now
    public bool Crouching { get; set; }
    /// true if the character was crouching during the previous frame
    public bool CrouchingPreviously { get; set; }
    /// true if the character is looking up right now
    public bool LookingUp { get; set; }
    /// true if the character is clinging to a wall right now
    public bool WallClinging { get; set; }
    /// true if the character is jetpacking right now
    public bool Jetpacking { get; set; }
    /// true if the character is dash diving right now
    public bool Diving { get; set; }
    /// true if the character is firing its gun right now
    public bool Firing { get; set; }
    /// becomes true for one frame when the character stops shooting
    public bool FiringStop { get; set; }
    /// true if the character is colliding with a ladder
    public bool LadderColliding { get; set; }
    /// true if the character is colliding with the top of a ladder
    public bool LadderTopColliding { get; set; }
    /// true if the character is climbing on a ladder
    public bool LadderClimbing { get; set; }
    /// the current ladder climbing speed of the character
    public float LadderClimbingSpeed { get; set; }
    /// the firing direction - 1 : top, 2 : top right, 3 : right, 4 : bottom right, 5 : bottom, 6 : bottom left, 7 : left, 8 : top left
    public int FiringDirection { get; set; }
    /// true if the character is currently dead
    public bool IsDead { get; set; }
    /// the remaining jetpack fuel duration (in seconds)
    public float JetpackFuelDurationLeft { get; set; }
    /// true if the character is in a dialogue zone
    public bool InDialogueZone { get; set; }
    /// the current dialogue zone
    public DialogueZone CurrentDialogueZone;
    // Hurting
    public bool Hurting { get; set; }

    /// true if the character is attacking using its melee attack right now
    public bool MeleeEnergized { get; set; }

    // Pickup
    public bool CanPickup { get; set; }
    public bool Pickup { get; set; }
    public bool CritterUp { get; set; }
    public bool OnRideAnimal { get; set; }
    public bool Swimming { get; set; }
    public bool DoubleJumping { get; set; }
    public bool CoveredInSpores { get; set; }
    public bool WantsToShoot { get; set; }

    /// <summary>
    /// Initializes all states to their default value
    /// </summary>
    public virtual void Initialize()
    {
        CanMoveFreely = true;
        CanDash = true;
        CanShoot = true;
        CanMelee = true;
        CanJetpack = true;
        Dashing = false;
        Running = false;
        Crouching = false;
        CrouchingPreviously = false;
        LookingUp = false;
        WallClinging = false;
        Jetpacking = false;
        Diving = false;
        LadderClimbing = false;
        LadderColliding = false;
        LadderTopColliding = false;
        LadderClimbingSpeed = 0f;
        Firing = false;
        FiringStop = false;
        FiringDirection = 3;
        InDialogueZone = false;
        CurrentDialogueZone = null;
        CanPickup = false;
        Pickup = false;
        CritterUp = false;
        OnRideAnimal = false;
        DoubleJumping = false;
    }
}
