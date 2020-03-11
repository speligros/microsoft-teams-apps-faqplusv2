namespace Microsoft.Teams.Apps.FAQPlusPlus.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;

    using ErrorResponseException = Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models.ErrorResponseException;



    public class SupportDialog : ComponentDialog
    {
        private readonly ILogger<SupportDialog> _logger;

        private readonly IStatePropertyAccessor<SupportStatus> _supportStatus;        

        private IQnaServiceProvider _qnaServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportDialog"/> class.
        /// </summary>
        /// <param name="userState"></param>
        /// <param name="logger"></param>
        public SupportDialog(
            UserState userState,
            ILogger<SupportDialog> logger)
            : base(nameof(SupportDialog))
        {
            this._logger = logger;
            this._supportStatus = userState.CreateProperty<SupportStatus>("SupportStatus");

            /* TODO original values for steps:
                TransportStepAsync, NameStepAsync, NameConfirmStepAsync, AgeStepAsync, PictureStepAsync, ConfirmStepAsync, SummaryStepAsync,
            */
            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                SearchQNAStepAsync,
                AskIsUsefulStepAsync,
                AskIsMoreHelpRequiredStepAsync,
                AskFurtherActionStepAsync,
                RegisterInfoForExpertStepAsync,
                ShowExpertDetailsStepAsync,
                RegisterInfoForTicketStepAsync,
                ShowTicketDetailsStepAsync,
                GoodbyeStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Get the reply to a question asked by end user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="text">Text message.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task GetQuestionAnswerReplyAsync(
            ITurnContext<IMessageActivity> turnContext,
            string text)
        {
            try
            {
                var queryResult = await this._qnaServiceProvider.GenerateAnswerAsync(question: text, isTestKnowledgeBase: false).ConfigureAwait(false);
                var answerData = queryResult.Answers.First();
                this._logger.LogInformation("QNA identified answer [" + answerData.Id + "] with score [" + answerData.Score + "] for text [" + text + "]");

                if (answerData.Id != -1)
                {
                    AnswerModel answerModel = new AnswerModel();
                    if (Validators.IsValidJSON(answerData.Answer))
                    {
                        answerModel = JsonConvert.DeserializeObject<AnswerModel>(answerData.Answer);
                    }

                    if (!string.IsNullOrEmpty(answerModel?.Title) || !string.IsNullOrEmpty(answerModel?.Subtitle) || !string.IsNullOrEmpty(answerModel?.ImageUrl) || !string.IsNullOrEmpty(answerModel?.RedirectionUrl))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(MessagingExtensionQnaCard.GetEndUserRichCard(text, answerData))).ConfigureAwait(false);
                    }
                    else if ((answerData.Context != null) && (answerData.Context.Prompts != null) && (answerData.Context.Prompts.Count > 0))
                    {
                        this._logger.LogInformation("QNA answer has prompts [" + answerData.Context.Prompts.Count + "]");
                        // Replaced response card for a text
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetAnswerCard(answerData.Questions.FirstOrDefault(), answerData.Answer, text, answerData.Context.Prompts))).ConfigureAwait(false);
                    }
                    else
                    {
                        this._logger.LogInformation("QNA answer has no prompts");
                        // Replaced response card for a text
                        // await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetCard(answerData.Questions.FirstOrDefault(), answerData.Answer, text))).ConfigureAwait(false);
                        await turnContext.SendActivityAsync(MessageFactory.Text(answerData.Answer, answerData.Answer)).ConfigureAwait(false);
                        // Was the answer helpful?
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetWasItHelpfulCard())).ConfigureAwait(false);

                    }
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(text))).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Check if knowledge base is empty and has not published yet when end user is asking a question to bot.
                if (((ErrorResponseException)ex).Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var knowledgeBaseId = await this._configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                    var hasPublished = await this._qnaServiceProvider.GetInitialPublishedStatusAsync(knowledgeBaseId).ConfigureAwait(false);

                    // Check if knowledge base has not published yet.
                    if (!hasPublished)
                    {
                        this._logger.LogError(ex, "Error while fetching the qna pair: knowledge base may be empty or it has not published yet.");
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(text))).ConfigureAwait(false);
                        return;
                    }
                }

                // Throw the error at calling place, if there is any generic exception which is not caught.
                throw;
            }
        }


        private static async Task<DialogTurnResult> SearchQNAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your mode of transport."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Car", "Bus", "Bicycle" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["transport"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to give your age?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your age."),
                    RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> PictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;

            var msg = (int)stepContext.Values["age"] == -1 ? "No age given." : $"I have your age as {stepContext.Values["age"]}.";

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            if (stepContext.Context.Activity.ChannelId == Channels.Msteams)
            {
                // This attachment prompt example is not designed to work for Teams attachments, so skip it in this case
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Skipping attachment prompt in Teams channel..."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please attach a profile picture (or type any message to skip)."),
                    RetryPrompt = MessageFactory.Text("The attachment must be a jpeg/png image file."),
                };

                return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["picture"] = ((IList<Attachment>)stepContext.Result)?.FirstOrDefault();

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var supportStatus = await _supportStatus.GetAsync(stepContext.Context, () => new SupportStatus(), cancellationToken);

                // TODO fulfill data
                supportStatus.Answer = "sample";
                // userProfile.Transport = (string)stepContext.Values["transport"];
                var msg = ".";
                /* TODO
                var msg = $"I have your mode of transport as {userProfile.Transport} and your name as {userProfile.Name}";

                if (userProfile.Age != -1)
                {
                    msg += $" and your age as {userProfile.Age}";
                }

                msg += ".";
                */
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                /* TODO
                if (userProfile.Picture != null)
                {
                    try
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(userProfile.Picture, "This is your profile picture."), cancellationToken);
                    }
                    catch
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("A profile picture was saved but could not be displayed here."), cancellationToken);
                    }
                }
                */
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be kept."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }

        private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var attachments = promptContext.Recognized.Value;
                var validImages = new List<Attachment>();

                foreach (var attachment in attachments)
                {
                    if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
                    {
                        validImages.Add(attachment);
                    }
                }

                promptContext.Recognized.Value = validImages;

                // If none of the attachments are valid images, the retry prompt should be sent.
                return validImages.Any();
            }
            else
            {
                await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a profile picture...");

                // We can return true from a validator function even if Recognized.Succeeded is false.
                return true;
            }
        }
    }
}