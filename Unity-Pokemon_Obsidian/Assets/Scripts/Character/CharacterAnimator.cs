using System.Collections.Generic;
using UnityEngine;
using Pokemon.Gameplay.Util;

namespace Pokemon.Character
{
    public enum FacingDirection
    {
        Up, Down, Left, Right
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private FacingDirection defaultDirection = FacingDirection.Down;

        [Header("Sprite Settings")]
        [SerializeField] private List<Sprite> walkDownSprites;
        [SerializeField] private List<Sprite> walkUpSprites;
        [SerializeField] private List<Sprite> walkRightSprites;
        [SerializeField] private List<Sprite> walkLeftSprites;

        public FacingDirection DefaultDirection => defaultDirection;

        // Parameters
        public float MoveX { get; set; }
        public float MoveY { get; set; }
        public bool IsMoving { get; set; }

        // States
        private SpriteAnimator _walkDownAnim;
        private SpriteAnimator _walkUpAnim;
        private SpriteAnimator _walkRightAnim;
        private SpriteAnimator _walkLeftAnim;

        private SpriteAnimator _currentAnim;
        private bool _wasPreviouslyMoving;

        // References
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _walkDownAnim = new SpriteAnimator(walkDownSprites, _spriteRenderer);
            _walkUpAnim = new SpriteAnimator(walkUpSprites, _spriteRenderer);
            _walkRightAnim = new SpriteAnimator(walkRightSprites, _spriteRenderer);
            _walkLeftAnim = new SpriteAnimator(walkLeftSprites, _spriteRenderer);

            SetFacingDirection(defaultDirection);

            _currentAnim = _walkDownAnim;
        }

        private void Update()
        {
            var prevAnim = _currentAnim;

            if (MoveX == 1)
            {
                _currentAnim = _walkRightAnim;
            }
            else if(MoveX == -1)
            {
                _currentAnim = _walkLeftAnim;
            }
            else if (MoveY == 1)
            {
                _currentAnim = _walkUpAnim;
            }
            else if (MoveY == -1)
            {
                _currentAnim = _walkDownAnim;
            }
            if(_currentAnim != prevAnim || IsMoving != _wasPreviouslyMoving)
                _currentAnim.Start();

            if (IsMoving)
                _currentAnim.HandleUpdate();
            else
                _spriteRenderer.sprite = _currentAnim.Frames[0];

            _wasPreviouslyMoving = IsMoving;
        }

        public void SetFacingDirection(FacingDirection dir)
        {
            switch (dir)
            {
                case FacingDirection.Right:
                    MoveX = 1;
                    break;
                case FacingDirection.Left:
                    MoveX = -1;
                    break;
                case FacingDirection.Down:
                    MoveY = -1;
                    break;
                case FacingDirection.Up:
                    MoveY = 1;
                    break;
            }
        }
    }
}
