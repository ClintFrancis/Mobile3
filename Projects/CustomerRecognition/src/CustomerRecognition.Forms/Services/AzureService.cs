using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CustomerRecognition.Common;
using CustomerRecognition.Common.Message;
using Newtonsoft.Json;

namespace CustomerRecognition.Forms.Services
{
    public static class AzureService
    {
        public static async Task<CustomerDetailsResponse> GetCustomerDetails(string customerId)
        {
            var post = new CustomerDetailsRequest();
            post.CustomerId = customerId;

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await MakeRequest<CustomerDetailsResponse>(ProjectConfig.CustomerDetailsUrl, httpContent);

            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Posts a customer image for recognition.
        /// </summary>
        /// <returns>The image for recognition.</returns>
        /// <param name="image">Image.</param>
        public static async Task<IdentifyCustomerResponse> IdentifyCustomer(byte[] image)
        {
            var post = new IdentifyCustomerRequest();
            post.Image = Convert.ToBase64String(image);

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await MakeRequest<IdentifyCustomerResponse>(ProjectConfig.IdentifyCustomerUrl, httpContent);
                return result;

            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<CreateCustomerResponse> CreateCustomer(Customer customer, string imageUrl)
        {
            var post = new CreateCustomerRequest();
            post.Customer = customer;
            post.ImageUrl = imageUrl;

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await MakeRequest<CreateCustomerResponse>(ProjectConfig.CreateCustomerUrl, httpContent);

            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<CreateOrderResponse> CreateOrder(Customer customer, Order order)
        {
            var post = new CreateOrderRequest();
            post.Customer = customer;
            post.Order = order;

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await MakeRequest<CreateOrderResponse>(ProjectConfig.CreateOrderUrl, httpContent);
                return result;

            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<OrdersResponse> GetCurrentOrders()
        {
            var post = new OrdersRequest();

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await MakeRequest<OrdersResponse>(ProjectConfig.GetCurrentOrdersUrl, httpContent);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<IdentifyWaitingCustomersResponse> IdentifyWaitingCustomers(byte[] image)
        {
            var post = new IdentifyWaitingCustomersRequest();
            post.Image = Convert.ToBase64String(image);

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await MakeRequest<IdentifyWaitingCustomersResponse>(ProjectConfig.IdentifyWaitingCustomersUrl, httpContent, new JsonConverter[] { new RectangleJsonConverter() });
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<OrderStatusResponse> SetOrderStatus(string orderId, OrderStatus status)
        {
            var post = new OrderStatusRequest();
            post.OrderId = orderId;
            post.Status = status;

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await MakeRequest<OrderStatusResponse>(ProjectConfig.SetOrderStatusUrl, httpContent);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<ResetResponse> Reset(bool faceGroups, bool customers, bool orders, bool images)
        {
            var post = new ResetRequest();
            post.FaceGroups = faceGroups;
            post.Customers = customers;
            post.Orders = orders;
            post.Images = images;

            try
            {
                var json = JsonConvert.SerializeObject(post);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                return await MakeRequest<ResetResponse>(ProjectConfig.ResetGroupsUrl, httpContent);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }



        /// <summary>
        /// Makes the http requests.
        /// </summary>
        /// <returns>The request.</returns>
        /// <param name="url">URL.</param>
        /// <param name="httpContent">Http content.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        static async Task<T> MakeRequest<T>(string url, HttpContent httpContent, JsonConverter[] converters = null) where T : BaseResponse
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpResponse = await httpClient.PostAsync(url, httpContent);
                    if (httpResponse.Content != null)
                    {
                        var content = await httpResponse.Content.ReadAsStringAsync();
                        var identifyResult = JsonConvert.DeserializeObject<T>(content, converters);

                        return identifyResult;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return default(T);
        }
    }
}
