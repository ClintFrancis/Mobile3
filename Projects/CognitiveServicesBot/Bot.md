# The Bot

## Exploring the Solution

The source for the bot has been written in C#. If you'd like to explore Node.js specific examples there are plenty of alternate projects available from Microsoft's BotBuilder Github account [here](https://github.com/Microsoft/BotBuilder-Samples).

The source for this example provides everything you need to demo a working bot and if you want to you can get started straight away by populating the required keys found in the Bot Configuration section.  

Here's what we'll cover as part of this bot:

- Services and models
- Message Control
- Dialogs
- Scorable Dialogs
- Rich Cards

***
## Services And Models
The first thing we'll look at with the project is how the Services and models have been set up.

#### AzureSearchService
The `AzureSearchService` class simply provides us with a group of methods to access our CosmosDB data via the Azure Search Service. For the most part these methods are provide us with a way of searching for different items, but we've also implemented a `FetchFacets` method to return a list of all the available categories in our database.

The other methods to note are:

```
public async Task<string> CheckCategoryExists(string value)
```

```
public async Task<string> CheckAPIExists(string value}
```

Both of these methods perform a fuzzy search and return a _corrected_ value if matched. This just ensures that when replying to the user we use the corrected search term to indicate the correct name. For example _'commuter vision'_ will be corrected to _'Computer Vision'_.

#### ProductModel
The `ProductModel` class is used to track a users search history during an active `Dialog` with the bot. Its a straight forward model that contains two static helper methods to set and clear the data associated with the current state.

State data can be used for many purposes, such as determining where the prior conversation left off or simply greeting a returning user by name. In this case I'm storing the history of a users query. You can read more about how to use state data [here](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-state).


## Message Control
The first class we're going to take a look at is the `MessagesController.cs`. The `MessagesController` is the main contact point with the bot's users, more specifically the Post method handles incoming messages from the end user as well as replying to them.

When the bot receives an activity from the user, it checks to see what type of activity it is, then acts accordingly. There are various [activity types](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-activities) built into the bot framework:

- _Message_ - Messages are the core type of interaction between Bot and the end users. These can be anything from simple text to complex interactions with UI elements.
- _DeleteUserData_ - Indicates to a bot that a user has requested that the bot delete any user data it may have stored.
- ConversationUpdate - Indicates that the bot was added to a conversation, other members were added to or removed from the conversation, or conversation metadata has changed.
- ContactRelationUpdate - Indicates that the bot was added or removed from a user's contact list.
- Typing - Indicates that the user or bot on the other end of the conversation is compiling a response.
- Ping - Represents an attempt to determine whether a bot's endpoint is accessible.

When an `Activity` is sent to the `Post` method, we check the type and forward it to the right handler.
 
```
public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
{
	if (activity.Type == ActivityTypes.Message)
	{
		await Conversation.SendAsync(activity, MakeRoot);
	}
	else
	{
		HandleSystemMessage(activity);
	}
	var response = Request.CreateResponse(HttpStatusCode.OK);
	return response;
}
```  

Any activities with the type `Message` are routed to the `MakeRoot` method which generates a new instance of the `DefaultDialog.cs` class. Ordinarily you could just send the activity to a new `IDialog` instance directly, but the `DefaultDialog` class inherits from `LuisDialog` so we need to instance it differently.

```
internal static IDialog<object> MakeRoot()
{
	return Chain.From(() => new DefaultDialog());
}
```


## Default Dialog

The `DefaultDialog` class handles all of our LUIS interactions with the user so we're going to focus on it in detail. The first thing to point out is that this `Dialog` inherits from `LuisDialog` which allows LUIS to trigger methods that match its intents directly. 

```
[LuisModel("[LUIS_APP_ID]", "[LUIS_SERVICE_KEY]", domain: "[LUIS_DOMAIN]")]
[Serializable]
public class DefaultDialog:LuisDialog<object>
```

#### Methods for each LUIS intent on our LUIS model
Each of the LUIS intents is matched to a different method, using the `LuisIntent` attribute for identification.

### None
If we can't find a match to the users utterance then this method will handle replying to the user.

```
[LuisIntent("None")]
public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
{
    string response = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
    await context.PostAsync(response);
}
```

### services.search
When LUIS matches a users intent with a service search this method will handle the dialog. As this function has a lot going on let's break it down piece by piece.

This method will handle all of our `services.search` intents that are recognised by LUIS. The method has been marked with a LuisIntent attribute that declares the specific intent registered in our LUIS app.

```
[LuisIntent("services.search")]
public async Task SearchService(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
```

The first thing that we do after receiving the incoming `LuisResult` is to check if there are any identified _product_ entities.

```
var messageToForward = await message;

EntityRecommendation productSearch;
if (result.TryFindEntity(ServiceEntities.Product, out productSearch))
{ ...
```

If there are any `cognitiveservice.product` entities present in the `LuisResult` we want to qualify what the user is searching for.

We retrieve the ProductModel from the current conversation context (If one doesn't exist a new one will be created). For consistency the `SearchTerm` is also converted to title case.

```
var model = ProductModel.GetContextData(context);
// Title case the search entity for consistency
model.SearchTerm = new CultureInfo("en").TextInfo.ToTitleCase(productSearch.Entity.ToLower());
```

To help narrow down the scope of a users request, If the`Result.Query` contains either _api_ or _category_ we use the `searchService.CheckAPIExists()` or `searchService.CheckCategoryExists()` methods to validate that the entity exists and if necessary return the correct name. 

The corrected value is stored in the `ProductModel` so we can keep track of the users intention when the `context` is passed through to another dialog. 

```
// Are we searching for an API?
string query = result.Query.ToLower();
if (query.Contains("api"))
{
	var matchedAPI = await searchService.CheckAPIExists(model.SearchTerm);
	if (!string.IsNullOrEmpty(matchedAPI))
		model.API = model.SearchTerm = matchedAPI;
}

// Are we searching for a Category?
else if (query.Contains("category"))
{
	var matchedCategory = await searchService.CheckCategoryExists(model.SearchTerm);
	if (!string.IsNullOrEmpty(matchedCategory))
		model.Category =  model.SearchTerm = matchedCategory;
}
```

The user is then notified that we're starting the search and
the `context` is forwarded on to a new dialog - in this case the `ServiceSearchDialog`. 

You'll note that one of the arguments provided is called `AfterDialog`. This is the method we return to _after_ the new dialog on the stack has been completed. It's important that we close out each `Dialog` instance as it completes to pop it off the `Dialog` stack.

```
// Is this a general product search?
ProductModel.SetContextData(context, model);

await context.PostAsync($"Ok, let me look for information on '{model.SearchTerm}'.");
await context.Forward(new ServiceSearchDialog(), AfterDialog, messageToForward, CancellationToken.None);
```

```
private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
{
	var messageHandled = await result;
	ProductModel.ClearContextData(context);
	context.Done<object>(null);
}
```


Lastly, if we were unable to find a `cognitiveservice.product` entity in the `LuisResult` we forward the context to the `ServiceExploreDialog` which will guide the user in finding the information they want.
	
```
// If we cant identify a product entity, start an explore dialog
else
{
    await context.PostAsync("let's explore what you can do with the Cognitive Services.");
	await context.Forward(new ServiceExploreDialog(), AfterDialog, messageToForward, CancellationToken.None);
}
```

### services.help
When LUIS matches a users Intent with a request for help this method will handle the context. This method simply let's the user know that we've received their query before we forward the context on to the `ServiceExploreDialog` to guide the user in their search.

```
[LuisIntent("services.help")]
public async Task RequestHelp(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
{
	var messageToForward = await message;

	await context.PostAsync("Let's get started looking at the Cognitive Services");
	await context.PostAsync("Hold on one second!");

	await context.Forward(new ServiceExploreDialog(), AfterDialog, messageToForward, CancellationToken.None);
}
```

### ServiceSearchDialog

The `ServiceSearchDialog` is a standard `IDialog<object>` instance. This class contains three methods:

#### `StartAsync`
The `StartAsync` method is required as part of an `IDialog` instance. We're simply waiting for the message to arrive, which is then forwarded on the the `MessageRecievedAsync` method.

```
public async Task StartAsync(IDialogContext context)
{
	context.Wait(this.MessageReceivedAsync);
}
```

#### `MessageReceivedAsync`
The `MessageReceivedAsync` method is where we handle all our search logic, so lets look at it in detail.

```
public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
```

We check whether we already have an _api_ or _category_ set in the `ProductModel`. If we do, we message the user that we have found something for them and then forward the `context` on to a new instance of the `ServiceExploreDialog` class. This means that if the user is interested in a specific _api_ they can be routed directly to the _api_ Dialog.

```
var model = ProductModel.GetContextData(context);

if(!string.IsNullOrEmpty(model.API) || !string.IsNullOrEmpty(model.Category))
{
	await context.PostAsync($"I've found some information on '{model.SearchTerm}'");

	ProductModel.SetContextData(context, model);
	await context.Forward(new ServiceExploreDialog(), AfterDialog, messageToForward, CancellationToken.None);
}
``` 

If there isn't an _api_ or _category_ present in the `ProductModel` we perform a general search using the supplied `SearchTerm`. The results of the search are then returned to the user as a carousel of custom `AdaptiveCard` views. These `AdaptiveCard` views are created using the `CardUtil.CreateFeatureCard(Value value)` util method which we'll look at these in more detail soon.

Note that at the end of each branch in the logic we're calling `context.Done<object>(null)` to close the `Dialog` and remove it from the stack. If you forget to signal that you're done with the `context` then the user will get stuck in the same `IDialog` instance. 

```
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
```

#### `AfterDialog`

The `AfterDialog` method ensures that we signal that we are done with the `context` once any child `IDialog` instances complete. If there's a message passed back, we make sure thats passed on as well before this `IDialog` is closed. 

```
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
```

### ServiceExploreDialog

The `ServiceExploreDialog` class simply guides the user through a decision hierarchy going from: _Category > Api > Feature_. 

Once we determine which step a user is at in their search we forward the `context` on the the appropriate `IDialog` for move down the hierarchy to the information they're looking for.

```
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
```

We also have four follow up handlers declared:
`AfterCategoryDialog`, `AfterAPIDialog`, `AfterFeatureDialog` and `AfterFeatureDisplayDialog`. Each of these methods passes the user to the next step which will be one of the four granular dialogs. 

The full explore pattern is:

```
> ChooseCategoryDialog
	> [AfterCategoryDialog]
		> ChooseAPIDialog
			> [AfterAPIDialog]
				> ChooseFeatureDialog
					> [AfterFeatureDialog]
						> DisplayFeaturesDialog
							> [AfterFeatureDisplayDialog]
```


### Granular Dialogs
The four "Granular Dialog" classes are used to prompt the user are used prompt the user to make a choice and / or display search results. By separating each of these steps out into their own `IDialog` classes we're able to easily route the user directly into any stage of the conversation giving us greater flexibility. Take a look through each of the classes to get a sense of how they operate.

- `ChooseCategoryDialog`
- `ChooseAPIDialog`
- `ChooseFeatureDialog`
- `DisplayFeaturesDialog`

### Scorable Dialogs

Scorable dialogs act like global message handlers. When users attempt to access certain functionality within a bot by using words like "help," "cancel," or "start over" in the middle of a conversation - the bot is expecting a different response. Scorable dialogs allow the bot to gracefully handle these requests.

Scorable dialogs monitor all incoming messages and determine whether a message is actionable in some way. Messages that are scorable are assigned a score between [0 – 1] by each scorable dialog. 

The scorable dialog that determines the highest score is added to the top of the dialog stack and then hands the response to the user. After the scorable dialog completes execution, the conversation continues from where it left off.

In this bot we're using a single scorable dialog `CancelScorable` to cancel our current dialog and restart. When the user enters 'cancel' we reset the dialog stack and the `ProductModel` to start again.

There's a lot of information on using scorable dialogs available to read here:
[https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-scorable-dialogs](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-scorable-dialogs)

### Rich Cards

Bots and channels typically exchange text strings but some channels also support exchanging attachments, which lets your bot send richer messages to users. 

We're going to use rich cards to return our _service_ results to the user within our app. Using the `CardUtil` class we can generate a rich card that is specific to the users channel (Slack, Skype, etc).

```
public static Attachment CreateCardAttachment(string channelID, Value value)
```

This project currently supports two types of rich cards `ThumbNail` and `Adaptive`. Skype doesn't currently offer support for Adaptive Cards, so we're opting to use a Thumbnail card instead if the user is messaging using Skype.

#### Thumbnail Card

Thumbnail cards typically contain a single thumbnail image, one or more buttons, and text. 

#### Adaptive Card

Adaptive Cards are an open card exchange format enabling developers to exchange UI content in a common and consistent way across multiple channels. 

While the Adaptive Card in this project is build using code you can also use a `json` file to declare a cards layout.

Check out [adaptivecards.io](http://adaptivecards.io/) for more information on how to use and create Adaptive Cards.

