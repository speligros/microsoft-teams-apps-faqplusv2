// <copyright file="QnaServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;

    /// <summary>
    /// Qna maker service provider class.
    /// </summary>
    public class LuisServiceProvider : ILuisServiceProvider
    {
        /// <summary>
        /// Environment type.
        /// </summary>
        private const string EnvironmentType = "Prod";

        private readonly IConfigurationDataProvider configurationProvider;
        private readonly LuisSettings options;
        // TODO add LuisRecognizer client

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisServiceProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">ConfigurationProvider fetch and store information in storage table.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        public LuisServiceProvider(IConfigurationDataProvider configurationProvider, IOptionsMonitor<LuisSettings> optionsAccessor)
        {
            this.configurationProvider = configurationProvider;
            this.options = optionsAccessor.CurrentValue;
            // TODO LuisRecognizer
        }

        public string GetAppId()
        {
            return this.options.AppId;
        }

        public string GetAPIHostName()
        {
            return this.options.APIHostName;
        }

        public string GetAPIKey()
        {
            return this.options.APIKey;
        }
    }
}
