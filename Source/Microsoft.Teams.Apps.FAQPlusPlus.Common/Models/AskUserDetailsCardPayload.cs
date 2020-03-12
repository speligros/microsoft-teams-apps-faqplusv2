namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    public class AskUserDetailsCardPayload : TeamsAdaptiveSubmitActionData
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Department { get; set; }

        public string Responsible { get; set; }

        public string CreateDate { get; set; }
    }
}