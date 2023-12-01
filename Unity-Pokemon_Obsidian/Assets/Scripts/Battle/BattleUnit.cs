using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Pokemon.Battle
{
    public class BattleUnit : MonoBehaviour
    {
        [SerializeField] private bool isPlayerUnit;
        [SerializeField] private BattleHud hud;

        public bool IsPlayerUnit => isPlayerUnit;

        public BattleHud Hud => hud;

        public Monsters.Pokemon Pokemon { get; set; }

        private Image _image;
        private Vector3 _originalPos;
        private Vector2 _originalAnchoredPos;
        private Color _originalColor;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _originalPos = _image.transform.localPosition;
            _originalAnchoredPos = _image.rectTransform.anchoredPosition;
            _originalColor = _image.color;
        }

        public void Setup(Monsters.Pokemon pokemon)
        {
            Pokemon = pokemon;

            transform.localScale = Vector3.one;

            _image.sprite = isPlayerUnit ? Pokemon.Base.BackSprite : Pokemon.Base.FrontSprite;
            _image.color = _originalColor;
            
            hud.gameObject.SetActive(true);
            hud.SetData(pokemon);
            
            PlayEnterAnimation();
        }

        public void Clear()
        {
            hud.gameObject.SetActive(false);
        }

        public void PlayEnterAnimation()
        {
            _image.transform.localPosition = isPlayerUnit ? new Vector3(-500f, _originalPos.y) : new Vector3(500f,  _originalPos.y);
            _image.rectTransform.DOAnchorPos(_originalAnchoredPos, 1f);
        }

        public void PlayAttackAnimation()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(isPlayerUnit
                ? _image.transform.DOLocalMoveX(_originalPos.x + 50f, 0.25f)
                : _image.transform.DOLocalMoveX(_originalPos.x - 50f, 0.25f));

            sequence.Append(_image.transform.DOLocalMoveX(_originalPos.x, 0.25f));
        }

        public void PlayHitAnimation()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_image.DOColor(Color.gray, 0.1f));
            sequence.Append(_image.DOColor(_originalColor, 0.1f));
        }

        public void PlayFaintAnimation()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_image.transform.DOLocalMoveY(_originalPos.y - 150f, 0.5f));
            sequence.Join(_image.DOFade(0f, 0.5f));
        }

        public void PlayReturnAnimation()
        {
            PlayFaintAnimation(); // same Animation for Return
        }

        public IEnumerator PlayCaptureAnimation()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_image.DOFade(0, 0.5f));
            sequence.Join(transform.DOLocalMoveY(_originalPos.y + 50f, 0.5f));
            sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
            yield return sequence.WaitForCompletion();
        }

        public IEnumerator PlayBreakOutAnimation()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_image.DOFade(1, 0.5f));
            sequence.Join(transform.DOLocalMoveY(_originalPos.y, 0.5f));
            sequence.Join(transform.DOScale(Vector3.one, 0.5f));
            yield return sequence.WaitForCompletion();
        }
    }
}