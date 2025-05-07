using UnityEngine;

namespace UI
{
    /// <summary>
    /// ScriptableObject for storing test sprites that can be accessed at runtime
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteTestAsset", menuName = "Influence/Sprite Test Asset")]
    public class SpriteTestAsset : ScriptableObject
    {
        [Tooltip("Collection of sprites for testing")]
        public Sprite[] TestSprites;
    }
}