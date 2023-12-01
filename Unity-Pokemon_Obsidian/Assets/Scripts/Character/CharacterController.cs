using System;
using System.Collections;
using UnityEngine;
using Pokemon.Gameplay.Util;

namespace Pokemon.Character
{
    [RequireComponent(typeof(CharacterAnimator))]
    public abstract class CharacterController : MonoBehaviour
    {
        [Header("Character Settings")]
        [SerializeField] private string name;
        [SerializeField] private Sprite sprite;

        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float overlapCheckSize = 0.2f;

        protected CharacterAnimator _animator;

        public string Name => name;
        public Sprite Sprite => sprite;

        public bool IsMoving { get; protected set; }

        public Vector3 FacingDir => new(_animator.MoveX, _animator.MoveY);

        protected virtual void Start()
        {
            _animator = GetComponent<CharacterAnimator>();
        }

        protected IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null)
        {
            _animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
            _animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

            var targetPos = transform.position;
            targetPos.x += moveVector.x;
            targetPos.y += moveVector.y;

            if (!IsPathClear(targetPos))
                yield break;

            IsMoving = true;

            while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;

            IsMoving = false;

            OnMoveOver?.Invoke();
        }

        public virtual void HandleUpdate()
        {
            _animator.IsMoving = IsMoving;
        }

        public void LookTowards(Vector3 targetPos)
        {
            var xDiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
            var yDiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

            if (xDiff == 0 || yDiff == 0)
            {
                _animator.MoveX = Mathf.Clamp(xDiff, -1f, 1f);
                _animator.MoveY = Mathf.Clamp(yDiff, -1f, 1f);
            }
            else
                Debug.LogError("Error in Look Towards: You can't ask the character to look diagonally");
        }

        protected bool IsPathClear(Vector3 targetPos)
        {
            var diff = targetPos - transform.position;
            var dir = diff.normalized;


            return !Physics2D.BoxCast(transform.position + dir, new Vector2(overlapCheckSize, overlapCheckSize), 0f,
                dir, diff.magnitude - 1,
                GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer |
                GameLayers.Instance.PlayerLayer);
        }

        protected bool IsWalkable(Vector3 targetPos)
        {
            return Physics2D.OverlapCircle(targetPos, overlapCheckSize, 
                GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer) == null;
        }
    }
}
