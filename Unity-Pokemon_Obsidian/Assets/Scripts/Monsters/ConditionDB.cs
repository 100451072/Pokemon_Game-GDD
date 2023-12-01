using Pokemon.Types;
using System.Collections.Generic;
using UnityEngine;
//Import Language Manger
using GEAR.Localization;

namespace Pokemon.Monsters
{
    public class ConditionDB
    {
        public static void Init()
        {
            foreach (var (conditionType, condition) in Conditions)
            {
                condition.Type = conditionType;
            }
        }

        public static Dictionary<ConditionType, Condition> Conditions { get; set; } =
            new Dictionary<ConditionType, Condition>
            {
                {
                    ConditionType.psn,
                    new Condition()
                    {
                        //Name = "Poison",
                        Name = "ConditionDB_1",
                        //StartMessage = "has been poisoned",
                        StartMessage = "ConditionDB_2",
                        OnAfterTurn = pokemon =>
                        {
                            pokemon.UpdateHp(pokemon.MaxHp / 8);
                            //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison");
                            pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_3"), pokemon.Base.Name));
                        }
                    }
                },

                {
                    ConditionType.brn,
                    new Condition()
                    {
                        //Name = "Burn",
                        Name = "ConditionDB_4",
                        //StartMessage = "has been burned",
                        StartMessage = "ConditionDB_5",
                        OnAfterTurn = pokemon =>
                        {
                            pokemon.UpdateHp(pokemon.MaxHp / 16);
                            //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to burn");
                            pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_6"), pokemon.Base.Name));
                        }
                    }
                },

                {
                    ConditionType.par,
                    new Condition()
                    {
                        //Name = "Paralyzed",
                        Name = "ConditionDB_7",
                        //StartMessage = "has been paralyzed",
                        StartMessage = "ConditionDB_8",
                        OnBeforeMove = (pokemon) =>
                        {
                            // returns true if the pokemon will be able to perform the move
                            if (UnityEngine.Random.Range(1, 5) != 1) 
                                return true;
                            
                            //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed and can't move");
                            pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_9"), pokemon.Base.Name));
                            return false;
                        }
                        
                    }
                },

                {
                    ConditionType.frz,
                    new Condition()
                    {
                        //Name = "Freeze",
                        Name = "ConditionDB_10",
                        //StartMessage = "has been frozen",
                        StartMessage = "ConditionDB_11",
                        OnBeforeMove = (pokemon) =>
                        {
                            // returns true if the pokemon will be able to perform the move
                            if (UnityEngine.Random.Range(1, 5) != 1) 
                                return false;
                            
                            pokemon.CureStatus();
                            //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore");
                            pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_12"), pokemon.Base.Name));
                            return true;
                        }
                    }
                },

                {
                    ConditionType.slp,
                    new Condition()
                    {
                        //Name = "Sleep",
                        Name = "ConditionDB_13",
                        //StartMessage = "has fallen asleep",
                        StartMessage = "ConditionDB_14",
                        OnStart = (pokemon) =>
                        {
                            // Sleep for 1-3 turns
                            pokemon.StatusTime = UnityEngine.Random.Range(1, 4);
                            Debug.Log($"Will be asleep for {pokemon.StatusTime} moves");
                            Debug.Log($"Will be asleep for {pokemon.StatusTime} moves");
                        },
                        OnBeforeMove = (pokemon) =>
                        {
                            // returns true if the pokemon will be able to perform the move
                            if (pokemon.StatusTime <= 0)
                            {
                                pokemon.CureStatus();
                                //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                                pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_15"), pokemon.Base.Name));
                                return true;
                            }

                            pokemon.StatusTime--;
                            //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping");
                            pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_16"), pokemon.Base.Name));
                            return false;
                        }
                    }
                },

                // Volatile Status Conditions
                {
                    ConditionType.confusion,
                    new Condition()
                    {
                        //Name = "Confusion",
                        Name = "ConditionDB_17",
                        //StartMessage = "has been confused",
                        StartMessage = "ConditionDB_18",
                        OnStart = (pokemon) =>
                        {
                            // Confused for 1-4 turns
                            pokemon.VolatileStatusTime = UnityEngine.Random.Range(1, 5);
                            Debug.Log($"Will be confused for {pokemon.StatusTime} moves");
                        },
                        OnBeforeMove = (pokemon) =>
                        {
                            // returns true if the pokemon will be able to perform the move
                            if (pokemon.VolatileStatusTime <= 0)
                            {
                                pokemon.CureVolatileStatus();
                                //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                                pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_19"), pokemon.Base.Name));
                                return true;
                            }

                            pokemon.VolatileStatusTime--;

                            // 50% chance to do a move
                            if (UnityEngine.Random.Range(1, 3) == 1)
                                return true;

                            // Hurt by confusion
                            //pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused");
                            pokemon.StatusChanges.Enqueue(string.Format(LanguageManager.Instance.GetString("ConditionDB_20"), pokemon.Base.Name));
                            pokemon.UpdateHp(pokemon.MaxHp / 8);
                            //pokemon.StatusChanges.Enqueue("It hurt itself due to confusion");
                            pokemon.StatusChanges.Enqueue(LanguageManager.Instance.GetString("ConditionDB_21"));
                            return false;
                        }
                    }
                }
            };

        public static float GetStatusBonus(Condition condition)
        {
            if (condition == null)
                return 1f;

            switch (condition.Type)
            {
                case ConditionType.slp:
                case ConditionType.frz:
                    return 2f;
                case ConditionType.par:
                case ConditionType.psn:
                case ConditionType.brn:
                    return 1.5f;
                default:
                    return 1;
            }
        }
    }
}
