using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string tooltipText;
        public float tooltipDelay = 0.5f;
        
        private GameObject tooltipObject;
        private float hoverStartTime;
        private bool isHovering = false;
        private static TooltipHandler currentlyHovered = null;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Ensure any existing tooltips from other handlers are hidden
            if (currentlyHovered != null && currentlyHovered != this)
            {
                currentlyHovered.ForceHideTooltip();
            }
            
            currentlyHovered = this;
            hoverStartTime = Time.time;
            isHovering = true;
            CancelInvoke("ShowTooltip");
            Invoke("ShowTooltip", tooltipDelay);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentlyHovered == this)
            {
                currentlyHovered = null;
            }
            
            isHovering = false;
            CancelInvoke("ShowTooltip");
            HideTooltip();
        }
        
        private void ShowTooltip()
        {
            if (!isHovering) return;
            
            if (tooltipObject == null)
            {
                tooltipObject = CreateTooltip();
            }
            
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(true);
                UpdateTooltipPosition();
            }
        }
        
        private void HideTooltip()
        {
            if (tooltipObject != null && isActiveAndEnabled)
            {
                tooltipObject.SetActive(false);
            }
        }
        
        // This version doesn't check if the component is enabled
        private void ForceHideTooltip()
        {
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(false);
            }
        }
        
        private GameObject CreateTooltip()
        {
            // Find a valid parent canvas for the tooltip
            Canvas canvas = FindValidCanvas();
            if (canvas == null)
            {
                Debug.LogWarning("TooltipHandler: No canvas found to attach tooltip to");
                return null;
            }
            
            GameObject tooltip = new GameObject("Tooltip", typeof(RectTransform));
            tooltip.transform.SetParent(canvas.transform, false);
            
            // Make sure tooltip doesn't block raycasts
            CanvasGroup canvasGroup = tooltip.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            
            RectTransform rect = tooltip.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 100);
            
            // Add background
            Image bg = tooltip.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            // Add text
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(tooltip.transform, false);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = tooltipText;
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            
            // Set tooltip inactive initially
            tooltip.SetActive(false);
            
            return tooltip;
        }
        
        private Canvas FindValidCanvas()
        {
            // Try to find a canvas in the current hierarchy first
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                return parentCanvas;
            }
            
            // If not found in parent hierarchy, try other methods
            
            // Find the canvas this UI element is part of
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            // First try to find the topmost enabled canvas in ScreenSpace-Overlay mode
            foreach (Canvas c in canvases)
            {
                if (c.enabled && c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return c;
                }
            }
            
            // If none found, try to use any active canvas
            foreach (Canvas c in canvases)
            {
                if (c.enabled)
                {
                    return c;
                }
            }
            
            return null; // Don't create a new canvas as it might be causing issues
        }
        
        private void UpdateTooltipPosition()
        {
            if (tooltipObject == null || !tooltipObject.activeSelf) return;
            
            // Update tooltip position to follow mouse
            Vector3 mousePos = Input.mousePosition;
            
            // Ensure tooltip stays within screen bounds
            RectTransform rect = tooltipObject.GetComponent<RectTransform>();
            Vector2 tooltipSize = rect.sizeDelta;
            
            // Add offset, keeping tooltip on screen
            float xOffset = 20;
            float yOffset = -20;
            
            // Check right edge
            if (mousePos.x + xOffset + tooltipSize.x > Screen.width)
            {
                xOffset = -tooltipSize.x - 20; // Place to the left of cursor
            }
            
            // Check bottom edge
            if (mousePos.y + yOffset - tooltipSize.y < 0)
            {
                yOffset = tooltipSize.y + 20; // Place above cursor
            }
            
            rect.position = mousePos + new Vector3(xOffset, yOffset, 0);
        }
        
        private void Update()
        {
            // Only update position if this is the currently hovered element and tooltip is active
            if (isHovering && currentlyHovered == this && tooltipObject != null && tooltipObject.activeSelf)
            {
                UpdateTooltipPosition();
            }
        }
        
        private void OnDisable()
        {
            // Hide tooltip if this component is disabled
            if (currentlyHovered == this)
            {
                currentlyHovered = null;
                ForceHideTooltip();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up tooltip when this component is destroyed
            if (tooltipObject != null)
            {
                Destroy(tooltipObject);
            }
            
            // Clear current handler if it's this one
            if (currentlyHovered == this)
            {
                currentlyHovered = null;
            }
        }
    }
}