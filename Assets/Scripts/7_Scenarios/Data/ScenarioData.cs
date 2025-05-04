using System;

namespace Scenarios
{
    [Serializable]
    public class ScenarioData
    {
        public string id;
        public string title;
        public string description;
        public string objectiveText;
        
        public ScenarioData(string id, string title, string description, string objectiveText)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.objectiveText = objectiveText;
        }
    }
}