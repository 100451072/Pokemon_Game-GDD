using System.Collections.Generic;
using Pokemon.Moves;
using Pokemon.Types;
using UnityEngine;

namespace Pokemon.Monsters
{
    [CreateAssetMenu(fileName ="Pokemon", menuName ="Pokemon/Create new pokemon")]
    public class PokemonBase : ScriptableObject
    {
        [Header("Description")]
        [SerializeField] private string _name;
        
        [TextArea]
        [SerializeField] private string _description;
        
        [Header("Sprites")]
        [SerializeField] private Sprite _frontSprite;
        [SerializeField] private Sprite _backSprite;

        [Header("Types")]
        [SerializeField] private PokemonType _type1;
        [SerializeField] private PokemonType _type2;

        [Header("Base Stats")]
        // https://bulbapedia.bulbagarden.net/wiki/List_of_Pok%C3%A9mon_by_base_stats_(Generation_VII)

        [SerializeField] private int _maxHp;
        [SerializeField] private int _attack;
        [SerializeField] private int _defense;
        [SerializeField] private int _spAttack;
        [SerializeField] private int _spDefense;
        [SerializeField] private int _speed;

        // https://bulbapedia.bulbagarden.net/wiki/List_of_Pok%C3%A9mon_by_catch_rate
        [SerializeField] private int _catchRate;

        [Header("Moves")]
        [SerializeField] List<LearnableMove> _learnableMoves;


        public string Name => _name;
        public string Description => _description;
        public Sprite FrontSprite => _frontSprite;
        public Sprite BackSprite => _backSprite;
        public PokemonType Type1 => _type1;
        public PokemonType Type2 => _type2;
        public int MaxHp => _maxHp;
        public int Attack => _attack;
        public int Defense => _defense;
        public int SpAttack => _spAttack;
        public int SpDefense => _spDefense;
        public int Speed => _speed;
        public int CatchRate => _catchRate;
        public List<LearnableMove> LearnableMoves => _learnableMoves;
    }
}
