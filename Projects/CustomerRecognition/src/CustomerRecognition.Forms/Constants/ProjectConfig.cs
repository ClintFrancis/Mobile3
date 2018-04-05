using System;
namespace CustomerRecognition.Forms
{
    public static class ProjectConfig
    {
        static string BaseUrl = "https://FUNCTION_APP_NAME.azurewebsites.net/api/";
        //static string BaseUrl = "http://localhost:7071/api/"; 

        public static string CustomerDetailsUrl = BaseUrl + "CustomerDetails?code=YOUR_API_KEY_HERE";
        public static string IdentifyCustomerUrl = BaseUrl + "IdentifyCustomer?code=YOUR_API_KEY_HERE";
        public static string CreateCustomerUrl = BaseUrl + "CreateCustomer?code=YOUR_API_KEY_HERE";
        public static string CreateOrderUrl = BaseUrl + "CreateOrder?code=YOUR_API_KEY_HERE";
        public static string GetCurrentOrdersUrl = BaseUrl + "GetCurrentOrders?code=YOUR_API_KEY_HERE";
        public static string IdentifyWaitingCustomersUrl = BaseUrl + "IdentifyWaitingCustomers?code=YOUR_API_KEY_HERE";
        public static string ResetGroupsUrl = BaseUrl + "ResetStoredData?code=YOUR_API_KEY_HERE";
        public static string SetOrderStatusUrl = BaseUrl + "SetOrderStatus?code=YOUR_API_KEY_HERE";
    }
}
