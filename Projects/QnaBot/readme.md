# QnA Bot Tutorial

<br/>
This is a quick tutorial where we'll create and publish a new intelligent Bot that uses machine learning and natural language understanding to answer questions about Microsoft Bot Framework.  Then we'll create a native iOS and Android app to interact with our bot.  Let's get started!

<br/>

### Setup: Accounts & Subscriptions

To get started, you'll need to create/login to a few accounts.  (don't worry, they're all free)


**Microsoft Azure**

We'll be using Azure Bot Service, so you'll need an Azure account. If you don't already have one, you can create a **free** Azure account [here](https://azure.microsoft.com/free/).


**Microsoft Bot Framework**

[Create or login](https://dev.botframework.com/login) to your [Microsoft Bot Framework](https://dev.botframework.com/) account. Even if you already have an account, make sure to login, as it'll simplify setting up your bot in Azure.


**QnA Maker**

[Create or login](https://qnamaker.ai/Account/SignIn) to your [QnA Maker](https://qnamaker.ai/) account. Even if you already have an account, make sure to login, as it'll simplify setting up your bot in Azure.

<br/>
  

# Azure Bot Service

Next, we'll use Azure Bot Service to build our bot.

## Create an Azure Web Bot Service

Click [here](https://portal.azure.com/#create/Microsoft.BotServiceSdkGalleryPackage) to create a new Azure Web Bot Service resource in the Azure portal  

`Bot name`: This will be your bot's default name  
`Subscription`: Choose the subscription you want to use from the drop-down  
`Resource Group`: Leave "Create new" selected, and leave the value the same as _App name_  
`Location`: Choose the location that best describes your current location  
`App name`: This will first part of your bot's url endpoint  
`Bot template`: Select a project template. It will create source code of your bot from SDK template  
`App service plan/Location`: Select or create a service plan for your bot  
`Azure Storage`: This storage will be used to store bot state  
`Microsoft App ID and password`: This data will be used to identify bot app with Microsoft App ID. Keep Auto create  
![screenshot](images/qa-bot-sample-29.png)  
![screenshot](images/qa-bot-sample-30.png)  
Click **Create** to create and deploy your new Azure Web Bot Service.


## Train your QnA service

Now that your bot is set up and pointing to a new QnA service, we'll need to teach the QnA service some, well... questions and answers.

Click [here](https://qnamaker.ai/Home/MyServices) to **Create a new service". Paste the following urls into the **URLs** section. (_Note: you're free to use any FAQ urls you want, it won't prevent you from completing this sample)_
- https://docs.microsoft.com/en-us/bot-framework/resources-bot-framework-faq
- https://docs.microsoft.com/en-us/bot-framework/troubleshoot-general-problems

![screenshot](images/qa-bot-sample-35.png)  
After you created a bot you can see the list of found Questions and Answers  
![screenshot](images/qa-bot-sample-36.png)  

Click **Save and retrain**, then click **Publish**. You'll be presented with a screen showing the changes to your services knowledge base that will be published.  
![screenshot](images/qa-bot-sample-16.png)  
Click **Publish**.  

![screenshot](images/qa-bot-sample-32.png) 

We'll need values of **QnAKnowledgebaseId** and **QnASubscriptionKey** to configure our Web Bot to work with our QnA Service.  


## Configure your bot

Once you trained and published your QnA service, you should connect it with your Web Bot.  You can click [here](https://portal.azure.com/#blade/HubsExtension/Resources/resourceType/Microsoft.BotService%2FbotServices) to see a list of Bot Services.  
![screenshot](images/qa-bot-sample-31.png)  

Then you should go to bot's Application Settings and place your **QnAKnowledgebaseId** and **QnASubscriptionKey**  
![screenshot](images/qa-bot-sample-33.png)  

Click **Save**. Now you have trained and configured Azure Web Bot service with QnA Knowledgebase.

## Test your bot

Return to your Azure Web Bot in the Azure portal and click **Test in Web Chat**.
- Type `Hi` in the chat window and press return
- Type `What's the Direct Line channel?` and press return

![screenshot](images/qa-bot-sample-34.png)  


<br/>

# Create a Native Mobile App

_To complete this step you'll need to have Visual Studio for Mac. You can download and install it [here](https://www.visualstudio.com/vs/visual-studio-mac/)._

Now we'll create a native iOS and Android app to interact with our bot. I've created a custom Project Template for Visual Studio that sets everything up automatically. We'll set that up first.


## Install NomadCode.ProjectTemplates extension

Here's how to install NomadCode.ProjectTemplates:

1. Download the `.mpack` file under the Downloads section of the [latest release of NomadCode.ProjectTemplates](https://github.com/colbylwilliams/NomadCode.ProjectTemplates/releases/latest)
2. Launch Visual Studio for Mac, open the _**Visual Studio**_ menu and select _**Extensions...**_
3. In the bottom left of the _Extension Manager_ dialog, click **Install from file...**
4. Choose the `.mpack` file you downloaded in step 1
5. When prompted, select **Install**
6. Once installation is complete, click "Ok" and close the _Extension Manager_ dialog


## Create new App

Now we'll create our mobile app solution.

In Visual Studio for Mac, got to _**File**_ -> _**New Solution...**_ (or Shift + ⌘ + N).  Then, in the _New Solution_ dialog's left column, select **App** (under Multiplatform) then **Native Bot (iOS, Android)** and click **Next**.
![screenshot](images/qa-bot-sample-19.png)
##### Note: Depending on the version of Visual Studio for Mac, this project type may be under _Other -> Miscellaneous -> General_ instead
![screenshot](images/qa-bot-sample-20.png)  
Finally name your project, choose a location, and click **Create**.


## Enable Direct Line channel

Your app will communicate with your bot via the Direct Line channel, so we'll need to enable that back in the Azure portal.

Return to your Azure Bot Service in the Azure portal, and click **CHANNELS** in the top right to configure the channels your bot supports.
![screenshot](images/qa-bot-sample-21.png)
Under the **Add a channel** section, select the **Direct Line** channel.
##### This should open a new tab to _Configure Direct Line_. If it does not, make sure you're logged in to your Microsoft Bot Framework account, and you're not blocking the associated cookies.
![screenshot](images/qa-bot-sample-23.png)
On the _Configure Direct Line_ page, click **+ Add new site** in the left column and enter `Mobile App into the **Name your site** field and click **Done**.  
![screenshot](images/qa-bot-sample-24.png)  
You should now see two **Secret keys**. Click **Show** next to one of the keys, **Copy the key to your clipboard**, then click **Done**  
![screenshot](images/qa-bot-sample-25.png)


## Add Direct Line Secret to  Mobile App

Now that we have the secret, return to Visual Studio for Mac and open (if it's not already) the app solution we created earlier.  

In the Solution Explorer open the `Keys.cs` located in the `Keys` directory of the Shared project, and paste in the Direct Line secret for the value of `DirectLineSecret` on `line 16`.
![screenshot](images/qa-bot-sample-28.png)


## Build and Run the App

Finally, build and run the app on the iOS Simulator and interact with your bot:
- Type `Hi` in the chat window and press return
- Type `What's the Direct Line channel?` and press return  

![screenshot](images/qa-bot-sample-26.png)

---
## Authors
- Colby Williams - [Original Post](http://www.colbylwilliams.com/2017/05/18/intelligent-bot-in-a-native-ios-and-android-app.html)
