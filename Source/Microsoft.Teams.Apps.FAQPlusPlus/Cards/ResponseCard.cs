// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// Construct the response card - when user asks a question to QnA Maker through bot.
        /// </summary>
        /// <param name="question">Knowledgebase question, from QnA Maker service.</param>
        /// <param name="answer">Knowledgebase answer, from QnA Maker service.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <returns>Response card.</returns>
        public static Attachment GetCard(string question, string answer, string userQuestion)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Strings.ResponseHeaderText,
                        Wrap = true,
                    },/*
                    new AdaptiveTextBlock
                    {
                        Text = question,
                        Wrap = true,
                    },*/
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.AskAnExpertButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.AskAnExpertDisplayText,
                                Text = Constants.AskAnExpert,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.ShareFeedbackButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.ShareFeedbackDisplayText,
                                Text = Constants.ShareFeedback,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        public static Attachment GetAnswerCard(string question, string answer, string userQuestion, IList<PromptDTO> prompts )
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true,
                    },
                },
                Actions = GenerateActions(prompts),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        // <summary>
        /// Construct the response card for "Was this information helpful?" - when a response is returned to the user, ask if it was helpful.
        /// </summary>
        public static Attachment GetWasItHelpfulCard()
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "¿Ha sido útil la respuesta?",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "Sí",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "Sí",
                                Text = Constants.YesCommand,
                            },
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "No",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "No",
                                Text = Constants.NoCommand,
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        // <summary>
        /// Construct the response card for "Do you need more help?" - when the user resported the info was not helpful, ask for more help.
        /// </summary>
        public static Attachment GetNeedMoreHelpCard(string question)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "¿Necesitas más ayuda?",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.AskAnExpertButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.AskAnExpertDisplayText,
                                Text = Constants.AskAnExpert,
                            },
                            UserQuestion = question,
                            KnowledgeBaseAnswer = "",
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.ShareFeedbackButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.ShareFeedbackDisplayText,
                                Text = Constants.ShareFeedback,
                            },
                            UserQuestion = question,
                            KnowledgeBaseAnswer = "",
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        public static Attachment GetNewUserCard()
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "Formulario Usuario nuevo",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "Alta de Usuario",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "Proceder a alta usuario",
                                Text = Constants.ShowUserDetailsCommand,
                            },
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "Cancelar",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "Cancelar",
                                Text = Constants.CancelCommand,
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        public static Attachment GetUserDetailsCard()
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "Detalles de usuario",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction> {
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        public static Attachment GetNewCableRequestCard()
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "Nueva petición de cable",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "Nueva petición de cable",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "Proceder a petición de cable",
                                Text = Constants.ShowCableRequestDetailsCommand,
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        public static Attachment GetCableRequestDetailsCard()
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "Detalles de petición de cable",
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }


        private static List<AdaptiveAction> GenerateActions(IList<PromptDTO> prompts)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();
            foreach (PromptDTO prompt in prompts) {
                actionsList.Add(new AdaptiveSubmitAction
                {
                    Title = prompt.DisplayText,
                    Data = new ResponseCardPayload
                    {
                        MsTeams = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            DisplayText = prompt.DisplayText,
                            Text = prompt.DisplayText,
                        },
                    },

                });
            }

            return actionsList;
        }

        public static Attachment GetNewUserCard(AskUserDetailsCardPayload cardPayload)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Text = "Formulario usuario nuevo",
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskUserDetailsCardPayload.Name),
                        Placeholder = "nombre",
                        IsMultiline = false,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.Name,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Apellido: ",
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskUserDetailsCardPayload.Surname),
                        Placeholder = "apellido",
                        IsMultiline = true,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.Surname,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Departamento: ",
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskUserDetailsCardPayload.Department),
                        Placeholder = "departamento",
                        IsMultiline = true,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.Department,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Responsable: ",
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskUserDetailsCardPayload.Responsible),
                        Placeholder = "responsable",
                        IsMultiline = true,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.Responsible,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Fecha alta: ",
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskUserDetailsCardPayload.CreateDate),
                        Placeholder = "fecha alta",
                        IsMultiline = true,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.CreateDate,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "Alta de Usuario",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "Proceder a alta usuario",
                                Text = Constants.ShowUserDetailsCommand,
                            },
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        // TODO should be a localized text: String.XXXXXX
                        Title = "Cancelar",
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                // TODO should be a localized text: String.XXXXXX
                                DisplayText = "Cancelar",
                                Text = Constants.CancelCommand,
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }
    }
}