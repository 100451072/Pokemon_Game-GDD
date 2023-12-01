using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pokemon.Monsters
{
    public class PokemonParty : MonoBehaviour
    {
        [SerializeField] private List<Pokemon> pokemons;

        public List<Pokemon> Pokemons => pokemons;

        private void Start()
        {
            foreach (var pokemon in pokemons)
            {
                pokemon.Init();
            }
        }

        public Pokemon GetHealthyPokemon()
        {
            return pokemons.FirstOrDefault(pokemon => pokemon.Hp > 0);
        }

        public void AddPokemon(Pokemon newPokemon)
        {
            if(pokemons.Count < 6)
                pokemons.Add(newPokemon);
            else
            {
                // ToDO: Add to the PC once that's implemented
            }
        }
    }
}
