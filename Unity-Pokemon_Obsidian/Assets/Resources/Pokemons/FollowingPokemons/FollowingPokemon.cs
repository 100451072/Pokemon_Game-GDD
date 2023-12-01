using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon.Gameplay.Util;

public enum FacingDirection
    {
        Up, Down, Left, Right
    }

public class FollowingPokemon : MonoBehaviour
{
    public int speed;
    public GameObject player;
    public float minRadius;
    private Vector3 previousPosition;

    public bool IsMoving { get; set; }

    [Header("Sprite Settings")]
    [SerializeField] private List<Sprite> walkDownSprites;
    [SerializeField] private List<Sprite> walkUpSprites;
    [SerializeField] private List<Sprite> walkRightSprites;
    [SerializeField] private List<Sprite> walkLeftSprites;
 
    // States
    private SpriteAnimator _walkDownAnim;
    private SpriteAnimator _walkUpAnim;
    private SpriteAnimator _walkRightAnim;
    private SpriteAnimator _walkLeftAnim;

    private SpriteAnimator _currentAnim;
    private bool _wasPreviouslyMoving;

    // References
    private SpriteRenderer _spriteRenderer;

    public Vector3 _movementDirection;
    public Vector3 _offset = Vector3.zero;
    public float offsetValue = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        previousPosition = player.transform.position;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        _walkDownAnim = new SpriteAnimator(walkDownSprites, _spriteRenderer);
        _walkUpAnim = new SpriteAnimator(walkUpSprites, _spriteRenderer);
        _walkRightAnim = new SpriteAnimator(walkRightSprites, _spriteRenderer);
        _walkLeftAnim = new SpriteAnimator(walkLeftSprites, _spriteRenderer);

        _currentAnim = _walkDownAnim;

        ChangePosAndSprite();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        _movementDirection = player.transform.position - previousPosition;

        // Move the pokemon to the position of the player without colliding
        if (distance > minRadius)
        {
            Vector3 targetPosition = player.transform.position + _offset;
            Vector3 localPosition = targetPosition - transform.position;
            localPosition = localPosition.normalized;
            transform.Translate(localPosition.x * Time.deltaTime * speed, localPosition.y * Time.deltaTime * speed, localPosition.z * Time.deltaTime * speed);
        }

        ChangePosAndSprite();
        previousPosition = player.transform.position;
    }
    
    public void ChangePosAndSprite()
    {
        var prevAnim = _currentAnim;

        // Move right
        if (_movementDirection.x > 0)
        {
            _currentAnim = _walkRightAnim;
            _offset = new Vector3(-offsetValue, 0, 0); // Move Pokémon left
        }
        // Move left
        else if(_movementDirection.x < 0)
        {
            _currentAnim = _walkLeftAnim;
            _offset = new Vector3(offsetValue, 0, 0); // Move Pokémon right
        }
        // Move up
        else if (_movementDirection.y > 0)
        {
            _currentAnim = _walkUpAnim;
            _offset = new Vector3(0, -offsetValue, 0); // Move Pokémon down
        }
        // Move down
        else if (_movementDirection.y < 0)
        {
            _currentAnim = _walkDownAnim;
            _offset = new Vector3(0, offsetValue, 0); // Move Pokémon up
        }
        if (_movementDirection != Vector3.zero)
            IsMoving = true;
        if(_currentAnim != prevAnim || IsMoving != _wasPreviouslyMoving)
            _currentAnim.Start();

        if (IsMoving)
        {
            _currentAnim.HandleUpdate();
        }
        else
            _spriteRenderer.sprite = _currentAnim.Frames[0];

        _wasPreviouslyMoving = IsMoving;

    }
}
