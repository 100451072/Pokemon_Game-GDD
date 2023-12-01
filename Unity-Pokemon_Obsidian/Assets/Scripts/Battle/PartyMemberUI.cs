using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pokemon.Battle
{
    public class PartyMemberUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private HPBar hpBar;

        [SerializeField] private Color highlightedColor;

        private Monsters.Pokemon _pokemon;

        public void SetData(Monsters.Pokemon pokemon)
        {
            _pokemon = pokemon;

            image.sprite = pokemon.Base.FrontSprite;
            nameText.text = pokemon.Base.Name;
            levelText.text = $"Lvl {pokemon.Level}";
            hpBar.SetHp((float)pokemon.Hp / pokemon.MaxHp);
        }

        public void SetSelected(bool selected)
        {
            nameText.color = selected ? highlightedColor : Color.black;
        }
    }
}