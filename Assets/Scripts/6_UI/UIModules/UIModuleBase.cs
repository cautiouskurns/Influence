using UnityEngine;

namespace UI
{
    /// <summary>
    /// Preferred position for UI modules on screen
    /// </summary>
    public enum UIPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        Center
    }
    
    /// <summary>
    /// Base class for all UI modules
    /// </summary>
    public abstract class UIModuleBase : MonoBehaviour
    {
        [Header("Module Settings")]
        [SerializeField] protected bool isVisible = true;
        
        /// <summary>
        /// Initialize the UI module
        /// </summary>
        public virtual void Initialize()
        {
            // Base initialization
            gameObject.SetActive(isVisible);
        }
        
        /// <summary>
        /// Show the UI module
        /// </summary>
        public virtual void Show()
        {
            isVisible = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Hide the UI module
        /// </summary>
        public virtual void Hide()
        {
            isVisible = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Toggle visibility of the UI module
        /// </summary>
        public virtual void ToggleVisibility()
        {
            isVisible = !isVisible;
            gameObject.SetActive(isVisible);
        }
    }
}