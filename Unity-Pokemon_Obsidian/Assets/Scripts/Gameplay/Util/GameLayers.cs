using UnityEngine;

namespace Pokemon.Gameplay.Util
{
    public class GameLayers : MonoBehaviour
    {
        [Header("Layer Mask Settings")]
        [SerializeField] private LayerMask solidObjectsLayer;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask longGrassLayer;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask fovLayer;

        public LayerMask SolidObjectsLayer => solidObjectsLayer;
        public LayerMask InteractableLayer => interactableLayer;
        public LayerMask LongGrassLayer => longGrassLayer;
        public LayerMask PlayerLayer => playerLayer;
        public LayerMask FovLayer => fovLayer;

        public static GameLayers Instance { get; private set; }

        private void Awake()
        {
            if(Instance == null)
                Instance = this;
            else
                Destroy(this);
        }
    }
}
