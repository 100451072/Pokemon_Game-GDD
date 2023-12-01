using Pokemon.Battle;
using Pokemon.Gameplay.Dialog;
using Pokemon.Monsters;
using Pokemon.Character;
using UnityEngine;

namespace Pokemon.Gameplay
{
    public enum GameState
    {
        FreeRoam,
        Battle,
        Dialog,
        Cutscene
    }

    public class GameController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private BattleSystem battleSystem;
        [SerializeField] private Camera worldCamera;

        private GameState _state;
        private MapArea _mapArea;

        private TrainerController _trainer;

        public GameController Instance { get; private set; }

        private void Awake()
        {
            if(Instance == null)
                Instance = this;
            else
            {
                Destroy(this);
                return;
            }

            ConditionDB.Init();
        }

        private void Start()
        {
            _mapArea = FindObjectOfType<MapArea>();

            playerController.OnEncountered.AddListener(StartBattle);
            battleSystem.OnBattleOver.AddListener(EndBattle);

            playerController.OnEnterTrainersView.AddListener((trainerCollider) =>
            {
                var trainer = trainerCollider.GetComponentInParent<TrainerController>();
                if (trainer == null) return;

                _state = GameState.Cutscene;

                trainer.StartBattleAction = StartTrainerBattle;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            });

            playerController.OnInteract.AddListener(interactable =>
            {
                if(interactable is TrainerController trainer)
                    trainer.StartBattleAction = StartTrainerBattle;
            });

            DialogManager.Instance.OnShowDialog.AddListener(() => _state = GameState.Dialog);
            DialogManager.Instance.OnCloseDialog.AddListener(() => _state = _state == GameState.Dialog ? GameState.FreeRoam : _state);
        }

        private void Update()
        {
            switch (_state)
            {
                case GameState.FreeRoam:
                    playerController.HandleUpdate();
                    break;
                case GameState.Battle:
                    battleSystem.HandleUpdate();
                    break;
                case GameState.Dialog:
                    DialogManager.Instance.HandleUpdate();
                    break;
            }
        }

        private void StartBattle()
        {
            _state = GameState.Battle;
            battleSystem.gameObject.SetActive(true);
            worldCamera.gameObject.SetActive(false);

            var playerParty = playerController.GetComponent<PokemonParty>();
            var wildPokemon = _mapArea.GetRandomWildPokemon();

            var wildPokemonCopy = new Monsters.Pokemon(wildPokemon.Base, wildPokemon.Level);
            battleSystem.StartBattle(playerParty, wildPokemonCopy);
        }

        public void StartTrainerBattle(TrainerController trainer)
        {
            _state = GameState.Battle;
            battleSystem.gameObject.SetActive(true);
            worldCamera.gameObject.SetActive(false);

            _trainer = trainer;
            var playerParty = playerController.GetComponent<PokemonParty>();
            var trainerParty = trainer.GetComponent<PokemonParty>();

            battleSystem.StartTrainerBattle(playerParty, trainerParty);
        }

        private void EndBattle(bool won)
        {
            if (_trainer != null && won)
            {
                _trainer.BattleLost();
                _trainer = null;
            }

            _state = GameState.FreeRoam;
            battleSystem.gameObject.SetActive(false);
            worldCamera.gameObject.SetActive(true);
        }
    }
}
