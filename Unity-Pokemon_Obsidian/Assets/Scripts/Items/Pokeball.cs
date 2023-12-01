using System.Collections;
using DG.Tweening;
using Pokemon.Monsters;
using UnityEngine;

namespace Pokemon.Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Pokeball : MonoBehaviour
    {
        [SerializeField] private PokeballType type;

        private SpriteRenderer _spriteRenderer;

        public PokeballType Type => type;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public virtual int TryToCatchPokemon(Monsters.Pokemon pokemon)
        {
            // https://bulbapedia.bulbagarden.net/wiki/Catch_rate
            var a = (3 * pokemon.MaxHp - 2 * pokemon.Hp) * pokemon.Base.CatchRate * ConditionDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

            if (a >= 255)
                return 4;

            var b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

            var shakeCount = 0;
            while (shakeCount < 4)
            {
                if (UnityEngine.Random.Range(0, 65535) >= b)
                    break;
                ++shakeCount;
            }
            return shakeCount;
        }

        public IEnumerator PlayThrowAnimation(Vector3 target)
        {
            yield return transform.DOJump(target + new Vector3(0, 2), 2f, 1, 1f)
                .WaitForCompletion();
        }

        public IEnumerator PlayShakeAnimation(int shakeCount)
        {
            // Shake pokeball for max 3 times.
            for (var i = 0; i < Mathf.Min(shakeCount, 3); i++)
            {
                yield return new WaitForSeconds(0.5f);
                yield return transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
            }
        }

        public IEnumerator PlayFadeOutAnimation()
        {
            yield return _spriteRenderer.DOFade(0, 1.5f).WaitForCompletion();
        }
    }
}
