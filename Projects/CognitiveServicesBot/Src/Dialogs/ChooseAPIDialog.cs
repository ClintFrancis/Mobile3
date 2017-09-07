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
	public class ChooseAPIDialog : IDialog<string>
	{
		private readonly AzureSearchService searchService = new AzureSearchService();

		public async Task StartAsync(IDialogContext context)
		{
			context.Wait(this.MessageReceivedAsync);
		}

		public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var message = await result;
			var model = ProductModel.GetContextData(context);
			if (string.IsNullOrEmpty(model.Category))
			{
				context.Done("Sorry, Something went wrong!");
			}

			SearchResult searchResult = await searchService.FilterByCategory(model.Category);
			if (searchResult.value.Length != 0)
			{
				var apis = (from item in searchResult.value
							select item.API)
							.ToList()
							.Distinct();

				if (apis.Count() == 1)
				{

				}
				else
				{
					PromptDialog.Choice(context, OnAPIOptionSelected, apis, $"Here are the API's available as part of the {model.Category} category", "Sorry, thats not a valid option", 3);
				}
			}

			else
			{
				context.Done($"Sorry, I couldnt find a category that matched '{model.Category}'");
			}
		}

		private async Task OnAPIOptionSelected(IDialogContext context, IAwaitable<string> result)
		{
			var message = await result;
			var model = ProductModel.GetContextData(context);
			model.API = message;
			ProductModel.SetContextData(context, model);

			context.Done($"Ok, here's what the {message} offers:");

			// TODO provide a custom view back to the user
		}
	}
}