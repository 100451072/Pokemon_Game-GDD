using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon.Moves;
using Pokemon.Stats;
using Pokemon.Types;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
// Import language Manager
using GEAR.Localization;

namespace Pokemon.Monsters
{
    [Serializable]
    public class Pokemon
    {
        [SerializeField] private PokemonBase _base;
        [SerializeField] private int _level;

        public PokemonBase Base => _base;
        public int Level => _level;

        public int Hp { get; set; }
        public List<Move> Moves { get; private set; }
        public Move CurrentMove { get; set; }
        public Dictionary<Stat, int> Stats { get; private set; }
        public Dictionary<Stat, int> StatBoosts { get; private set; }
        public Condition Status { get; private set; }
        public int StatusTime { get; set; }
        public Condition VolatileStatus { get; private set; }
        public int VolatileStatusTime { get; set; }
        public Queue<string> StatusChanges { get; private set; }
        public bool HpChanged { get; set; }

        public UnityEvent OnStatusChanged;

        public Pokemon(PokemonBase pBase, int pLevel)
        {
            _base = pBase;
            _level = pLevel;

            Init();
        }

        public void Init()
        {
            Moves = new List<Move>();
            StatusChanges = new Queue<string>();

            OnStatusChanged = new UnityEvent();

            // Generate Moves
            foreach (var move in Base.LearnableMoves)
            {
                if (move.Level <= Level)
                    Moves.Add(new Move(move.Base));
                if (Moves.Count >= 4)
                    break;
            }

            CalculateStats();
            ResetStatBoost();

            Hp = MaxHp;
            Status = VolatileStatus = null;
        }

        private void CalculateStats()
        {
            Stats = new Dictionary<Stat, int>
            {
                { Stat.Attack, Mathf.FloorToInt(Base.Attack * Level / 100f) + 5 },
                { Stat.Defense, Mathf.FloorToInt(Base.Defense * Level / 100f) + 5 },
                { Stat.SpAttack, Mathf.FloorToInt(Base.SpAttack * Level / 100f) + 5 },
                { Stat.SpDefense, Mathf.FloorToInt(Base.SpDefense * Level / 100f) + 5 },
                { Stat.Speed, Mathf.FloorToInt(Base.Speed * Level / 100f) + 5 }
            };

            MaxHp = Mathf.FloorToInt(Base.MaxHp * Level / 100f) + 10 + Level;
        }

        private void ResetStatBoost()
        {
            StatBoosts = new Dictionary<Stat, int>()
            {
                { Stat.Attack, 0 },
                { Stat.Defense, 0 },
                { Stat.SpAttack, 0 },
                { Stat.SpDefense, 0 },
                { Stat.Speed, 0 },
                { Stat.Accuracy, 0 },
                { Stat.Evasion, 0 }
            };
        }

        private int GetStat(Stat stat)
        {
            var statVal = Stats[stat];

            // Apply stat boost
            var boost = StatBoosts[stat];
            var boostValues = new[] { 1f, 1.5f, 2f, 2.5f, 3f, 3, 5f, 4 };

            statVal = boost >= 0 ? Mathf.FloorToInt(statVal * boostValues[boost]) : 
                Mathf.FloorToInt(statVal / boostValues[-boost]);

            return statVal;
        }

        public void ApplyBoosts(List<StatBoost> statBoosts)
        {
            foreach (var statBoost in statBoosts)
            {
                var stat = statBoost.stat;
                var boost = statBoost.boost;

                StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

                if(boost > 0)
                    //StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
                    StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("Pokemon_1"), Base.Name, stat));
                else if(boost < 0)
                    //StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
                    StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("Pokemon_1"), Base.Name, stat));

                Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
            }
        }

        public DamageDetails TakeDamage(Move move, Pokemon attacker)
        {
            // https://bulbapedia.bulbagarden.net/wiki/Damage

            var critical = 1f;
            if (Random.value <= 0.0625f)
                critical = 2f;

            var type1 = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1);
            var type2 = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

            var damageDetails = new DamageDetails()
            {
                TypeEffectiveness = type1 * type2,
                Critical = critical,
                Fainted = false
            };

            float attack = move.Base.Category == MoveCategory.Special ? attacker.SpAttack : attacker.Attack;
            float defense = move.Base.Category == MoveCategory.Special ? SpDefense : Defense;

            var modifiers = Random.Range(0.85f, 1f) * type1 * type2 * critical;
            var a = (2 * attacker.Level + 10) / 250f;
            var d = a * move.Base.Power * (attack / defense) + 2;
            var damage = Mathf.FloorToInt(d * modifiers);

            UpdateHp(damage);

            return damageDetails;
        }

        public void UpdateHp(int damage)
        {
            Hp = Mathf.Clamp(Hp - damage, 0, MaxHp);
            HpChanged = true;
        }

        public void SetStatus(ConditionType conditionId)
        {
            if (Status != null) return;

            Status = ConditionDB.Conditions[conditionId];
            Status?.OnStart?.Invoke(this);
            StatusChanges.Enqueue($"{Base.Name} {LanguageManager.Instance.GetString(Status?.StartMessage)}");
            OnStatusChanged?.Invoke();
        }

        public void CureStatus()
        {
            Status = null;
            OnStatusChanged?.Invoke();
        }

        public void SetVolatileStatus(ConditionType conditionId)
        {
            if (VolatileStatus != null) return;

            VolatileStatus = ConditionDB.Conditions[conditionId];
            VolatileStatus?.OnStart?.Invoke(this);
            StatusChanges.Enqueue($"{Base.Name} {LanguageManager.Instance.GetString(VolatileStatus?.StartMessage)}");
        }

        public void CureVolatileStatus()
        {
            VolatileStatus = null;
        }

        public Move GetRandomMove()
        {
            var moveWithPp = Moves.Where(move => move.Pp > 0).ToList();
            var randomIndex = Random.Range(0, moveWithPp.Count);
            return Moves[randomIndex];
        }

        public bool OnBeforeMove()
        {
            var canPerformMove = true;

            if (Status?.OnBeforeMove != null)
            {
                if(!Status.OnBeforeMove(this))
                    canPerformMove = false;
            }

            if (VolatileStatus?.OnBeforeMove != null)
            {
                if (!VolatileStatus.OnBeforeMove(this))
                    canPerformMove = false;
            }

            return canPerformMove;
        }

        public void OnAfterTurn()
        {
            Status?.OnAfterTurn?.Invoke(this);
            VolatileStatus?.OnAfterTurn?.Invoke(this);
        }

        public void OnBattleOver()
        {
            VolatileStatus = null;
            ResetStatBoost();
        }


        // https://bulbapedia.bulbagarden.net/wiki/Stat
        public int MaxHp { get; private set; }
        public int Attack => GetStat(Stat.Attack);
        public int Defense => GetStat(Stat.Defense);
        public int SpAttack => GetStat(Stat.SpAttack);
        public int SpDefense => GetStat(Stat.SpDefense);
        public int Speed => GetStat(Stat.Speed);
    }
}
