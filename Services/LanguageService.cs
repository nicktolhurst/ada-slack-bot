using System;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.Conversations;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using System.Threading;
using Azure.Core;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using static Ada.Helpers.Helpers;

namespace Ada.Services
{
    public class LanguageService
    {
        private readonly string _conversationalLanguageUnderstandingProjectName;
        private readonly string _conversationalLanguageUnderstandingDeploymentName;

        public LanguageService(IConfiguration configuration)
        {
            var azureLanguageServiceConfiguration = configuration.GetSection("AzureLanguageService");

            var endpoint = new Uri(azureLanguageServiceConfiguration.GetValue<string>("Uri"));
            var credential = new AzureKeyCredential(azureLanguageServiceConfiguration.GetValue<string>("Key"));
            var questionAnsweringConfig = azureLanguageServiceConfiguration.GetSection("QnA");

            QuestionAnsweringClient = new QuestionAnsweringClient(endpoint, credential);

            QuestionAnsweringProject = new QuestionAnsweringProject(questionAnsweringConfig.GetValue<string>("Project"), questionAnsweringConfig.GetValue<string>("Deployment"));

            var conversationAnalysisConfig = azureLanguageServiceConfiguration.GetSection("ConversationAnalysis");
            ConversationAnalysisClient = new ConversationAnalysisClient(endpoint, credential);
            _conversationalLanguageUnderstandingProjectName = conversationAnalysisConfig.GetValue<string>("Project");
            _conversationalLanguageUnderstandingDeploymentName = conversationAnalysisConfig.GetValue<string>("Deployment");
        }

        public QuestionAnsweringClient QuestionAnsweringClient { get; }
        public QuestionAnsweringProject QuestionAnsweringProject { get; }
        public ConversationAnalysisClient ConversationAnalysisClient  { get; }

        public async Task<RecognizerResult> RecognizeIntentAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await RecognizeIntentInternalAsync(turnContext?.Activity?.AsMessageActivity()?.Text, turnContext, cancellationToken);
        }

        private async Task<RecognizerResult> RecognizeIntentInternalAsync(string utterance, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var request = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = utterance,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName = _conversationalLanguageUnderstandingProjectName,
                    deploymentName = _conversationalLanguageUnderstandingDeploymentName,
                    stringIndexType = "Utf16CodeUnit", // Use Utf16CodeUnit for strings in .NET.
                },
                kind = "Conversation",
            };


            var cluResponse = await ConversationAnalysisClient.AnalyzeConversationAsync(RequestContent.Create(request));

            using JsonDocument result = JsonDocument.Parse(cluResponse.ContentStream);
            
            var recognizerResult = RecognizerResultBuilder.BuildRecognizerResultFromCluResponse(result, utterance);

            var traceInfo = JObject.FromObject(
                new
                {
                    response = result,
                    recognizerResult,
                });

            return recognizerResult;
        }

    }
}