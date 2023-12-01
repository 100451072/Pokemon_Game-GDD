using System;
using System.Collections.Generic;
using Pokemon.Stats;
using Pokemon.Types;
using UnityEngine;

namespace Pokemon.Moves
{
    [Serializable]
    public class MoveEffects
    {
        [SerializeField] private List<StatBoost> boosts;
        [SerializeField] private ConditionType status;
        [SerializeField] private ConditionType volatileStatus;

        public List<StatBoost> Boosts => boosts;
        public ConditionType Status => status;
        public ConditionType VolatileStatus => volatileStatus;
    }

    [Serializable]
    public class SecondaryEffects : MoveEffects
    {
        [SerializeField] private int chance;
        [SerializeField] private MoveTarget target;

        public int Chance => chance;
        public MoveTarget Target => target;
    }
}
