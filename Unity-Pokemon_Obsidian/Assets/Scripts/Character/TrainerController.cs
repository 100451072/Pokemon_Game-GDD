using System;
using System.Collections;
using Pokemon.Gameplay.Dialog;
using Pokemon.Gameplay.Util;
using UnityEngine;

namespace Pokemon.Character
{
    public class TrainerController : CharacterController, Interactable
    {
        [Header("Trainer Settings")] [SerializeField]
        private GameObject exclamation;

        [SerializeField] private GameObject fov;
        [SerializeField] private Dialog dialog;
        [SerializeField] private Dialog dialogAfterBattle;

        // State
        private bool _battleLost;

        public Action<TrainerController> StartBattleAction { get; set; }

        protected override void Start()
        {
            base.Start();
            SetFovRotation(_animator.DefaultDirection);
        }

        private void Update()
        {
            HandleUpdate();
        }

        public void Interact(Transform initiator)
        {
            LookTowards(initiator.position);

            if (!_battleLost)
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
                {
                    Debug.Log("Starting Trainer Battle");
                    StartBattleAction?.Invoke(this);
                }));
            }
            else
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
            }
        }

        public IEnumerator TriggerTrainerBattle(PlayerController player)
        {
            // Show Exclamation
            exclamation.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            exclamation.SetActive(false);

            // Walk towards the player
            var diff = player.transform.position - transform.position;
            var moveVec = diff - diff.normalized;
            moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

            yield return Move(moveVec);

            // Show dialog
            yield return DialogManager.Instance.ShowDialog(dialog, () =>
            {
                Debug.Log("Starting Trainer Battle");
                StartBattleAction?.Invoke(this);
            });
        }

        public void BattleLost()
        {
            _battleLost = true;
            fov.gameObject.SetActive(false);
        }

        public void SetFovRotation(FacingDirection dir)
        {
            var angle = dir switch
            {
                FacingDirection.Right => 90f,
                FacingDirection.Up => 180f,
                FacingDirection.Left => 270,
                _ => 0f
            };

            fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
        }
    }
}