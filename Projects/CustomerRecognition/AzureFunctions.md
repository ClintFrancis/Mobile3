# Azure Functions

Azure Functions are a solution for easily running small pieces of code, or "functions" in the cloud. You can write just the code you need for the problem at hand, without worrying about a whole application or the infrastructure to run it. Functions can make development even more productive, and you can use your development language of choice, such as C#, F#, Node.js, Java, or PHP. As Azure Functions are serverless applications, you only pay for the time your code runs and Azure will scale as needed.

There are a range of different Function types available, but for this project we're going to be using HTTPTriggers. HTTPTriggers will trigger the execution of our code by using an HTTP request, so essentially whenever our mobile application makes a request, the function will be invoked.

You can read more on the full list of function types available here:</br>
[https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)

## Set up Functions
In this chapter we'll focus on setting up the Function App and later on we'll configure the source code for use with our deployment.

## Create a Function App
For this project we'll be need to create a new Face API account to get our unique API keys for the service.

### 1. Navigate to Function App in the Azure Portal

/images/5_01_Function_App.png

### 2. Create a Function App 

Create a new Function App with a unique id, in this case I'll be using _'customerrecognitionapp'_. We'll use our existing Resource Group and create a new Storage instance for hosting the scripts we'll add later.

/images/5_02_Function_App_Create.png

## To Be Continued

Now that our Functions App has been successfully created we'll configure our functions in the next chapter before we publish them to Azure.