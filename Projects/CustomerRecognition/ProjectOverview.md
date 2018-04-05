# Project Overview



### Setup

Once all the Azure services have been set up correctly, we can begin to configure the app. There are two solutions that will need to be updated with variables specific to your Azure instance.

- [Functions](ProjectFunctions.md)
- [Mobile](ProjectMobile.md)


## Project Structure

- CustomerRecognition.Common
- CustomerRecognition.Droid
- CustomerRecognition.Forms
- CustomerRecognition.Functions
- CustomerRecognition.iOS
- CustomerRecognition.UWP
- MicrosoftProjectOxford.Face


### Common Code

The `CustomerRecognition.Common` solution contains all of the data classes that are shared between the projects.

- Customer
- Identification
- Order
- OrderStatus 
- Messages

These provide a way of making sure that all data types are the same and can easily be converted to JSON and back.

### Functions

All the code specific to the Functions is stored in this solution. 

### Forms, Droid, iOS and UWP

These solutions make up the Xamarin.Forms code base for the App. The `CustomerRecognition.Forms` solution contains all the common code for the application (Navigation, Models, Services etc). Each of the other solutions contains platform specific code for controlling the cameras and publishing.

Click [here](https://www.xamarin.com/forms) to find out more about Xamarin.Forms.

## Using the App

1. Click on the **New** table and take a photo of a customer
2. Add their name, order and cost of purchase, then submit the order.
3. Check the order is listed in the **Orders** view.
4. Click the **Identify** tab and check the camera is able to identify the customer (The default timer interval is 5secs between captures)
5. The customer will have a red border showing that their order is currently pending.
6. Go to the **Orders** tab and change the status of the order to 'Ready'.
7. Return to the **Identify** tab and the customers face will now be highlighted in green
8. If a customer has one order ready and another order pending the outline will be shown in yellow.
9. Marking the order as complete will stop the user from being identified again until they have a new order (pending or ready) in the system.