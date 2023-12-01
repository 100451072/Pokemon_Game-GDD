using System.Collections.Generic;
using Pokemon.Types;
using UnityEngine;

namespace Pokemon.Moves
{
    [CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
    public class MoveBase : ScriptableObject
    {
        [Header("Description")]
        [SerializeField] private string name;

        [TextArea]
        [SerializeField] private string description;

        [Header("Move Base Stats")]
        [SerializeField] private PokemonType type;
        [SerializeField] private int power;

        [Range(0, 100)]
        [SerializeField] private int accuracy = 100;

        [SerializeField] private bool alwaysHits;
        
        [Tooltip("PP is the number of times a move can be performed.")]
        [SerializeField] private int pp;

        [SerializeField] private int priority;

        [SerializeField] private MoveCategory category;
        [SerializeField] private MoveEffects effects;
        [SerializeField] private List<SecondaryEffects> secondaries;
        [SerializeField] private MoveTarget target;

        public string Name => name;
        public string Description => description;
        public PokemonType Type => type;
        public int Power => power;
        public int Accuracy => accuracy;
        public bool AlwaysHits => alwaysHits;
        public int Pp => pp;
        public int Priority => priority;
        public MoveCategory Category => category;
        public MoveEffects Effects => effects;
        public List<SecondaryEffects> Secondaries => secondaries;
        public MoveTarget Target => target;
    }
}