using AdaptiveCards;
using CognitiveServicesBot.Model;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace CognitiveServicesBot.Utils
{
	public static class CardUtil
	{
		public static Attachment CreateCardAttachment(string channelID, Value value)
		{
			Attachment attachment = null;
			switch (channelID)
			{
				case "skype":
					attachment = CreateThumbnailCard(value).ToAttachment();
					break;
				default:
					attachment = new Attachment()
					{
						ContentType = AdaptiveCard.ContentType,
						Content = CreateFeatureCard(value)
					};
					break;
			}
			return attachment; 
		}

		public static ThumbnailCard CreateThumbnailCard(Value value)
		{
			var card = new ThumbnailCard();
			card.Title = value.Name;
			card.Subtitle = value.Category + " / " + value.API;
			card.Images = new List<CardImage>()
			{
				new CardImage(WebConfigurationManager.AppSettings["BlobStorageURL"] + value.imageURL)
			};
			card.Text = value.Description;
			card.Buttons = new List<CardAction>()
			{
				new CardAction()
				{
					Value = value.documentationURL,
					Type = ActionTypes.OpenUrl,
					Title = "Documentation"
				}
			};

			return card;
		}

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
									new AdaptiveCards.Fact{Title = "Api", Value = value.API},
									new AdaptiveCards.Fact{Title = "Category", Value = value.Category}
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