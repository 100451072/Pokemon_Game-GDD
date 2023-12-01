using System;
using Pokemon.Gameplay.Util;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Pokemon.Character
{
    [Serializable]
    public class EnterTrainerViewEvent : UnityEvent<Collider2D> { }

    [Serializable]
    public class InteractEvent: UnityEvent<Interactable>{ }

    public class PlayerController : CharacterController
    {
        [Header("Player Settings")]
        [Range(0, 100)]
        [SerializeField] private int encounterProbability = 10;
        
        public UnityEvent OnEncountered;
        public EnterTrainerViewEvent OnEnterTrainersView;
        public InteractEvent OnInteract;

        public bool IsOnLongGrass => 
            Physics2D.OverlapCircle(transform.position, overlapCheckSize, GameLayers.Instance.LongGrassLayer) != null;

        public bool IsInTrainerView =>
            Physics2D.OverlapCircle(transform.position, overlapCheckSize, GameLayers.Instance.FovLayer) != null;

        public override void HandleUpdate()
        {
            if (!IsMoving)
            {
                Vector2 input;
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");

                // remove diagonal movement
                if (input.x != 0) input.y = 0;

                if (input != Vector2.zero)
                {
                    StartCoroutine(Move(input, OnMoveOver));
                }
            }

            base.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Z))
                Interact();
        }

        private void Interact()
        {
            var interactPos = transform.position + FacingDir;
            //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);

            var interactableObject = Physics2D.OverlapCircle(interactPos, overlapCheckSize, GameLayers.Instance.InteractableLayer);
            if (interactableObject == null) return;

            var interactable = interactableObject.GetComponent<Interactable>();
            OnInteract?.Invoke(interactable);
            interactable?.Interact(transform);

        }

        private void OnMoveOver()
        {
            CheckForEncounters();
            CheckIfInTrainersView();
        }

        private void CheckForEncounters()
        {
            if (!IsOnLongGrass) return;
            if (Random.Range(0, 100) > encounterProbability) return;

            Debug.Log("Encounter a wild pokemon");
            _animator.IsMoving = false;
            OnEncountered?.Invoke();
        }

        private void CheckIfInTrainersView()
        {
            var trainerCollider = Physics2D.OverlapCircle(transform.position, overlapCheckSize, GameLayers.Instance.FovLayer);
            if (trainerCollider == null) return;

            _animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(trainerCollider);
        }
    }
}