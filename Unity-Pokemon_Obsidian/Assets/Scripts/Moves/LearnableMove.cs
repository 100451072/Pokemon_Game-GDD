using System;
using UnityEngine;

namespace Pokemon.Moves
{
    [Serializable]
    public class LearnableMove
    {
        [SerializeField] private MoveBase _base;
        [SerializeField] private int _level;

        public MoveBase Base => _base;
        public int Level => _level;
    }
}