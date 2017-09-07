using CognitiveServicesBot.Model;
using CognitiveServicesBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveServicesBot.Dialogs
{
    [Serializable]
    public class ServiceExploreDialog : IDialog<object>
    {
		private readonly AzureSearchService searchService = new AzureSearchService();

		public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
			var messageToForward = await result;

			var model = ProductModel.GetContextData(context);
			if (!string.IsNullOrEmpty(model.API))
			{
				await context.Forward(new ChooseFeatureDialog(), AfterFeatureDialog, messageToForward, CancellationToken.None);
			}

			else if(!string.IsNullOrEmpty(model.Category))
			{
				await context.Forward(new ChooseAPIDialog(), AfterCategoryDialog, messageToForward, CancellationToken.None);
			}

			else
			{
				await context.Forward(new ChooseCategoryDialog(), AfterCategoryDialog, messageToForward, CancellationToken.None);
			}
		}

		private async Task AfterCategoryDialog(IDialogContext context, IAwaitable<string> result)
		{
			var messageHandled = await result;
			if (!string.IsNullOrEmpty(messageHandled))
			{
				await context.PostAsync(messageHandled);
				var model = ProductModel.GetContextData(context);
				if (!string.IsNullOrEmpty(model.Category))
				{
					await context.Forward(new ChooseAPIDialog(), AfterAPIDialog, context.Activity, CancellationToken.None);
				}
			}
			else
				context.Done("Sorry, I wasn't sure what you wanted.");
		}

		private async Task AfterAPIDialog(IDialogContext context, IAwaitable<string> result)
		{
			var messageHandled = await result;
			if (!string.IsNullOrEmpty(messageHandled))
			{
				await context.PostAsync(messageHandled);
				var model = ProductModel.GetContextData(context);
				if (!string.IsNullOrEmpty(model.API))
				{
					await context.Forward(new ChooseFeatureDialog(), AfterFeatureDialog, context.Activity, CancellationToken.None);
				}
			}
			else
				context.Done("Sorry, I wasn't sure what you wanted.");
		}

		private async Task AfterFeatureDialog(IDialogContext context, IAwaitable<string> result)
		{
			var messageHandled = await result;
			await context.Forward(new DisplayFeaturesDialog(), AfterFeatureDisplayDialog, context.Activity, CancellationToken.None);
		}

		private async Task AfterFeatureDisplayDialog(IDialogContext context, IAwaitable<string> result)
		{
			context.Done(string.Empty);
		}
	}
}