namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.Luis;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Luis service provider interface.
    /// </summary>
    public interface ILuisServiceProvider
    {
        /// <summary>
        /// .
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) 
            where T : IRecognizerConvert, new();

        /// <summary>
        /// .
        /// </summary>
        /// <returns></returns>
        bool IsConfigured();
    }
}
