using AdaptiveCards;
using CognitiveServicesBot.Services;
using CognitiveServicesBot.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveServicesBot.Dialogs
{
	[Serializable]
    public class ServiceSearchDialog:IDialog<object>
    {
		private readonly AzureSearchService searchService = new AzureSearchService();

		public async Task StartAsync(IDialogContext context)
		{
			context.Wait(this.MessageReceivedAsync);
		}

		public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var messageToForward = await result;

			//await context.PostAsync("Hold on one second!");

			var model = ProductModel.GetContextData(context);

			if(!string.IsNullOrEmpty(model.API) || !string.IsNullOrEmpty(model.Category))
			{
				await context.PostAsync($"I've found some information on '{model.SearchTerm}'");

				ProductModel.SetContextData(context, model);
				await context.Forward(new ServiceExploreDialog(), AfterDialog, messageToForward, CancellationToken.None);
			}

			else
			{
				var results = await searchService.Search(model.SearchTerm);

				if (results.value.Length > 0)
				{
					List<Attachment> attachments = new List<Attachment>();
					for (int i = 0; i < results.value.Length; i++)
					{
						Attachment attachment = new Attachment()
						{
							ContentType = AdaptiveCard.ContentType,
							Content = CardUtil.CreateFeatureCard(results.value[i])
						};
						attachments.Add(attachment);
					}

					var reply = context.MakeMessage();
					reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
					reply.Attachments = attachments;

					await context.PostAsync(reply);

					context.Done<object>(null);
				}
				else
				{
					await context.PostAsync($"Sorry! I couldnt find anything that matched the search '{model.SearchTerm}'");
					context.Done<object>(null);
				}
			}
		}

		private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
		{
			var messageHandled = (string)await result;

			if (!string.IsNullOrEmpty(messageHandled))
			{
				context.Done(messageHandled);
			}
			else
			{
				context.Done<object>(null);
			}
		}
	}
}