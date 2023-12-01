using System.Collections;
using System.Collections.Generic;
using Pokemon.Gameplay.Util;
using Pokemon.Gameplay.Dialog;
using UnityEngine;

namespace Pokemon.Character
{
    public enum NPCState
    {
        Idle,
        Walking,
        Dialog
    }

    public class NpcController : CharacterController, Interactable
    {
        [Header("NPC Settings")]
        [SerializeField] private List<Vector2> movementPattern;
        [SerializeField] private float timeBetweenPattern;
        [SerializeField] private Dialog dialog;

        private NPCState _state;
        private float _idleTimer;
        private int _currentPattern;

        private void Update()
        {
            if (_state == NPCState.Idle)
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer > timeBetweenPattern)
                {
                    _idleTimer = 0;
                    if(movementPattern.Count > 0)
                        StartCoroutine(Walk());
                }
            }
            HandleUpdate();
        }

        private IEnumerator Walk()
        {
            _state = NPCState.Walking;

            var oldPos = transform.position;

            yield return Move(movementPattern[_currentPattern]);

            if(transform.position != oldPos)
                _currentPattern = (_currentPattern + 1) % movementPattern.Count;

            _state = NPCState.Idle;
        }

        public void Interact(Transform initiator)
        {
            if (_state == NPCState.Idle)
            {
                _state = NPCState.Dialog;
                LookTowards(initiator.position);

                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => 
                {
                    _idleTimer = 0;
                    _state = NPCState.Idle;
                }));
            }
        }
    }
}
