using CustomerRecognition.Client;
using CustomerRecognition.Common;
using CustomerRecognition.Common.Message;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CustomerRecognition.Functions
{
    public static class CustomerFunctions
    {
        static string ImagesContainerID = "faces";

        [FunctionName("CustomerDetails")]
        public static async Task<HttpResponseMessage> CustomerDetails([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<CustomerDetailsRequest>(body);
            if (request == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");

            var response = new CustomerDetailsResponse();
            response.Customer = CosmosClient.Instance.GetCustomerByID(request.CustomerId);
            if (request.OrderHistoryTotal > 0)
                response.OrderHistory = CosmosClient.Instance.GetCustomerOrders(request.CustomerId, request.OrderHistoryTotal);

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("IdentifyCustomer")]
        public static async Task<HttpResponseMessage> IdentifyCustomer([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var imageCapture = JsonConvert.DeserializeObject<IdentifyCustomerRequest>(body, new JsonConverter[] { new GuidJsonConverter() });
            if (imageCapture == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");

            // Decode data from Base64
            byte[] imageBytes = Convert.FromBase64String(imageCapture.Image);

            // Detect the faces
            Face detectedFace;
            using (var stream = new MemoryStream(imageBytes))
            {
                detectedFace = await FaceClient.Instance.DetectPrimaryFace(stream, true);

                if (detectedFace == null || detectedFace.FaceId == null)
                    req.CreateResponse(HttpStatusCode.ExpectationFailed, "Error: No face was detected");
            }

            // Crop the face
            var rect = new Rectangle(
                         detectedFace.FaceRectangle.Left,
                         detectedFace.FaceRectangle.Top,
                         detectedFace.FaceRectangle.Width,
                         detectedFace.FaceRectangle.Height
                    );
            rect.Inflate(50, 50);

            using (var stream = new MemoryStream(imageBytes))
            {
                Bitmap bmp = Image.FromStream(stream) as Bitmap;
                var cropped = ImageUtils.CropBitmap(bmp, rect);
                imageBytes = cropped.ToByteArray(ImageFormat.Png);
            }

            // Store the image
            string storedUrl = StorageClient.Instance.StoreImage(imageBytes, ImagesContainerID)?.ToString();

            log.Trace(new TraceEvent(System.Diagnostics.TraceLevel.Verbose, "heres the url!: " + storedUrl));

            var response = new IdentifyCustomerResponse()
            {
                ImageUrl = storedUrl,
                Emotion = FaceClient.Instance.ParseEmotions(detectedFace.FaceAttributes.Emotion)
            };

            // Check for existing Customers
            response.Customer = await FaceClient.Instance.IdentifyCustomerFace(detectedFace.FaceId);

            // Update the customers history with the recent photo
            if (response.Customer != null)
            {
                await FaceClient.Instance.AddCustomerFace(response.Customer, storedUrl);

                // Get their previous order
                var orders = CosmosClient.Instance.GetCustomerOrders(response.Customer.id, 1); // Could show more if we wanted
                response.PreviousOrder = orders.FirstOrDefault();
            }

            // If there are no matched customers return a new Customer with the FaceID
            else
            {
                response.Customer = new Customer()
                {
                    FaceId = detectedFace.FaceId,
                    Age = detectedFace.FaceAttributes.Age,
                    Gender = detectedFace.FaceAttributes.Gender
                };
            }

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("CreateCustomer")]
        public static async Task<HttpResponseMessage> CreateCustomer([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<CreateCustomerRequest>(body, new JsonConverter[] { new GuidJsonConverter() });
            if (request == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");

            Customer customer = request.Customer;

            // Create the response
            var response = new CreateCustomerResponse();

            // If we're not provided a valid First Name throw an error
            if (string.IsNullOrEmpty(customer.FirstName))
            {
                response.Message = "No FirstName was provided";
                return req.CreateResponse(HttpStatusCode.ExpectationFailed, response);
            }

            // Check for existing customers
            Customer exitingCustomer = await FaceClient.Instance.IdentifyCustomerFace(customer.FaceId);
            if (exitingCustomer != null)
            {
                response.Message = "Customer already exists";
                response.Customer = exitingCustomer;
                return req.CreateResponse(HttpStatusCode.Conflict, response);
            }

            // If not register a new Person and store in the database
            customer = await FaceClient.Instance.RegisterNewCustomer(customer, request.ImageUrl);

            response.Customer = customer;

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("CreateOrder")]
        public static async Task<HttpResponseMessage> CreateOrder([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<CreateOrderRequest>(body, new JsonConverter[] { new GuidJsonConverter() });
            if (request == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");

            Customer customer = request.Customer;

            var response = new CreateOrderResponse();

            // Register the customer if they dont exist yet
            if (customer.Anonymous || string.IsNullOrEmpty(customer.id))
            {
                // If we're not provided a valid First Name throw an error
                if (string.IsNullOrEmpty(customer.FirstName))
                {
                    response.Message = "No FirstName was provided";
                    return req.CreateResponse(HttpStatusCode.ExpectationFailed, response);
                }

                // If we're not provided a valid ImageUrl throw an error
                if (string.IsNullOrEmpty(request.Order.CustomerImageUrl))
                {
                    response.Message = "No ImageUrl was provided";
                    return req.CreateResponse(HttpStatusCode.ExpectationFailed, response);
                }

                // Check for existing customers
                Customer exitingCustomer = await FaceClient.Instance.IdentifyCustomerFace(customer.FaceId);
                if (exitingCustomer == null)
                    customer = await FaceClient.Instance.RegisterNewCustomer(customer, request.Order.CustomerImageUrl);

                else
                    customer.id = exitingCustomer.id;
            }

            // Store the order in the system
            request.Order.CustomerId = customer.id;

            response.Order = await CosmosClient.Instance.SaveOrder(request.Order);
            response.Message = "Order Created";

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("SetOrderStatus")]
        public static async Task<HttpResponseMessage> SetOrderStatus([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<OrderStatusRequest>(body);
            if (request == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");
            }

            var result = await CosmosClient.Instance.UpdateOrderStatus(request.OrderId, request.Status);
            var response = new OrderStatusResponse();

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("GetCurrentOrders")]
        public static async Task<HttpResponseMessage> GetCurrentOrders([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var response = new OrdersResponse();
            response.Orders = CosmosClient.Instance.GetCurrentOrders();

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("IdentifyWaitingCustomers")]
        public static async Task<HttpResponseMessage> IdentifyWaitingCustomers([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var imageCapture = JsonConvert.DeserializeObject<IdentifyWaitingCustomersRequest>(body, new JsonConverter[] { new GuidJsonConverter() });
            if (imageCapture == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");

            // Decode data from Base64
            var imageBytes = Convert.FromBase64String(imageCapture.Image); //ImageUtils.ConvertImage(imageCapture.Image);
            Stream stream = new MemoryStream(imageBytes); // TODO put this in a using

            // Detect the customer faces
            var detectedFaces = await FaceClient.Instance.DetectCustomerFaces(stream, true);
            var response = new IdentifyWaitingCustomersResponse();
            response.Results = detectedFaces;

            var json = JsonConvert.SerializeObject(response, Formatting.Indented, new JsonConverter[] { new RectangleJsonConverter() });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        [FunctionName("ResetStoredData")]
        public static async Task<HttpResponseMessage> ResetStoredData([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get request body
            var body = await req.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<ResetRequest>(body);
            if (request == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "No Valid Data submitted.");

            if (request.FaceGroups) await FaceClient.Instance.ResetFaceGroups();
            if (request.Customers) await CosmosClient.Instance.ResetCustomers();
            if (request.Orders) await CosmosClient.Instance.ResetOrders();
            if (request.Images) await StorageClient.Instance.ResetContainer(ImagesContainerID);

            var response = new ResetResponse();
            response.Message = "Data has been reset.";

            return req.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
