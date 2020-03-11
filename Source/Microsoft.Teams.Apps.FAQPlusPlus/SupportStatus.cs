using Microsoft.Bot.Schema;

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    /// <summary>
    /// This is our application state. Just a regular serializable .NET class.
    /// </summary>
    public class SupportStatus
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public bool IsAnswered { get; set; }

        public bool IsUseful { get; set; }

        public bool IsMoreHelpRequired { get; set; }

        public bool IsTicketRequired { get; set; }

        public bool IsExpertRequired { get; set; }
    }
}