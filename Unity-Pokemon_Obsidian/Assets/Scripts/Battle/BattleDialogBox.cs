using System.Collections;
using System.Collections.Generic;
using Pokemon.Moves;
using TMPro;
using UnityEngine;

namespace Pokemon.Battle
{
    public class BattleDialogBox : MonoBehaviour
    {
        [SerializeField] private int lettersPerSecond;
        [SerializeField] private Color highlightedColor;

        [SerializeField] private TMP_Text dialogText;
        [SerializeField] private GameObject actionSelector;
        [SerializeField] private GameObject moveSelector;
        [SerializeField] private GameObject moveDetails;
        [SerializeField] private GameObject choiceBox;

        [SerializeField] private List<TMP_Text> actionTexts;
        [SerializeField] private List<TMP_Text> moveTexts;

        [SerializeField] private TMP_Text ppText;
        [SerializeField] private TMP_Text typeText;

        [SerializeField] private TMP_Text yesText;
        [SerializeField] private TMP_Text noText;

        public void SetDialog(string dialog)
        {
            dialogText.text = dialog;
        }

        public IEnumerator TypeDialog(string dialog)
        {
            dialogText.text = string.Empty;
            foreach (var letter in dialog.ToCharArray())
            {
                dialogText.text += letter;
                yield return new WaitForSeconds(1f/lettersPerSecond);
            }
            yield return new WaitForSeconds(1f);
        }

        public void EnableDialogText(bool enabled)
        {
            dialogText.enabled = enabled;
        }

        public void EnableActionSelector(bool enabled)
        {
            actionSelector.SetActive(enabled);
        }

        public void EnableMoveSelector(bool enabled)
        {
            moveSelector.SetActive(enabled);
            moveDetails.SetActive(enabled);
        }

        public void EnableChoiceBox(bool enabled)
        {
            choiceBox.SetActive(enabled);
        }

        public void UpdateActionSelection(int selectedAction)
        {
            for (var actionIndex = 0; actionIndex < actionTexts.Count; actionIndex++)
            {
                actionTexts[actionIndex].color = actionIndex == selectedAction ? highlightedColor : Color.black;
            }
        }

        public void SetMoveNames(List<Move> moves)
        {
            for (var moveIndex = 0; moveIndex < moveTexts.Count; moveIndex++)
            {
                moveTexts[moveIndex].text = moveIndex < moves.Count ? moves[moveIndex].Base.Name : "-";
            }
        }

        public void UpdateMoveSelection(int selectedMove, Move move)
        {
            for (var moveIndex = 0; moveIndex < moveTexts.Count; moveIndex++)
            {
                moveTexts[moveIndex].color = moveIndex == selectedMove ? highlightedColor : Color.black;
            }

            ppText.text = $"PP {move.Pp}/{move.Base.Pp}";
            ppText.color = move.Pp <= 0 ? Color.red : Color.black;

            typeText.text = move.Base.Type.ToString();
        }

        public void UpdateChoiceBox(bool yesSelected)
        {
            if (yesSelected)
            {
                yesText.color = highlightedColor;
                noText.color = Color.black;
            }
            else
            {
                yesText.color = Color.black;
                noText.color = highlightedColor;
            }
        }
    }
}