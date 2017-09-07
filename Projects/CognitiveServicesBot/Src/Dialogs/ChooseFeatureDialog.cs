using CognitiveServicesBot.Model;
using CognitiveServicesBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CognitiveServicesBot.Dialogs
{
	[Serializable]
	public class ChooseFeatureDialog : IDialog<string>
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
			if (string.IsNullOrEmpty(model.API))
			{
				context.Done("Sorry, Something went wrong!");
			}

			SearchResult searchResult = await searchService.FilterByAPI(model.API);
			if (searchResult.value.Length != 0)
			{
				var apis = (from item in searchResult.value
							select item.Name)
							.ToList();

				if (apis.Count() == 1)
				{
					model.Feature = apis.FirstOrDefault();
					ProductModel.SetContextData(context, model);

					context.Done(apis.FirstOrDefault());
				}
				else if(apis.Count() > 1)
				{
					string api = new CultureInfo("en").TextInfo.ToTitleCase(model.API.ToLower());
					PromptDialog.Choice(context, OnFeatureOptionSelected, apis, $"Here are some of the key features available as part of the {model.API}", "Sorry, thats not a valid option", 3);
				}
				else
				{
					context.Fail(new ArgumentException($"No APIs were found with the name {message}"));
				}
			}
			else
			{
				context.Done("Oops! Something went wrong!");
			}
		}

		private async Task OnFeatureOptionSelected(IDialogContext context, IAwaitable<string> result)
		{
			var feature = await result;
			var model = ProductModel.GetContextData(context);
			model.Feature = feature;
			ProductModel.SetContextData(context, model);

			context.Done($"Ok, here's what the {feature} feature offers:");
		}
	}
}