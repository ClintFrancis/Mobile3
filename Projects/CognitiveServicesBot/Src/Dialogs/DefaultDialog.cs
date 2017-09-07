using AdaptiveCards;
using CognitiveServicesBot.Services;
using CognitiveServicesBot.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CognitiveServicesBot.Dialogs
{
    [LuisModel("731c6246-5ff3-4225-810a-11d783fac9c8", "815e51a037ba4a6d96e4ff042535d4e2", domain: "westcentralus.api.cognitive.microsoft.com")]
    [Serializable]
    public class DefaultDialog:LuisDialog<object>
    {
		private readonly AzureSearchService searchService = new AzureSearchService();

		[LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            string response = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(response);
        }

        [LuisIntent("services.search")]
        public async Task SearchService(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
			var messageToForward = await message;

			EntityRecommendation productSearch;
            if (result.TryFindEntity(ServiceEntities.Product, out productSearch))
            {
				var model = ProductModel.GetContextData(context);
				// Title case the search entity for consistency
				model.SearchTerm = new CultureInfo("en").TextInfo.ToTitleCase(productSearch.Entity.ToLower());

				// Are we searching for an API?
				string query = result.Query.ToLower();
				if (query.Contains("api"))
				{
					var matchedAPI = await searchService.CheckAPIExists(model.SearchTerm);
					if (!string.IsNullOrEmpty(matchedAPI))
						model.API = model.SearchTerm = matchedAPI;
				}

				// Are we searching for a Category?
				else if (query.Contains("category"))
				{
					var matchedCategory = await searchService.CheckCategoryExists(model.SearchTerm);
					if (!string.IsNullOrEmpty(matchedCategory))
						model.Category =  model.SearchTerm = matchedCategory;
				}

				// Is this a general product search?
				ProductModel.SetContextData(context, model);
				await context.PostAsync($"Ok, let me look for information on '{model.SearchTerm}'.");
				await context.Forward(new ServiceSearchDialog(), AfterDialog, messageToForward, CancellationToken.None);
            }

			// If we cant identify a product entity, start an explore dialog
            else
            {
                await context.PostAsync("let's explore what you can do with the Cognitive Services.");
				await context.Forward(new ServiceExploreDialog(), AfterDialog, messageToForward, CancellationToken.None);
			}
        }

        [LuisIntent("services.help")]
        public async Task RequestHelp(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
			var messageToForward = await message;

			await context.PostAsync("Let's get started looking at the Cognitive Services");
			await context.PostAsync("Hold on one second!");

			await context.Forward(new ServiceExploreDialog(), AfterDialog, messageToForward, CancellationToken.None);
		}

		private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            var messageHandled = await result;

			ProductModel.ClearContextData(context);
			context.Done<object>(null);
		}
	}
}