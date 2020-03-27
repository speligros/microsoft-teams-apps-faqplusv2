// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    
    /// <summary>
    /// This a Startup class for this Bot.
    /// </summary>
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Startup Configuration.</param>
        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.Configuration = configuration;
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Hosting Environment.</param>
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"> Service Collection Interface.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this._logger.LogInformation($"StartUp - Configuring services...", SeverityLevel.Warning);

            services.AddApplicationInsightsTelemetry();

            this._logger.LogInformation($"StartUp - Loading KnowledgeBase settings...", SeverityLevel.Warning);
            services.Configure<KnowledgeBaseSettings>(knowledgeBaseSettings =>
            {
                knowledgeBaseSettings.SearchServiceName = this.Configuration["SearchServiceName"];
                knowledgeBaseSettings.SearchServiceQueryApiKey = this.Configuration["SearchServiceQueryApiKey"];
                knowledgeBaseSettings.SearchServiceAdminApiKey = this.Configuration["SearchServiceAdminApiKey"];
                knowledgeBaseSettings.SearchIndexingIntervalInMinutes = this.Configuration["SearchIndexingIntervalInMinutes"];
                knowledgeBaseSettings.StorageConnectionString = this.Configuration["StorageConnectionString"];
            });

            this._logger.LogInformation($"StartUp - Loading Luis settings...", SeverityLevel.Warning);
            services.Configure<LuisSettings>(luisSettings =>
            {
                luisSettings.AppId = this.Configuration["Luis:LuisAppId"];
                luisSettings.APIKey = this.Configuration["Luis:LuisAPIKey"];
                luisSettings.APIHostName = this.Configuration["Luis:LuisAPIHostName"];
            });

            this._logger.LogInformation($"StartUp - QNA settings...", SeverityLevel.Warning);
            services.Configure<QnAMakerSettings>(qnAMakerSettings =>
            {
                qnAMakerSettings.ScoreThreshold = this.Configuration["ScoreThreshold"];
            });

            this._logger.LogInformation($"StartUp - Bot settings...", SeverityLevel.Warning);
            services.Configure<BotSettings>(botSettings =>
            {
                botSettings.AccessCacheExpiryInDays = Convert.ToInt32(this.Configuration["AccessCacheExpiryInDays"]);
                botSettings.AppBaseUri = this.Configuration["AppBaseUri"];
                botSettings.MicrosoftAppId = this.Configuration["MicrosoftAppId"];
                botSettings.TenantId = this.Configuration["TenantId"];
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            this._logger.LogInformation($"StartUp - Loading ConfigurationDataProvider...", SeverityLevel.Warning);
            services.AddSingleton<Common.Providers.IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(this.Configuration["StorageConnectionString"]));
            services.AddHttpClient();
            this._logger.LogInformation($"StartUp - Loading ConfigurationCredentialProvider...", SeverityLevel.Warning);
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            this._logger.LogInformation($"StartUp - Loading TicketsProvider...", SeverityLevel.Warning);
            services.AddSingleton<ITicketsProvider>(new TicketsProvider(this.Configuration["StorageConnectionString"]));
            this._logger.LogInformation($"StartUp - Loading BotFrameworkHttpAdapter...", SeverityLevel.Warning);
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
            this._logger.LogInformation($"StartUp - Loading MicrosoftAppCredentials...", SeverityLevel.Warning);
            services.AddSingleton(new MicrosoftAppCredentials(this.Configuration["MicrosoftAppId"], this.Configuration["MicrosoftAppPassword"]));

            this._logger.LogInformation($"StartUp - Loading QnAMakerClient...", SeverityLevel.Warning);
            IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(this.Configuration["QnAMakerSubscriptionKey"])) { Endpoint = this.Configuration["QnAMakerApiEndpointUrl"] };
            string endpointKey = Task.Run(() => qnaMakerClient.EndpointKeys.GetKeysAsync()).Result.PrimaryEndpointKey;

            this._logger.LogInformation($"StartUp - Loading QnaServiceProvider...", SeverityLevel.Warning);
            services.AddSingleton<IQnaServiceProvider>((provider) => new QnaServiceProvider(
                provider.GetRequiredService<Common.Providers.IConfigurationDataProvider>(),
                provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(),
                qnaMakerClient,
                new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = this.Configuration["QnAMakerHostUrl"] }));
            this._logger.LogInformation($"StartUp - Loading ActivityStorageProvider...", SeverityLevel.Warning);
            services.AddSingleton<IActivityStorageProvider>((provider) => new ActivityStorageProvider(provider.GetRequiredService<IOptionsMonitor<KnowledgeBaseSettings>>()));
            this._logger.LogInformation($"StartUp - Loading KnowledgeBaseSearchService...", SeverityLevel.Warning);
            services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(this.Configuration["SearchServiceName"], this.Configuration["SearchServiceQueryApiKey"], this.Configuration["SearchServiceAdminApiKey"], this.Configuration["StorageConnectionString"]));

            // Luis service
            this._logger.LogInformation($"StartUp - Loading LuisServiceProvider...", SeverityLevel.Warning);
            services.AddSingleton<ILuisServiceProvider>((provider) => new LuisServiceProvider(
                provider.GetRequiredService<Common.Providers.IConfigurationDataProvider>(),
                provider.GetRequiredService <IOptionsMonitor<LuisSettings>>()));

            this._logger.LogInformation($"StartUp - Loading SearchService...", SeverityLevel.Warning);
            services.AddSingleton<ISearchService, SearchService>();
            this._logger.LogInformation($"StartUp - Loading MemoryCache...", SeverityLevel.Warning);
            services.AddSingleton<IMemoryCache, MemoryCache>();

            services.AddTransient(sp => (BotFrameworkAdapter)sp.GetRequiredService<IBotFrameworkHttpAdapter>());
            services.AddTransient<IBot, FaqPlusPlusBot>();

            // Create the telemetry middleware(used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            services.AddMemoryCache();

            this._logger.LogInformation($"StartUp - Finished!", SeverityLevel.Warning);
        }
    }
}