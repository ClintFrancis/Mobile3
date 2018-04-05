# Customer Recognition Demo

Let's say we own an chain of coffee shops across the country and we'd like our customers to have a personalised experience whenever they order from us, regardless of _where_ they are.

We've been given the following requirements:

- To be able to greet returning customers by name.
- Look at a customers previous orders.
- Be able to identify customers waiting for their order.
- Gauge a customers satisfaction with the service.

To do this we're going to create an application that works across two stages of the ordering process, _ordering_ and _delivery_.

### Step One: Ordering

Placing an order is a crucial part of our process, here we want to be able to greet a returning customer by name and, if applicable, ask them if they'd like to make the same order as previously made. 

For example:

	Hi James, welcome back! Would you like a double espresso again?

To do this we're going to need a way of recognising the customer and accessing their previous orders by the time they arrive at the till.

To recognise the customers we'll use the Cognitive Services `Face Api`. By simply providing a photo of the customer the `Face Api` will be able to match them with any customers in the database and access their last order. 

The `Face Api` will return a unique `Face ID` alongside other useful information about the person in the image. In this example we'll also be requesting the gender, rough age and current emotion (determined from their facial expression) of the person in the image.

If a customer isn't already in the database, we'll use the unique `Face ID` returned from the `Face Api` to create a new customer in the database. All we need to do is simply ask for a name for the order which we'll also store alongside the `Face ID`. We'll also make a new order in the database and store the order details including the customers emotional state at the time of the capture.

### Step Two: Delivery

For the second stage of the process we want to be able to identify any waiting customers and deliver the orders to them once it is ready.

Again we'll use the `Face Api` to identify existing customers and highlight their face on the captured image when their order is complete. For each identified face the `Face Api` supplies a bounding rectangle, so all we need to do is highlight a customers face when their order is being processed or is ready.

Once the order has been marked as being complete, the customers order will no longer be active and their face will no longer be recognised by the delivery system.

### The Setup

To capture images and manage the orders we're going to create a single app that can be deployed cross-platform using Xamarin.Forms. 

When taking orders we could easily use an tablet or phone with a camera so we can identify customers as they approach the till. 

For managing orders we'll use a different setup to capture a wider view of the room, so instead of using a tablet we can use a mini Windows 10 PC with a connected USB camera. This way the serving staff will be able to look at a monitor to identify where a customer is sitting within the room. 

This example is written in C# and uses a single code base across all aspects of the project (Mobile, Desktop and the server).

## Setup: Accounts & Subscriptions

To get started, you’ll need a Microsoft Azure account (don’t worry, it's free). If you don’t already have one, you can create a free Azure account [here](https://azure.microsoft.com/free/).

# Chapters

### Setup Services
- [CosmosDB](CosmosDB.md)
- [Azure Storage](AzureStorage.md)
- [Face Api](FaceApi.md)
- [Azure Functions](AzureFunctions.md)

### Configure The Solution
- [Overview](ProjectOverview.md)
- [Functions](ProjectFunctions.md)
- [Mobile](ProjectMobile.md)

# Summary
After following the guide you'll have set up your own customer recognition system that works on both Mobile and Desktop. The provided code isn't production ready, but it will give you a great idea on how to get started using the Cognitive Services, Azure Functions and Xamarin!

## Bonus Round 

Some ideas for furthering the example.

- Integrating with Twilio to notify the user when their order is ready for collection.
- Comparing any differences in the emotional state of the customer between ordering and picking up.

# Authors
- Clint Francis - [Github](https://github.com/clintfrancis)
