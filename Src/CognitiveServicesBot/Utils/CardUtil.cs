using AdaptiveCards;
using CognitiveServicesBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace CognitiveServicesBot.Utils
{
	public static class CardUtil
	{
		public static AdaptiveCard CreateFeatureCard(Value value)
		{
			AdaptiveCard card = new AdaptiveCard();
			card.Speak = value.Name;
			card.Body = new List<CardElement> {
					new ColumnSet
					{
						Columns =
						{
							new Column
							{
								Size = ColumnSize.Auto,
								Items =
								{
									new Image
									{
										Url = WebConfigurationManager.AppSettings["BlobStorageURL"] + value.imageURL,
										Size = ImageSize.Small,
										Style = ImageStyle.Normal
									}
								}
							},
							new Column
							{
								Size = ColumnSize.Stretch,
								Items =
								{
									new TextBlock
									{
										Text = value.Name,
										Weight = TextWeight.Bolder,
										Size = TextSize.Large
									}
								}
							}
						}
					},
					new Container
					{
						Items =
						{
							new TextBlock
							{
								Text = value.Description,
								Speak = value.Description,
								Wrap = true
							},
							new FactSet
							{
								Facts =
								{
									new Fact{Title = "Api", Value = value.API},
									new Fact{Title = "Category", Value = value.Category}
								}
							}
						}
					}
				};
			card.Actions = new List<ActionBase>
			{
				new OpenUrlAction{Title = "View Documentation", Url=value.documentationURL}
			};
			
			return card;
		}
	}
}