using System.Collections.Generic;
using TMPro;
using UnityEngine;
// Import Language Manager
using GEAR.Localization;

namespace Pokemon.Battle
{
    public class PartyScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;

        private PartyMemberUI[] _memberSlots;
        private List<Monsters.Pokemon> _pokemons;

        public void Init()
        {
            _memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        }

        public void SetPartyData(List<Monsters.Pokemon> pokemons)
        {
            _pokemons = pokemons;

            for (var slotIndex = 0; slotIndex < _memberSlots.Length; slotIndex++)
            {
                if (slotIndex < pokemons.Count)
                {
                    _memberSlots[slotIndex].gameObject.SetActive(true);
                    _memberSlots[slotIndex].SetData(pokemons[slotIndex]);
                }
                else
                    _memberSlots[slotIndex].gameObject.SetActive(false);
            }

            //messageText.text = "Choose a Pokemon";
            messageText.text = LanguageManager.Instance.GetString("PartyScreen_1");
        }

        public void UpdateMemberSelection(int selectedMember)
        {
            for (var pokeIndex = 0; pokeIndex < _pokemons.Count; pokeIndex++)
            {
                _memberSlots[pokeIndex].SetSelected(pokeIndex == selectedMember);
            }
        }

        public void SetMessageText(string message)
        {
            messageText.text = message;
        }
    }
}
