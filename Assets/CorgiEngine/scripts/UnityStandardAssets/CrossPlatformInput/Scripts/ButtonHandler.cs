using UnityEngine;
using UnityEngine.UI;

namespace UnitySampleAssets.CrossPlatformInput
{
    public class ButtonHandler : MonoBehaviour
    {
        protected static CharacterBehavior _player;
        protected static CharacterShoot _shoot;

        public Image ButtonImage;
        private Color originalColor;

        bool checkPlayer()
        {
#if UNITY_STANDALONE
            return true;
#else
            if (_player == null)
            {
                if (GameManager.Instance.Player != null)
                {
                    if (GameManager.Instance.Player.GetComponent<CharacterBehavior>() != null)
                    {
                        _player = GameManager.Instance.Player;
                        _shoot = _player.GetComponent<CharacterShoot>();
                        return true;
                    }
                }
                else
                {
                    //Debug.Log("NULL player!");
                    if(GameObject.FindWithTag("Player") != null)
                        GameManager.Instance.Player = GameObject.FindWithTag("Player").GetComponent<CharacterBehavior>();
                }
            }
            else
            {
                return true;
            }

            return false;
#endif
        }


        private void Start()
        {
            originalColor = ButtonImage.color;
        }

        public void Update()
        {
            checkPlayer();
        }


        public void SetDownState(string name)
        {
#if UNITY_STANDALONE
            return;
#else
            if (_player.BehaviorState.MeleeEnergized)
                return;
            
            if (name == "Jump")
                _player.JumpStart();
            else if (name == "Fire")
            {
                _shoot.ShootOnce();
                _shoot.ShootStart();
            }
            else if (name == "Pickup")
                _player.Pickup();
            else if (name == "Melee")
            {
                if (_player.Permissions.MeleeAttackEnabled && GameManager.Instance.Points > 0)
                    _player.BehaviorState.MeleeEnergized = _player.BehaviorState.CanMelee;
                else
                {
                    _player.BehaviorState.MeleeEnergized = false;

                    if (_player.BehaviorState.CoveredInSpores)
                        StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.cyan));
                    else
                        StartCoroutine(GUIManager.Instance.HealthBar.Flicker(Color.yellow));

                    _player.PlayMeleeErrorSound();
                }
            }

            ButtonImage.color = new Color(1, 1, 1, 0.25f);
#endif
        }


        public void SetUpState(string name)
        {
#if UNITY_STANDALONE
            return;
#else
            if (name == "Jump")
                _player.JumpStop();
            else if (name == "Fire")
                _shoot.ShootStop();
            else if (name == "Melee")
                _player.BehaviorState.MeleeEnergized = false;

            ButtonImage.color = originalColor;
#endif
        }


        public void SetAxisPositiveState(string name)
        {
            //CrossPlatformInputManager.SetAxisPositive(name);
        }


        public void SetAxisNeutralState(string name)
        {
            //CrossPlatformInputManager.SetAxisZero(name);
        }


        public void SetAxisNegativeState(string name)
        {
            //CrossPlatformInputManager.SetAxisNegative(name);
        }
    }
}