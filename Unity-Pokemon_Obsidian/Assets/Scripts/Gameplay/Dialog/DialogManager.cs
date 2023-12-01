using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Pokemon.Gameplay.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        [SerializeField] private GameObject dialogBox;
        [SerializeField] private TMP_Text dialogText;
        [SerializeField] private int lettersPerSecond;

        private Dialog _dialog;
        private Action _onDialogFinished;

        private int _currentLine;
        private bool _isTyping;

        public bool IsShowing { get; private set; }

        public UnityEvent OnShowDialog;
        public UnityEvent OnCloseDialog;

        public static DialogManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public IEnumerator ShowDialog(Dialog dialog, Action onFinished = null)
        {
            yield return new WaitForEndOfFrame();

            OnShowDialog?.Invoke();

            IsShowing = true;
            _dialog = dialog;
            _onDialogFinished = onFinished;

            dialogBox.SetActive(true);
            StartCoroutine(TypeDialog(dialog.Lines[0]));
        }

        public void HandleUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Z) && !_isTyping)
            {
                if (++_currentLine < _dialog.Lines.Count)
                {
                    StartCoroutine(TypeDialog(_dialog.Lines[_currentLine]));
                }
                else
                {
                    _currentLine = 0;
                    IsShowing = false;
                    dialogBox.SetActive(false);

                    _onDialogFinished?.Invoke();
                    OnCloseDialog?.Invoke();
                }
            }
        }

        public IEnumerator TypeDialog(string line)
        {
            _isTyping = true;
            dialogText.text = string.Empty;
            foreach (var letter in line.ToCharArray())
            {
                dialogText.text += letter;
                yield return new WaitForSeconds(1f / lettersPerSecond);
            }
            _isTyping = false;
        }
    } 
}
