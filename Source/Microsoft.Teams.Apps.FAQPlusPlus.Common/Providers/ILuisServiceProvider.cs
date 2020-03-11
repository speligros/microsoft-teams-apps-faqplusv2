namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Luis service provider interface.
    /// </summary>
    public interface ILuisServiceProvider
    {
        /// <summary>
        /// Sample
        /// </summary>
        /// <returns>Config value</returns>
        string GetAppId();

        /// <summary>
        /// Sample
        /// </summary>
        /// <returns>Config value</returns>
        string GetAPIHostName();

        /// <summary>
        /// Sample
        /// </summary>
        /// <returns>Config value</returns>
        string GetAPIKey();
    }
}
