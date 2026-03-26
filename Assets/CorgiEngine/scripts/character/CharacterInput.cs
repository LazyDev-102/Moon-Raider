using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

using Rewired;

public class CharacterInput : PersistentHumbleSingleton<CharacterInput>
{
    private bool canPause = true;

    protected static CharacterBehavior _player1, _player2;

    bool canMove = true;
    bool canSelect = true;
    bool canBack = true;

    ModeMenu pauseMenu;
    

    // Use this for initialization
    void Start()
    {
        pauseMenu = GUIManager.Instance.PauseScreen.GetComponent<ModeMenu>();

        _player1 = null;
        _player2 = null;

        JoystickManager.Instance.Mode = JoystickManager.JoystickMode.GamePlay;
        JoystickManager.Instance.Reset();
    }


    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (JoystickManager.Instance.JoystickConnected)
        {
#endif
            bool paused = HandlePause();

            if (!paused)
            {
                HandlePlayerOne();

                if(GlobalVariables.TwoPlayer)
                    HandlePlayerTwo();
            }
#if UNITY_ANDROID || UNITY_IOS
        }
#endif
    }


    void HandlePlayerOne()
    {
        // if we can't get the player, we do nothing
        if (_player1 == null)
        {
            if (GameManager.Instance.Player != null)
            {
                if (GameManager.Instance.Player.GetComponent<CharacterBehavior>() != null)
                    _player1 = GameManager.Instance.Player;
            }
            else
            {
                //Debug.Log("CI: NULL player!");
                if (GameObject.FindWithTag("Player") != null)
                    GameManager.Instance.Player = GameObject.FindWithTag("Player").GetComponent<CharacterBehavior>();
                return;
            }
            return;
        }

        if (JoystickManager.Instance.playerOne == null)
            return;

        // MOVE
        var h = JoystickManager.Instance.playerOne.GetAxis("MoveX");
        var v = JoystickManager.Instance.playerOne.GetAxis("MoveY");

        float deadzone = 0.25f;
        Vector2 stickInput = new Vector2(h, v);
        if (stickInput.magnitude < deadzone)
            stickInput = Vector2.zero;
        else
            stickInput = stickInput.normalized * ((stickInput.magnitude - deadzone) / (1 - deadzone));

        // if the player can't move for some reason, we do nothing else
        if (!GameManager.Instance.CanMove)
            return;

        _player1.SetVerticalMove(stickInput.y);
        _player1.SetHorizontalMove(stickInput.x);

        if (JoystickManager.Instance.playerOne.GetButtonDown("Melee"))
        {
            if (_player1.Permissions.MeleeAttackEnabled && GameManager.Instance.Points > 0)
                _player1.BehaviorState.MeleeEnergized = _player1.BehaviorState.CanMelee;
            else
            {
                _player1.BehaviorState.MeleeEnergized = false;
                
                if(_player1.BehaviorState.CoveredInSpores)
                    StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.cyan));
                else
                    StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.yellow));

                _player1.PlayMeleeErrorSound();
            }
        }

        if (JoystickManager.Instance.playerOne.GetButtonUp("Melee"))
            _player1.BehaviorState.MeleeEnergized = false;

        if (_player1.BehaviorState.LookingUp)
        {
            _player1.SetHorizontalMove(0);
        }

        if (_player1.BehaviorState.MeleeEnergized)
            return;

        if (_player1.BehaviorState.Swimming)
        {
            if (JoystickManager.Instance.playerOne.GetButton("Jump"))
                _player1.JumpStart();
        }
        else
        {
            if (JoystickManager.Instance.playerOne.GetButtonDown("Jump"))
            {
                if (!_player1.BehaviorState.InDialogueZone)
                {
                    _player1.JumpStart();
                }
            }
        }

        if (JoystickManager.Instance.playerOne.GetButtonUp("Jump"))
            _player1.JumpStop();

        if (JoystickManager.Instance.playerOne.GetButtonDown("Pickup"))
            _player1.Pickup();

        if (_player1.shoot != null)
        {
            _player1.shoot.SetHorizontalMove(h);
            _player1.shoot.SetVerticalMove(v);

            if (JoystickManager.Instance.playerOne.GetButtonDown("Shoot"))
            {
                _player1.shoot.ShootOnce();
                _player1.shoot.ShootStart();
            }

            if (JoystickManager.Instance.playerOne.GetButtonUp("Shoot"))
                _player1.shoot.ShootStop();
        }
    }


    void HandlePlayerTwo()
    {
        if (_player2 == null)
        {
            if (GameManager.Instance.PlayerTwo != null)
            {
                if (GameManager.Instance.PlayerTwo.GetComponent<CharacterBehavior>() != null)
                    _player2 = GameManager.Instance.PlayerTwo;
            }
            else
            {
                if (GameObject.FindWithTag("PlayerTwo") != null)
                    GameManager.Instance.PlayerTwo = GameObject.FindWithTag("PlayerTwo").GetComponent<CharacterBehavior>();
            }

            return;
        }

        if (!_player2.CanControl)
            return;

        // MOVE
        var h = JoystickManager.Instance.playerTwo.GetAxis("MoveX");
        var v = JoystickManager.Instance.playerTwo.GetAxis("MoveY");

        float deadzone = 0.25f;
        Vector2 stickInput = new Vector2(h, v);
        if (stickInput.magnitude < deadzone)
            stickInput = Vector2.zero;
        else
            stickInput = stickInput.normalized * ((stickInput.magnitude - deadzone) / (1 - deadzone));


        // if the player can't move for some reason, we do nothing else
        if (!GameManager.Instance.CanMove || !_player2.isActiveAndEnabled)
        {
            return;
        }

        _player2.SetHorizontalMove(stickInput.x);
        _player2.SetVerticalMove(stickInput.y);

        if (_player2.BehaviorState.Swimming)
        {
            if (JoystickManager.Instance.playerTwo.GetButton("Jump"))
                _player2.JumpStart();
        }
        else
        {
            if (JoystickManager.Instance.playerTwo.GetButtonDown("Jump"))
            {
                if (!_player2.BehaviorState.InDialogueZone)
                {
                    _player2.JumpStart();
                }
            }
        }

        if (JoystickManager.Instance.playerTwo.GetButtonUp("Jump"))
            _player2.JumpStop();

        if (JoystickManager.Instance.playerTwo.GetButtonDown("Pickup"))
            _player2.Pickup();

        if (_player2.shoot != null)
        {
            _player2.shoot.SetHorizontalMove(h);
            _player2.shoot.SetVerticalMove(v);

            if (JoystickManager.Instance.playerTwo.GetButtonDown("Shoot"))
            {
                _player2.shoot.ShootOnce();
                _player2.shoot.ShootStart();
            }

            if (JoystickManager.Instance.playerTwo.GetButtonUp("Shoot"))
                _player2.shoot.ShootStop();
        }

        if (JoystickManager.Instance.playerTwo.GetButtonDown("PauseBack"))
        {
            GameManager.Instance.PlayerTwo.PlayBeamSound();

            //float max = (float)GameManager.Instance.PlayerTwo.BehaviorParameters.MaxGems;
            //GameManager.Instance.AddPoints((int)(0.1f * max));

            StartCoroutine(GameManager.Instance.BeamOut(0.01f, false));
            _player2 = null;
        }
    }


    void SpawnPlayer()
    {
        if (GameManager.Instance.PointPercentage < 0.1f)
        {
            if (GUIManager.Instance.PlayerTwoFlickerRoutine == null)
                GUIManager.Instance.PlayerTwoFlickerRoutine = StartCoroutine(GUIManager.Instance.FlickerPlayerTwoNotice());

            GameManager.Instance.Player.PlayMeleeErrorSound();
            return;
        }

        float max = (float)GameManager.Instance.Player.BehaviorParameters.MaxGems;
        GameManager.Instance.AddPoints((int)(-0.1f * max));

        // If we're not already a two player game, track it!
        if (!GlobalVariables.TwoPlayer)
        {
            AnalyticsEvent.Custom("twoplayer", new Dictionary<string, object>
                    {
                        { "level", GlobalVariables.LevelIndex },
                        { "world", GlobalVariables.WorldIndex },
                        { "time_elapsed", Time.timeSinceLevelLoad }
                    });
        }

        GlobalVariables.TwoPlayer = true;

        // SPAWN AVERY
        StartCoroutine(GameManager.Instance.BeamIn(0.01f));
    }


    bool HandlePause()
    {
        if (JoystickManager.Instance.playerOne == null)
            return true;

        if (canPause && !GameManager.Instance.Paused)
        {
            if (JoystickManager.Instance.playerOne.GetButtonDown("Pause"))
            {
                GameManager.Instance.Pause();
                pauseMenu.HighlightSlot();
                canPause = false;
                return true;
            }

            //Debug.Log("playerTwo not null: " + (JoystickManager.Instance.playerTwo != null));
            //Debug.Log("Paused " + JoystickManager.Instance.playerTwo.GetButtonDown("Pause"));
            //Debug.Log("Player Two is Null " + (GameManager.Instance.PlayerTwo == null));

            //JoystickManager.Instance.playerTwo.GetButtonDown("Pause") && GameManager.Instance.PlayerTwo == null

            if (JoystickManager.Instance.playerTwo != null && JoystickManager.Instance.playerTwo.GetButtonDown("Pause") && GameManager.Instance.PlayerTwo == null)
            {
                SpawnPlayer();
                return false;
            }
        }


        if (GameManager.Instance.Paused)
        {
            // HIGHLIGHT         
            float moveY = JoystickManager.Instance.playerOne.GetAxis("Nav Pause");

            if (canMove)
            {
                if (moveY >= 0.25f)
                {
                    pauseMenu.HighlightedSlot--;

                    if (pauseMenu.HighlightedSlot < 0)
                        pauseMenu.HighlightedSlot = 2;

                    pauseMenu.HighlightSlot();

                    canMove = false;
                    return true;
                }
                else if (moveY <= -0.25f)
                {
                    pauseMenu.HighlightedSlot++;

                    if (pauseMenu.HighlightedSlot > 2)
                        pauseMenu.HighlightedSlot = 0;

                    pauseMenu.HighlightSlot();

                    canMove = false;
                    return true;
                }
            }
            else
            {
                if (Mathf.Abs(moveY) < 0.25f)
                    canMove = true;
            }

            // SELECT
            bool select = JoystickManager.Instance.playerOne.GetButtonDown("PauseSelect");

            if (select && canSelect)
            {
                if (pauseMenu.HighlightedSlot == 0)
                {
                    GameManager.Instance.Pause();
                    canPause = true;
                }
                else if (pauseMenu.HighlightedSlot == 1)
                    LevelManager.Instance.RestartLevel();
                else if (pauseMenu.HighlightedSlot == 2)
                {
                    JoystickManager.Instance.playerOne = null;
                    GameManager.Instance.UnPause();
                    LevelManager.Instance.BackToMain();
                    canPause = true;
                }

                canSelect = false;
                return true;
            }
            else
            {
                if (!select)
                    canSelect = true;
            }

            // BACK
            bool back = JoystickManager.Instance.playerOne.GetButtonDown("PauseBack") || JoystickManager.Instance.playerOne.GetButtonDown("Pause");

            if (back && canBack)
            {
                GameManager.Instance.Pause();
                canBack = false;
                canPause = true;
                return true;
            }
            else
            {
                if (!back)
                    canBack = true;
            }

            return false;
        }

        return false;
    }
}
