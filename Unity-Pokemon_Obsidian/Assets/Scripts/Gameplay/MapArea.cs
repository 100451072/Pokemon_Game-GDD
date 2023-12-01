using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Gameplay
{
    public class MapArea : MonoBehaviour
    {
        [SerializeField] private List<Monsters.Pokemon> wildPokemons;

        public Monsters.Pokemon GetRandomWildPokemon()
        {
            var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
            wildPokemon.Init();
            return wildPokemon;
        }
    }
}
