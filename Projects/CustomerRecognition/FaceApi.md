# Cognitive Services Face API


## Definitions

First of all lets define a couple of terms used within the Face API.

### Face Detection
[Documentation](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395236)

Face detection is the action of locating faces in images. Users can upload an image or specify an image URL in the request. The detected faces are returned with face IDs indicating a unique identity in Face API. The rectangles indicate the locations of faces within the image measured in pixels, as well as the optional attributes for each face such as age, gender, head pose, facial hair and smiling.

### Face ID
A Face ID is derived from the detection results, in which a string represents a face in the Face API. Think of this as the Unique ID for a face within an image as there may be more than one face present at any given time.

### Person
[Documentation](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f3039523c)

A Person is a data structure managed in the Face API. A Person comes with a Person ID, as well as other attributes such as Name, a collection of Face IDs, and User Data.

### Person ID
Person ID is generated when a person is created successfully. A string is created to represent this person in the Face API.

### Person Group
[Documentation](https://westus.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395244)

A Person Group is a collection of Persons and is the unit of Identification. A Person Group comes with a Person Group ID, as well as other attributes such as Name and User Data.

## Some Frequently Asked Questions
#### Does the Facial Recognition API allow us to verify the user identity across multiple images?

Yes. When a photo is processed You are returned an array of identified faces each with their own unique Face ID. These Face ID's can be 'matched' against any existing Person ID's stored in your Person Groups. Similarly you can track a Face ID on its own across multiple images, however any Face ID will expire after 24 hours.
 
#### If a snapshot contains multiple facial images, when we analyse using Facial Recognition and Emotion API's, is this treated as a single call?

Each image submitted to the Face API is processed as a single call. A maximum of 64 faces can be returned for an image. The returned faces are ranked by face rectangle size in descending order. Support for face attributes includes age, gender, headPose, smile, facialHair, glasses, emotion, hair, makeup, occlusion, accessories, blur, exposure and noise. Note that each face attribute analysis has additional computational and time cost.

### Create a Face API Account
For this project we'll be need to create a new Face API account to get our unique API keys for the service.

##### 1. Navigate to Face API in the Azure Portal

/images/4_01_Face_API.png

##### 2. Create a Face API account  
Create a new account with a unique id, in this case I'll be using _'customerrecognitionfaceapi'_. Select the _'F0'_ pricing tier and select our existing customer recognition Resource Group, then create the Face API account.

/images/4_02_Face_API_Register.png

##### 3. Copy the API keys  

Once the service has been created, navigate to Resource Management > Keys and make a note of _Key 1_
/images/4_03_Face_API_Keys.png