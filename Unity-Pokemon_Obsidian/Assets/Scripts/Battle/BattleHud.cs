using System;
using System.Collections;
using System.Linq;
using Pokemon.Types;
using TMPro;
using UnityEngine;

namespace Pokemon.Battle
{
    [Serializable]
    public class StatusColor
    {
        public ConditionType ConditionType;
        public Color Color;
    }

    public class BattleHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private HPBar hpBar;

        [SerializeField] private StatusColor[] statusColors;

        private Monsters.Pokemon _pokemon;

        public void SetData(Monsters.Pokemon pokemon)
        {
            _pokemon = pokemon;
            nameText.text = pokemon.Base.Name;
            levelText.text = $"Lvl {pokemon.Level}";
            hpBar.SetHp((float) pokemon.Hp / pokemon.MaxHp);

            SetStatusText();
            _pokemon.OnStatusChanged.AddListener(SetStatusText);
        }

        private void SetStatusText()
        {
            if (_pokemon.Status == null)
            {
                statusText.text = string.Empty;
                statusText.gameObject.SetActive(false);
            }
            else
            {
                statusText.text = _pokemon.Status.Type.ToString().ToUpper();
                statusText.color = statusColors.FirstOrDefault(statusColor => statusColor.ConditionType == _pokemon.Status.Type)!.Color;
                statusText.gameObject.SetActive(true);
            }
        }

        public IEnumerator UpdateHp()
        {
            if (_pokemon.HpChanged)
            {
                yield return hpBar.SetHpSmooth((float)_pokemon.Hp / _pokemon.MaxHp);
                _pokemon.HpChanged = false;
            }
        }

    }
}