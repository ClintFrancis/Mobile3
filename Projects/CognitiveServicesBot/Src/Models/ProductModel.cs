using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CognitiveServicesBot
{
    public class ProductModel
    {
        public const string ID = "ProductModel";
        public string Category { get; set; }
        public string Feature { get; set; }
		public string API { get; set; }
        public string SearchTerm { get; set; }

		public static ProductModel GetContextData(IDialogContext context)
		{
			ProductModel model;
			context.ConversationData.TryGetValue<ProductModel>(ProductModel.ID, out model);
			if (model == null)
			{
				model = new ProductModel();
				SetContextData(context, model);
			}

			return model;
		}

		public static void SetContextData(IDialogContext context, ProductModel model)
		{
			context.ConversationData.SetValue<ProductModel>(ProductModel.ID, model);
		}

		public static void ClearContextData(IDialogContext context)
		{
			context.ConversationData.RemoveValue(ProductModel.ID);
		}
	}
}