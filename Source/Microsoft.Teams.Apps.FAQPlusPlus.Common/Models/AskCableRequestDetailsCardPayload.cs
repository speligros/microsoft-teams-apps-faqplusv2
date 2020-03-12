namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    public class AskCableRequestDetailsCardPayload : TeamsAdaptiveSubmitActionData
    {
        public string Delegation { get; set; }

        public string Office { get; set; }

        public string Floor { get; set; }

        public string NetPoint { get; set; }

    }
}