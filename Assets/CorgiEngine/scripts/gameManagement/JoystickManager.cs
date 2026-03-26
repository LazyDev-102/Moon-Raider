using UnityEngine;
using System.Collections;
using Rewired;
using System.Collections.Generic;


public class JoystickManager : PersistentSingleton<JoystickManager>
{
    public Player playerOne, playerTwo;

    public enum JoystickMode: int
    {
        GamePlay = 0,
        MainMenu = 1,
        Story = 2
    };

    public JoystickMode Mode = JoystickMode.Story;


    public bool JoystickConnected
    {
        get
        {
            if (!ReInput.isReady)
            {
                return false;
            }
            else
            {
#if UNITY_TVOS
                return (ReInput.controllers.joystickCount > 1);
#else
                return (ReInput.controllers.joystickCount > 0);
#endif
            }
        }
    }



    // Use this for initialization
    void Start()
    {
        ReInput.ControllerConnectedEvent += OnControllerConnected;

        if (ReInput.isReady)
        {
            for (int i = 0; i < ReInput.controllers.joystickCount; i++)
                AssignJoystickToNextOpenPlayer(ReInput.controllers.Joysticks[i]);
        }

        Reboot();
    }

    public void Reboot()
    {
        // Keyboard
        if (ReInput.isReady)
        {
            if (ReInput.players.allPlayerCount > 0)
            {
                playerOne = ReInput.players.GetPlayer(0);
                ResetPlayerOne();
            }

            Debug.Log(ReInput.players.Players.Count + " players connected");
        }
    }


    public void Reset()
    {
        if (playerOne != null)
        {
            if (playerOne.controllers.Joysticks.Count > 0)
                playerOne.controllers.maps.LoadMap(ControllerType.Joystick, playerOne.controllers.Joysticks[0].id, (int)Mode, 0, true);

#if UNITY_STANDALONE
            playerOne.controllers.maps.LoadMap(ControllerType.Keyboard, 0, (int)Mode, 0, true);
#endif
        }

        if (playerTwo != null)
        {
            if(playerTwo.controllers.Joysticks.Count > 0)
                playerTwo.controllers.maps.LoadMap(ControllerType.Joystick, playerTwo.controllers.Joysticks[0].id, (int)Mode, 0, true);
        }
    }


    public void ResetPlayerOne()
    {
        if (ReInput.isReady)
        {
            Debug.Log(ReInput.players.playerCount + " players connected");

#if UNITY_STANDALONE
            playerOne.controllers.maps.LoadMap(ControllerType.Keyboard, 0, (int)Mode, 0, true);
#endif

            if (playerOne.controllers.Joysticks.Count > 0)
            {
                Debug.Log("Loading Map for joystick " + playerOne.controllers.Joysticks[0].hardwareName + " to player 1 with mode " + (int)Mode);
                // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
                playerOne.controllers.maps.LoadMap(ControllerType.Joystick, playerOne.controllers.Joysticks[0].id, (int)Mode, 0, true);
            }
        }
    }


    public void ResetPlayerTwo()
    {
        if (ReInput.isReady)
        {
            Debug.Log(ReInput.players.playerCount + " players connected");

            // Is the issue that this gets called on P1 with remote already loaded? So this should only be explicitly called
            // after assign has a proper input?
            if (ReInput.players.playerCount > 1)
            {
                if (playerTwo.controllers.Joysticks.Count > 0 && ReInput.players.playerCount > 1)
                {
                    Debug.Log("Loading Map for joystick " + playerTwo.controllers.Joysticks[0].hardwareName + " to player 2 with mode " + (int)Mode);

                    playerTwo.controllers.maps.LoadMap(ControllerType.Joystick, playerTwo.controllers.Joysticks[0].id, (int)Mode, 0, true);
                }
            }
        }
    }


    // This will be called when a controller is connected
    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        if (args.controllerType != ControllerType.Joystick)
            return; // skip if this isn't a Joystick

        Joystick j = ReInput.controllers.GetJoystick(args.controllerId);

#if UNITY_TVOS
        if (j.name.Contains("Remote") || j.hardwareName.Contains("Remote"))
        {
            Debug.Log("Ignoring " + j.name);
            return;
        }
#endif

        // Assign Joystick to first Player that doesn't have any assigned
        AssignJoystickToNextOpenPlayer(j);
        Debug.Log(ReInput.controllers.joystickCount + " joysticks connected");
    }


    void AssignJoystickToNextOpenPlayer(Joystick j)
    {
        playerOne = ReInput.players.GetPlayer(0);
        if (playerOne != null)
        {
            if (playerOne.controllers.joystickCount == 0)
            {
                Debug.Log("Assigning " + j.name + " to player 1");
                playerOne.controllers.AddController(j, true); // assign joystick to player
                ResetPlayerOne();
                return;
            }
        }

        playerTwo = ReInput.players.GetPlayer(1);
        if (playerTwo != null)
        {
            if (playerTwo.controllers.joystickCount == 0)
            {
                Debug.Log("Assigning " + j.name + " to player 2");
                playerTwo.controllers.AddController(j, true); // assign joystick to player
                ResetPlayerTwo();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GUIManager.Instance == null)
            return;

#if UNITY_IOS || UNITY_ANDROID
        if (ReInput.controllers.joystickCount > 0)
        {
            GUIManager.Instance.SetMobileControlsActive(false);
        }
        else
        {
            GUIManager.Instance.SetMobileControlsActive(GameManager.Instance.CanMove && GameManager.Instance.Player != null);
            return;
        }
#endif
    }
}
