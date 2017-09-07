using AdaptiveCards;
using CognitiveServicesBot.Services;
using CognitiveServicesBot.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CognitiveServicesBot.Dialogs
{
	[Serializable]
	public class DisplayFeaturesDialog : IDialog<string>
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

			var value = await searchService.GetFeature(model.Feature);

			Attachment attachment = new Attachment()
			{
				ContentType = AdaptiveCard.ContentType,
				Content = CardUtil.CreateFeatureCard(value)
			};

			var reply = context.MakeMessage();
			reply.Attachments.Add(attachment);

			await context.PostAsync(reply, CancellationToken.None);
			context.Done(string.Empty);
		}
	}
}