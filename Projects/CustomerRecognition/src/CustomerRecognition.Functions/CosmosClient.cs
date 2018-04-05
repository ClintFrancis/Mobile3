using CustomerRecognition.Common;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerRecognition.Client
{
    class CosmosClient
    {
		private static CosmosClient _instance;
		public static CosmosClient Instance
		{
			get
			{
				_instance = _instance ?? new CosmosClient();
				return _instance;
			}
		}

		internal DocumentClient Client { get; private set; }
		public Uri CustomerCollectionUri { get; private set; }
		public Uri OrderCollectionUri { get; private set; }

		const string CUSTOMERS = "customers";
		const string ORDERS = "orders";

		private CosmosClient()
		{
			var endpoint = Environment.GetEnvironmentVariable("DocumentEndpointUri");
			var key = Environment.GetEnvironmentVariable("DocumentAuthKey");

			Client = new DocumentClient(new Uri(endpoint), key);
			CustomerCollectionUri = UriFactory.CreateDocumentCollectionUri(CUSTOMERS, CUSTOMERS);
			OrderCollectionUri = UriFactory.CreateDocumentCollectionUri(ORDERS, ORDERS);
		}

        public Customer GetCustomerByID(string id)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

            Customer customer = Client.CreateDocumentQuery<Customer>(CustomerCollectionUri, queryOptions)
                    .Where(f => f.id == id)
                    .Take(1).AsEnumerable().SingleOrDefault();

            return customer;
        }

        public Customer GetCustomerByPersonID(Guid personId)
		{
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

			Customer customer = Client.CreateDocumentQuery<Customer>(CustomerCollectionUri, queryOptions)
					.Where(f => f.PersonId == personId)
					.Take(1).AsEnumerable().SingleOrDefault();

			return customer;
		}

		public Order[] GetCustomerOrders(string customerId, int count = 5)
		{
			FeedOptions queryOptions = new FeedOptions { MaxItemCount = count };

			// Get up to five of a customers orders
			var orderQuery = Client.CreateDocumentQuery<Order>(OrderCollectionUri, queryOptions)
					.Where(f => f.CustomerId == customerId)
                    .OrderByDescending(d => d.Date)
					.Take(count)
					.AsEnumerable();

			return orderQuery.ToArray();
		}

        public Order[] GetCurrentOrders(int count = 100)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = count };

            // Get up to five of a customers orders
            var orderQuery = Client.CreateDocumentQuery<Order>(OrderCollectionUri, queryOptions)
                    .Where(f => f.Status == OrderStatus.Pending || f.Status == OrderStatus.Ready)
                    .OrderByDescending(d => d.Date)
                    .Take(count)
                    .AsEnumerable();
                    
            return orderQuery.ToArray();
        }

        public Order[] GetOrdersByStatus(OrderStatus status, int count = 100)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = count };

            // Get up to five of a customers orders
            var orderQuery = Client.CreateDocumentQuery<Order>(OrderCollectionUri, queryOptions)
                    .Where(f => f.Status == status)
                    .OrderByDescending(d => d.Date)
                    .Take(count)
                    .AsEnumerable();
                    
            return orderQuery.ToArray();
        }

        internal async Task<Customer> SaveCustomer(Customer customer)
		{
            var result = await Client.CreateDocumentAsync(CustomerCollectionUri, customer);
            customer.id = result.Resource.Id;
            return customer;
		}

		public async Task<Document> UpdateCustomer(Customer customer)
		{
			return await Client.UpsertDocumentAsync(CustomerCollectionUri, customer);
		}

		public async Task<Order> SaveOrder(Order order)
		{
            // If its new, then save it
            if (string.IsNullOrEmpty(order.id))
            {
                var collection = await Client.ReadDocumentCollectionAsync(OrderCollectionUri);
                var value = collection.CurrentResourceQuotaUsage;

                var totalDocuments = int.Parse(value.Split(';').Where(v => v.Contains("documentsCount")).FirstOrDefault().Split('=')[1]);
                order.OrderNumber = totalDocuments + 1;

                var result = await Client.CreateDocumentAsync(OrderCollectionUri, order);
                order.id = result.Resource.Id;
                return order;
            }

            // Otherwise return the existing id
            await Client.UpsertDocumentAsync(OrderCollectionUri, order);
            return order;
        }

        public async Task<bool> UpdateOrderStatus(string orderId, OrderStatus status)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

            var order = Client.CreateDocumentQuery<Order>(OrderCollectionUri, queryOptions)
                    .Where(f => f.id == orderId)
                    .Take(1)
                    .AsEnumerable()
                    .FirstOrDefault();

            if (order != null)
            {
                order.Status = status;
                await Client.UpsertDocumentAsync(OrderCollectionUri, order);

                return true;
            }

            return false;
        }

        public async Task ResetCustomers()
        {
            var documents = Client.CreateDocumentQuery(CustomerCollectionUri);

            foreach (var doc in documents)
            {
                await Client.DeleteDocumentAsync(doc.SelfLink);
            }
        }

        public async Task ResetOrders()
        {
            var documents = Client.CreateDocumentQuery(OrderCollectionUri);

            foreach (var doc in documents)
            {
                await Client.DeleteDocumentAsync(doc.SelfLink);
            }
        }
    }
}
