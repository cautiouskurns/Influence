using UnityEngine;
using System.Collections;

namespace PlayModeTests
{
    /// <summary>
    /// Test component to represent region economic data in play mode tests.
    /// </summary>
    public class RegionDataComponent : MonoBehaviour
    {
        [SerializeField] private int wealth = 0;
        [SerializeField] private int production = 0;

        /// <summary>
        /// The current wealth value of the region
        /// </summary>
        public int Wealth => wealth;

        /// <summary>
        /// The current production value of the region
        /// </summary>
        public int Production => production;
    }
}