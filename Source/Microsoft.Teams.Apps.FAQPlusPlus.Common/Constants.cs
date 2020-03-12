// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common
{
    /// <summary>
    /// constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Source.
        /// </summary>
        public const string Source = "Editorial";

        /// <summary>
        /// Delete command.
        /// </summary>
        public const string DeleteCommand = "delete";

        /// <summary>
        /// No command.
        /// </summary>
        public const string NoCommand = "no";

        /// <summary>
        /// Yes command.
        /// </summary>
        public const string YesCommand = "yes";

        /// <summary>
        /// Salute command.
        /// </summary>
        public const string SaluteCommand = "saludo";

        /// <summary>
        /// Goodbye command.
        /// </summary>
        public const string GoodbyeCommand = "despedida";

        public const string NewUserCommand = "proceder_a_alta_usuario";

        public const string ShowUserDetailsCommand = "mostrar_detalles_de_usuario";

        public const string NewCableRequestCommand = "proceder_a_petición_cable";

        public const string ShowCableRequestDetailsCommand = "mostrar_detalles_de_petición_de_cable";

        public const string CancelCommand = "cancelar";

        /// <summary>
        /// Regular expression pattern for valid redirection url.
        /// It checks whether the url is valid or not, while adding/editing the qna pair.
        /// </summary>
        public const string ValidRedirectUrlPattern = @"^(http|https|)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([a-zA-Z0-9\-\?\,\'\/\+&%\$#_]+)";

        /// <summary>
        /// Name of the QnA metadata property to map with the date and time the item was added.
        /// </summary>
        public const string MetadataCreatedAt = "createdat";

        /// <summary>
        /// Name of the QnA metadata property to map with the user who created the item.
        /// </summary>
        public const string MetadataCreatedBy = "createdby";

        /// <summary>
        /// Name of the QnA metadata property to map with the conversation id of the item.
        /// </summary>
        public const string MetadataConversationId = "conversationid";

        /// <summary>
        ///   Name of the QnA metadata property to map with the date and time the item was updated.
        /// </summary>
        public const string MetadataUpdatedAt = "updatedat";

        /// <summary>
        /// Name of the QnA metadata property to map with the user who updated the item.
        /// </summary>
        public const string MetadataUpdatedBy = "updatedby";

        /// <summary>
        /// Name of the QnA metadata property to map with the activity reference id for future reference.
        /// </summary>
        public const string MetadataActivityReferenceId = "activityreferenceid";

        /// <summary>
        /// TeamTour - text that triggers team tour action.
        /// </summary>
        public const string TeamTour = "team tour";

        /// <summary>
        /// TakeAtour - text that triggers take a tour action for the user.
        /// </summary>
        public const string TakeATour = "take a tour";

        /// <summary>
        /// AskAnExpert - text that renders the ask an expert card.
        /// </summary>
        public const string AskAnExpert = "ask_an_expert";

        /// <summary>
        /// Feedback - text that renders share feedback card.
        /// </summary>
        public const string ShareFeedback = "share feedback";

        /// <summary>
        /// Table name where SME activity details from bot will be saved.
        /// </summary>
        public const string TicketTableName = "Tickets";

        /// <summary>
        /// Name of column value to map with knowledgebase id in table storage.
        /// </summary>
        public const string KnowledgeBaseEntityId = "KnowledgeBaseId";

        /// <summary>
        /// FAQ Plus blob storage container name.
        /// </summary>
        public const string StorageContainer = "faqplus-search-container";

        /// <summary>
        /// FAQ Plus folder name under FAQ Plus blob storage container name.
        /// </summary>
        public const string BlobFolderName = "faqplus-metadata";

        /// <summary>
        /// Represents the command text to identify the action.
        /// </summary>
        public const string PreviewCardCommandText = "previewcard";
    }
}
