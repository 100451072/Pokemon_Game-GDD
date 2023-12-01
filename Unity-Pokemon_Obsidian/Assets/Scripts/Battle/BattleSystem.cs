using System;
using System.Collections;
using DG.Tweening;
using Pokemon.Monsters;
using Pokemon.Moves;
using Pokemon.Stats;
using Pokemon.Types;
using Pokemon.Character;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Pokemon.Items;
using Random = UnityEngine.Random;
// Import for the language manager
using GEAR.Localization;

namespace Pokemon.Battle
{
    public enum BattleState
    {
        Start,
        ActionSelection,
        MoveSelection,
        RunningTurn,
        Busy,
        PartyScreen,
        AboutToUse,
        BattleOver
    }

    public enum BattleAction
    {
        Move,
        UseItem,
        SwitchPokemon,
        Run
    }

    // Unity Event for handling the battle over event.
    // Returns true if player won the battle, otherwise false
    [Serializable] public class BattleOverEvent : UnityEvent<bool> { }

    public class BattleSystem : MonoBehaviour
    {
        [Header("Player/Trainer Settings")]
        [SerializeField] private Image playerImage;
        [SerializeField] private Image trainerImage;

        [Header("Battle Unit Settings")]
        [SerializeField] private BattleUnit playerUnit;
        [SerializeField] private BattleUnit enemyUnit;

        [Header("Screen Settings")]
        [SerializeField] private BattleDialogBox dialogBox;
        [SerializeField] private PartyScreen partyScreen;

        [Header("Item Settings")]
        [SerializeField] private GameObject pokeballPrefab;

        private BattleState _state;
        private BattleState? _prevState;

        private int _currentAction;
        private int _currentMove;
        private int _currentMember;
        private bool aboutToUseChoice = true;

        private BattleAction CurrentAction => (BattleAction)_currentAction;

        private PokemonParty _playerParty;
        private PokemonParty _trainerParty;
        private Monsters.Pokemon _wildPokemon;

        private bool _isTrainerBattle;
        private PlayerController _player;
        private TrainerController _trainer;

        private int _escapeAttemps;

        public BattleOverEvent OnBattleOver;

        public void StartBattle(PokemonParty playerParty, Monsters.Pokemon wildPokemon)
        {
            _playerParty = playerParty;
            _wildPokemon = wildPokemon;

            _isTrainerBattle = false;

            _player = playerParty.GetComponent<PlayerController>();

            StartCoroutine(SetupBattle());
        }

        public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
        {
            _playerParty = playerParty;
            _trainerParty = trainerParty;

            _isTrainerBattle = true;

            _player = playerParty.GetComponent<PlayerController>();
            _trainer = trainerParty.GetComponent<TrainerController>();

            StartCoroutine(SetupBattle());
        }

        public void HandleUpdate()
        {
            switch (_state)
            {
                case BattleState.ActionSelection:
                    HandleActionSelection();
                    break;
                case BattleState.MoveSelection:
                    HandleMoveSelection();
                    break;
                case BattleState.PartyScreen:
                    HandlePartyScreenSelection();
                    break;
                case BattleState.AboutToUse:
                    HandleAboutToUse();
                    break;
            }
        }

        public IEnumerator SetupBattle()
        {
            playerUnit.Clear();
            enemyUnit.Clear();

            if (!_isTrainerBattle)
            {
                // Wild Pokemon Battle
                playerUnit.Setup(_playerParty.GetHealthyPokemon());
                enemyUnit.Setup(_wildPokemon);

                // Need for language manager
                //yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_1"), enemyUnit.Pokemon.Base.Name));
            }
            else
            {
                // Trainer Battle

                // Show trainer and player sprites
                playerUnit.gameObject.SetActive(false);
                enemyUnit.gameObject.SetActive(false);

                playerImage.gameObject.SetActive(true);
                trainerImage.gameObject.SetActive(true);

                playerImage.sprite = _player.Sprite;
                trainerImage.sprite = _trainer.Sprite;

                // Need for language manager
                //yield return dialogBox.TypeDialog($"{_trainer.name} wants to battle");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_2"), _trainer.name));

                // Send out first pokemon of the trainer
                trainerImage.gameObject.SetActive(false);
                enemyUnit.gameObject.SetActive(true);
                var enemyPokemon = _trainerParty.GetHealthyPokemon();
                enemyUnit.Setup(enemyPokemon);
                // Need for language manager
                //yield return dialogBox.TypeDialog($"{_trainer.Name} send out {enemyPokemon.Base.Name}");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_3"), _trainer.Name, enemyPokemon.Base.Name));

                // Send out first pokemon of the player
                playerImage.gameObject.SetActive(false);
                playerUnit.gameObject.SetActive(true);
                var playerPokemon = _playerParty.GetHealthyPokemon();
                playerUnit.Setup(playerPokemon);
                // Need for language manager
                //yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_4"), playerPokemon.Base.Name));
            }

            _escapeAttemps = 0;
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            partyScreen.Init();
            ActionSelection();
        }

        private void BattleOver(bool won)
        {
            _state = BattleState.BattleOver;
            _playerParty.Pokemons.ForEach(p => p.OnBattleOver());
            OnBattleOver?.Invoke(won);
        }

        private void ActionSelection()
        {
            _state = BattleState.ActionSelection;
            // Need for language manager
            //dialogBox.SetDialog("Choose an action");
            dialogBox.SetDialog(LanguageManager.Instance.GetString("BattleSystem_5"));
            dialogBox.EnableActionSelector(true);
        }

        private void OpenPartyScreen()
        {
            _state = BattleState.PartyScreen;
            partyScreen.SetPartyData(_playerParty.Pokemons);
            partyScreen.gameObject.SetActive(true);
        }

        private void MoveSelection()
        {
            _state = BattleState.MoveSelection;
            dialogBox.EnableActionSelector(false);
            dialogBox.EnableDialogText(false);
            dialogBox.EnableMoveSelector(true);
        }

        private IEnumerator AboutToUse(Monsters.Pokemon newPokemon)
        {
            _state = BattleState.Busy;
            //yield return dialogBox.TypeDialog(
            // Need for language manager
            //$"{_trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to change pokemon?");
            yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_6"), _trainer.Name, newPokemon.Base.Name));

            _state = BattleState.AboutToUse;
            dialogBox.EnableChoiceBox(true);
        }

        private IEnumerator RunTurns(BattleAction playerAction)
        {
            _state = BattleState.RunningTurn;
            switch (playerAction)
            {
                case BattleAction.Move:
                {
                    playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[_currentMove];
                    enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

                    var playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
                    var enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

                    // Check who goes first
                    var playerGoesFirst = true;
                    if (enemyMovePriority > playerMovePriority)
                        playerGoesFirst = false;
                    else if(enemyMovePriority == playerMovePriority)
                        playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

                    var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
                    var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;

                    var secondPokemon = secondUnit.Pokemon;

                    // First Turn
                    yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
                    yield return RunAfterTurn(firstUnit);
                    if (_state == BattleState.BattleOver) yield break;

                    if (secondPokemon.Hp > 0)
                    {
                        // Second Turn
                        yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                        yield return RunAfterTurn(secondUnit);
                    }
                    break;
                }
                case BattleAction.SwitchPokemon:
                    var selectedPokemon = _playerParty.Pokemons[_currentMember];
                    _state = BattleState.Busy;
                    yield return SwitchPokemon(selectedPokemon);

                    // Enemy Turn
                    var enemyMove = enemyUnit.Pokemon.GetRandomMove();
                    yield return RunMove(enemyUnit, playerUnit, enemyMove);
                    yield return RunAfterTurn(enemyUnit);
                    break;
                case BattleAction.UseItem:
                    dialogBox.EnableActionSelector(false);
                    yield return ThrowPokeball();
                    break;
                case BattleAction.Run:
                    yield return TryToEscape();
                    break;
            }
            if(_state != BattleState.BattleOver)
                ActionSelection();
        }

        private IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
        {
            var canRunMove = sourceUnit.Pokemon.OnBeforeMove();
            if (!canRunMove)
            {
                yield return ShowStatusChanges(sourceUnit.Pokemon);
                yield return sourceUnit.Hud.UpdateHp();
                yield break;
            }
            yield return ShowStatusChanges(sourceUnit.Pokemon);

            move.Pp--;
            // Need for language manager
            //yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");
            yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_7"), sourceUnit.Pokemon.Base.Name, move.Base.Name));


            if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
            {
                sourceUnit.PlayAttackAnimation();
                yield return new WaitForSeconds(1f);

                targetUnit.PlayHitAnimation();

                if (move.Base.Category == MoveCategory.Status)
                {
                    yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
                }
                else
                {
                    var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                    yield return targetUnit.Hud.UpdateHp();
                    yield return ShowDamageDetails(damageDetails);
                }

                if (move.Base.Secondaries is { Count: > 0 } && targetUnit.Pokemon.Hp > 0)
                {
                    foreach (var secondary in move.Base.Secondaries)
                    {
                        var rnd = UnityEngine.Random.Range(1, 101);
                        if (rnd <= secondary.Chance)
                            yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }

                if (targetUnit.Pokemon.Hp <= 0)
                {
                    // Need for language manager
                    //yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted");
                    yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_8"), targetUnit.Pokemon.Base.Name));
                    targetUnit.PlayFaintAnimation();

                    yield return new WaitForSeconds(2f);

                    CheckForBattleOver(targetUnit);
                }
            }
            else
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_9"), sourceUnit.Pokemon.Base.Name));
            }

            // Statuses like burn or psn will hurt the pokemon after the turn
            sourceUnit.Pokemon.OnAfterTurn();
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHp();

            if (sourceUnit.Pokemon.Hp <= 0)
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_8"), sourceUnit.Pokemon.Base.Name));
                sourceUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);

                CheckForBattleOver(sourceUnit);
            }
        }

        private IEnumerator RunMoveEffects(MoveEffects effects, Monsters.Pokemon source, Monsters.Pokemon target, MoveTarget moveTarget)
        {
            // Start Boosting
            if (effects.Boosts != null)
            {
                if (moveTarget == MoveTarget.Self)
                    source.ApplyBoosts(effects.Boosts);
                else
                    target.ApplyBoosts(effects.Boosts);
            }

            // Status Condition
            if (effects.Status != ConditionType.none)
            {
                target.SetStatus(effects.Status);
            }

            // Volatile Status Condition
            if (effects.VolatileStatus != ConditionType.none)
            {
                target.SetVolatileStatus(effects.VolatileStatus);
            }

            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
        }

        private IEnumerator RunAfterTurn(BattleUnit sourceUnit)
        {
            if (_state == BattleState.BattleOver)
                yield break;

            yield return new WaitUntil(() => _state == BattleState.RunningTurn);
            
            // Statuses like burn or psn will hurt the pokemon after the turn
            sourceUnit.Pokemon.OnAfterTurn();
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHp();

            if (sourceUnit.Pokemon.Hp <= 0)
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_8"), sourceUnit.Pokemon.Base.Name));
                sourceUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);

                CheckForBattleOver(sourceUnit);
                yield return new WaitUntil(() => _state == BattleState.RunningTurn);
            }
        }

        private bool CheckIfMoveHits(Move move, Monsters.Pokemon source, Monsters.Pokemon target)
        {
            if (move.Base.AlwaysHits)
                return true;

            float moveAccuracy = move.Base.Accuracy;

            var accuracy = source.StatBoosts[Stat.Accuracy];
            var evasion = target.StatBoosts[Stat.Evasion];

            var boostValues = new[] { 1f, 4f/3f, 5f/3f, 2f, 7f/3f, 8f/3f, 3f };

            if (accuracy > 0)
                moveAccuracy *= boostValues[accuracy];
            else
                moveAccuracy /= boostValues[-accuracy];

            if (evasion > 0)
                moveAccuracy /= boostValues[evasion];
            else
                moveAccuracy *= boostValues[-evasion];

            return UnityEngine.Random.Range(0, 101) <= moveAccuracy;
        }


        private IEnumerator ShowStatusChanges(Monsters.Pokemon pokemon)
        {
            while (pokemon.StatusChanges.Count > 0)
            {
                var message = pokemon.StatusChanges.Dequeue();
                yield return dialogBox.TypeDialog(message);
            }
        }

        private void CheckForBattleOver(BattleUnit faintedUnit)
        {
            if (faintedUnit.IsPlayerUnit)
            {
                var nextPokemon = _playerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                    OpenPartyScreen();
                else
                    BattleOver(false);
            }
            else
            {
                if(!_isTrainerBattle)
                    BattleOver(true);
                else
                {
                    var nextPokemon = _trainerParty.GetHealthyPokemon();
                    if (nextPokemon != null)
                        StartCoroutine(AboutToUse(nextPokemon));
                    else
                        BattleOver(true);
                }
            }
        }

        private IEnumerator ShowDamageDetails(DamageDetails damageDetails)
        {
            if (damageDetails.Critical > 1f)
                // Need for language manager
                //yield return dialogBox.TypeDialog("A critical hit!");
                yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_10"));

            if (damageDetails.TypeEffectiveness > 1f)
                // Need for language manager
                //yield return dialogBox.TypeDialog("It's super effective!");
                yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_11"));
            else if (damageDetails.TypeEffectiveness < 1f)
                // Need for language manager
                //yield return dialogBox.TypeDialog("It's not very effective!");
                yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_12"));
        }

        private void HandleActionSelection()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ++_currentAction;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --_currentAction;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                _currentAction += 2;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                _currentAction -= 2;

            _currentAction = Mathf.Clamp(_currentAction, 0, 3);

            dialogBox.UpdateActionSelection(_currentAction);

            if (Input.GetKeyDown(KeyCode.Z))
            {
                switch (CurrentAction)
                {
                    case BattleAction.Move:
                        MoveSelection();
                        break;
                    case BattleAction.Run:
                        StartCoroutine(RunTurns(BattleAction.Run));
                        break;
                    case BattleAction.UseItem:
                        StartCoroutine(RunTurns(BattleAction.UseItem));
                        break;
                    case BattleAction.SwitchPokemon:
                        _prevState = _state;
                        OpenPartyScreen();
                        break;
                }
            }
        }

        private void HandleMoveSelection()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ++_currentMove;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --_currentMove;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                _currentMove += 2;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                _currentMove -= 2;

            _currentMove = Mathf.Clamp(_currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

            dialogBox.UpdateMoveSelection(_currentMove, playerUnit.Pokemon.Moves[_currentMove]);

            if(Input.GetKeyDown(KeyCode.Z))
            {
                var move = playerUnit.Pokemon.Moves[_currentMove];
                if (move.Pp <= 0) return;

                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(RunTurns(BattleAction.Move));
            }
            else if(Input.GetKeyDown(KeyCode.X))

            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                ActionSelection();
            }
        }

        private void HandlePartyScreenSelection()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ++_currentMember;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --_currentMember;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                _currentMember += 2;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                _currentMember -= 2;

            _currentMember = Mathf.Clamp(_currentMember, 0, _playerParty.Pokemons.Count - 1);

            partyScreen.UpdateMemberSelection(_currentMember);

            if (Input.GetKeyDown(KeyCode.Z))
            {
                var selectedMember = _playerParty.Pokemons[_currentMember];
                if (selectedMember.Hp <= 0)
                {
                    // Need for language manager
                    //partyScreen.SetMessageText("You can't send out a fainted pokemon");
                    partyScreen.SetMessageText(LanguageManager.Instance.GetString("BattleSystem_13"));
                    return;
                }
                if (selectedMember == playerUnit.Pokemon)
                {
                    // Need for language manager
                    //partyScreen.SetMessageText("You can't switch with the same pokemon");
                    partyScreen.SetMessageText(LanguageManager.Instance.GetString("BattleSystem_14"));
                    return;
                }

                partyScreen.gameObject.SetActive(false);

                if (_prevState == BattleState.ActionSelection)
                {
                    _prevState = null;
                    StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
                }
                else
                {
                    _state = BattleState.Busy;
                    StartCoroutine(SwitchPokemon(selectedMember));
                }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (playerUnit.Pokemon.Hp <= 0)
                {
                    // Need for language manager
                    //partyScreen.SetMessageText("You have to choose a pokemon to continue");
                    partyScreen.SetMessageText(LanguageManager.Instance.GetString("BattleSystem_15"));
                    return;
                }

                partyScreen.gameObject.SetActive(false);

                if (_prevState == BattleState.AboutToUse)
                {
                    _prevState = null;
                    StartCoroutine(SendNextTrainerPokemon());
                }
                else
                    ActionSelection();
            }
        }

        private void HandleAboutToUse()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                aboutToUseChoice = !aboutToUseChoice;

            dialogBox.UpdateChoiceBox(aboutToUseChoice);

            if(Input.GetKeyDown(KeyCode.Z))
            {
                dialogBox.EnableChoiceBox(false);
                if (aboutToUseChoice)
                {
                    // Yes Option
                    _prevState = BattleState.AboutToUse;
                    OpenPartyScreen();
                }
                else
                {
                    // No Option
                    StartCoroutine(SendNextTrainerPokemon());
                }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                dialogBox.EnableChoiceBox(false);
                StartCoroutine(SendNextTrainerPokemon());
            }
        }

        private IEnumerator SendOutPokemon(Monsters.Pokemon nextPokemon)
        {
            _currentMove = 0;
            playerUnit.Setup(nextPokemon);
            dialogBox.SetMoveNames(nextPokemon.Moves);
            // Need for language manager
            //yield return dialogBox.TypeDialog($"Go {nextPokemon.Base.Name}!");
            yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_16"), nextPokemon.Base.Name));
        }

        private IEnumerator SwitchPokemon(Monsters.Pokemon newPokemon)
        {
            if (playerUnit.Pokemon.Hp > 0)
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_17"), playerUnit.Pokemon.Base.Name));
                playerUnit.PlayReturnAnimation();
                yield return new WaitForSeconds(2f);
            }

            yield return SendOutPokemon(newPokemon);

            switch (_prevState)
            {
                case null:
                    _state = BattleState.RunningTurn;
                    break;
                case BattleState.AboutToUse:
                    _prevState = null;
                    StartCoroutine(SendNextTrainerPokemon());
                    break;
            }
        }

        private IEnumerator SendNextTrainerPokemon()
        {
            _state = BattleState.Busy;

            var nextPokemon = _trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(nextPokemon);
            // Need for language manager
            //yield return dialogBox.TypeDialog($"{_trainer.Name} send out {nextPokemon.Base.Name}!");
            yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_18"), _trainer.Name, nextPokemon.Base.Name));

            _state = BattleState.RunningTurn;
        }

        private IEnumerator ThrowPokeball()
        {
            _state = BattleState.Busy;

            if (_isTrainerBattle)
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog("You can't steal the trainers pokemon!");
                yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_19"));
                _state = BattleState.RunningTurn;
                yield break;
            }

            // Need for language manager
            //yield return dialogBox.TypeDialog($"{_player.Name} used POKEBALL!");
            yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_20"), _player.Name));

            var pokeballObj = Instantiate(pokeballPrefab, playerUnit.transform.position - new Vector3(0, 2), Quaternion.identity);
            var pokeball  = pokeballObj.GetComponent<Pokeball>();

            // Animations
            yield return pokeball.PlayThrowAnimation(enemyUnit.transform.position);
            yield return enemyUnit.PlayCaptureAnimation();
            yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

            var shakeCount = pokeball.TryToCatchPokemon(enemyUnit.Pokemon);

            yield return pokeball.PlayShakeAnimation(shakeCount);

            if (shakeCount == 4)
            {
                // Pokemon is caught
                // Need for language manager
                //yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_21"), enemyUnit.Pokemon.Base.Name));
                yield return pokeball.PlayFadeOutAnimation();

                _playerParty.AddPokemon(enemyUnit.Pokemon);
                // Need for language manager
                //yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party");
                yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_22"), enemyUnit.Pokemon.Base.Name));

                Destroy(pokeball.gameObject);
                BattleOver(true);
            }
            else
            {
                // Pokemon broke out
                yield return new WaitForSeconds(1f);
                yield return pokeball.PlayFadeOutAnimation();
                yield return enemyUnit.PlayBreakOutAnimation();

                if (shakeCount < 2)
                    // Need for language manager
                    //yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free");
                    yield return dialogBox.TypeDialog(string.Format(LanguageManager.Instance.GetString("BattleSystem_23"), enemyUnit.Pokemon.Base.Name));
                else
                    // Need for language manager
                    //yield return dialogBox.TypeDialog("Almost caught it");
                    yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_24"));

                Destroy(pokeball.gameObject);
                _state = BattleState.RunningTurn;
            }
        }

        private IEnumerator TryToEscape()
        {
            _state = BattleState.Busy;

            if(_isTrainerBattle)
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog("You can't run from trainer battles!");
                yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_25"));
                _state = BattleState.RunningTurn;
                yield break;
            }

            // https://bulbapedia.bulbagarden.net/wiki/Escape
            var playerSpeed = playerUnit.Pokemon.Speed;
            var enemySpeed = enemyUnit.Pokemon.Speed;
            _escapeAttemps++;

            if (enemySpeed < playerSpeed)
            {
                // Need for language manager
                //yield return dialogBox.TypeDialog("Ran away safely!");
                yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_26"));
                BattleOver(true);
            }
            else
            {
                var f = ((playerSpeed * 128) / enemySpeed + 30 * _escapeAttemps) % 256;
                if(Random.Range(0, 256) < f)
                {
                    // Need for language manager
                    //yield return dialogBox.TypeDialog("Ran away safely!");
                    yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_26"));
                    BattleOver(true);
                }
                else
                {
                    // Need for language manager
                    //yield return dialogBox.TypeDialog("Can't escape!");
                    yield return dialogBox.TypeDialog(LanguageManager.Instance.GetString("BattleSystem_27"));
                    _state = BattleState.RunningTurn;
                }
            }
        }
    }
}
