using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Scenarios
{
    public class ScenarioItemUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image background;
        
        private ScenarioData scenarioData;
        private System.Action<ScenarioData> onClickCallback;
        
        public void Initialize(ScenarioData data, System.Action<ScenarioData> callback)
        {
            scenarioData = data;
            onClickCallback = callback;
            
            if (titleText != null)
                titleText.text = data.title;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onClickCallback?.Invoke(scenarioData);
        }
    }
}