using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CognitiveServicesBot.Dialogs
{
	public class CancelResponsesDialog : IDialog<object>
	{
		public async Task StartAsync(IDialogContext context)
		{
			await context.PostAsync("OK, let's start again.");
			ProductModel.ClearContextData(context);

			context.Done<object>(null);
		}
	}
}