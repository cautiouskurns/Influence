using System.Collections.Generic;

namespace Scenarios
{
    [System.Serializable]
    public class NationStartCondition
    {
        public string nationId;
        public string nationName;
        public List<string> controlledRegionIds = new List<string>();
    }
}