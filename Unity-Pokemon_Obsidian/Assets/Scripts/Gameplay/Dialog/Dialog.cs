using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Gameplay.Dialog
{
    [Serializable]
    public class Dialog
    {
        [SerializeField] private List<string> lines;

        public List<string> Lines => lines;
    }
}
