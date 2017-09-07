using CognitiveServicesBot.Model;
using CognitiveServicesBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CognitiveServicesBot.Dialogs
{
	[Serializable]
	public class ChooseCategoryDialog: IDialog<string>
	{
		private readonly AzureSearchService searchService = new AzureSearchService();

		public async Task StartAsync(IDialogContext context)
		{
			context.Wait(this.MessageReceivedAsync);
		}

		public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var message = context.Activity.AsMessageActivity();

			FacetResult facetResult = await searchService.FetchFacets();
			if (facetResult.searchfacets.Category.Length != 0)
			{
				var categories = (
					from item in facetResult.searchfacets.Category
					select item.value)
					.ToList();

				PromptDialog.Choice(context, OnCategoryOptionSelected, categories, $"The Cognitive Services are divided into five main categories. Which category are you interested in finding out more about?", "Sorry, thats not a valid option", 3);
			}

			else
			{
				context.Done("Oops! Something went wrong!");
			}
		}

		private async Task OnCategoryOptionSelected(IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				string fullResult = await result;
				string[] option = fullResult.Split(null);
				string category = option[0];
				string preamble;

				switch (category)
				{
					case ServiceCategory.Vision:
						preamble = ServiceDetails.VisionDescription + "\n";
						break;
					case ServiceCategory.Speech:
						preamble = ServiceDetails.SpeechDescription + "\n";
						break;
					case ServiceCategory.Language:
						preamble = ServiceDetails.LanguageDescription + "\n";
						break;
					case ServiceCategory.Knowledge:
						preamble = ServiceDetails.KnowledgeDescription + "\n";
						break;
					case ServiceCategory.Search:
						preamble = ServiceDetails.SearchDescription + "\n";
						break;
					default:
						preamble = ServiceDetails.DefaultDescription;
						await context.PostAsync(preamble);
						context.Done<object>(null);
						return;
				}

				var model = ProductModel.GetContextData(context);
				model.Category = category;
				ProductModel.SetContextData(context, model);

				context.Done(preamble);
			}
			catch (TooManyAttemptsException ex)
			{
				context.EndConversation($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");
			}
		}
	}
}