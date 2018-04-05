using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerRecognition.Client
{
    public class StorageClient
    {
		private static StorageClient _instance;
		public static StorageClient Instance
		{
			get
			{
				_instance = _instance ?? new StorageClient();
				return _instance;
			}
		}

		string storageConnection = Environment.GetEnvironmentVariable("StorageConnectionString");
		CloudStorageAccount storageAccount;
		CloudBlobClient blobClient;

		private StorageClient()
		{
			storageAccount = CloudStorageAccount.Parse(storageConnection);
			blobClient = storageAccount.CreateCloudBlobClient();
		}

		public Uri StoreImage(byte[] image, string containerName)
		{
			// Retrieve a reference to a container.
			CloudBlobContainer container = blobClient.GetContainerReference(containerName);

			string fileName = Guid.NewGuid().ToString() + ".png";
			var blockBlob = container.GetBlockBlobReference(fileName);

			// Use a stream to upload
			using (var stream = new MemoryStream(image))
			{
				blockBlob.UploadFromStream(stream, stream.Length);
			}

			return blockBlob.Uri;
		}

        public async Task ResetContainer(string containerName)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            var contents = container.ListBlobs();
            foreach (var blob in contents)
            {
                CloudBlob cloudBlob = container.GetBlobReference(blob.Uri.ToString());
                cloudBlob.DeleteIfExists();
            }
        }
	}
}
