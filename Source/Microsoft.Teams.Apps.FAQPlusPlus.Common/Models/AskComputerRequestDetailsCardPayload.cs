namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    public class AskComputerRequestDetailsCardPayload : TeamsAdaptiveSubmitActionData
    {
        public string User { get; set; }

        public string Delegation { get; set; }

        public string Responsible { get; set; }

        public string Ram { get; set; }

        public string Disk { get; set; }

        public string DurationInMonths { get; set; }


    }
}