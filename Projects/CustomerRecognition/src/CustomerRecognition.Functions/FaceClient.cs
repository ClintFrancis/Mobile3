using CustomerRecognition.Common;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Common.Contract;
using System.IO;
using System.Drawing;

namespace CustomerRecognition.Client
{
	public class FaceClient
	{
		private static FaceClient _instance;
		public static FaceClient Instance
		{
			get
			{
				_instance = _instance ?? new FaceClient();
				return _instance;
			}
		}

		const string LoyalCustomerGroup = "loyal_customers";
		const string AnonymousCustomerGroup = "anonymous_customers";

		string FaceApiKey = Environment.GetEnvironmentVariable("FaceApiKey");
		string FaceApiRegion = Environment.GetEnvironmentVariable("FaceApiRegion");

		FaceServiceClient Service;

		private FaceClient()
		{
			Service = new FaceServiceClient(FaceApiKey, FaceApiRegion);
		}

		public async Task<Face> DetectPrimaryFace(string faceURL, bool includeFaceAttributes)
		{
			// Capture the age, emotion and gender
			var attributes = includeFaceAttributes ? new FaceAttributeType[]
			{
				FaceAttributeType.Age,
				FaceAttributeType.Emotion,
				FaceAttributeType.Gender
			} : null;

			var results = await Service.DetectAsync(faceURL, returnFaceAttributes: attributes);
			return results?.OrderByDescending(i => i.FaceRectangle.Size()).FirstOrDefault();
		}

        public async Task<Face> DetectPrimaryFace(Stream imageStream, bool includeFaceAttributes)
        {
            // Capture the age, emotion and gender
            var attributes = includeFaceAttributes ? new FaceAttributeType[]
            {
                FaceAttributeType.Age,
                FaceAttributeType.Emotion,
                FaceAttributeType.Gender
            } : null;

            var results = await Service.DetectAsync(imageStream, returnFaceAttributes: attributes);
            return results?.OrderByDescending(i => i.FaceRectangle.Size()).FirstOrDefault();
        }

        public async Task<Identification[]> DetectCustomerFaces(Stream imageStream, bool includeFaceAttributes)
        {
            // Capture the age, emotion and gender
            var attributes = includeFaceAttributes ? new FaceAttributeType[]
            {
                FaceAttributeType.Emotion
            } : null;

            var faces = await Service.DetectAsync(imageStream, returnFaceAttributes: attributes);
            var faceIds = faces.Select(f => f.FaceId).ToArray();

            IdentifyResult[] matchedCustomers = await Service.IdentifyAsync(LoyalCustomerGroup, faceIds);

            var collated = (from face in faces
                                        from match in matchedCustomers
                                        where matchedCustomers.Any(o => face.FaceId == match.FaceId)
                                        select new {
                                            FaceId = face.FaceId,
                                            Rectangle = face.FaceRectangle,
                                            Emotion = face.FaceAttributes?.Emotion,
                                            Candidate = match.Candidates.Where(c => c.Confidence > .6).FirstOrDefault()
                                        }).ToList();

            var response = new List<Identification>();
            foreach (var result in collated)
            {
                var customer = CosmosClient.Instance.GetCustomerByPersonID(result.Candidate.PersonId);
                var orders = CosmosClient.Instance.GetCustomerOrders(customer.id);
                var faceRect = result.Rectangle;
                var ident = new Identification()
                {
                    Customer = customer,
                    Emotion = ParseEmotions(result.Emotion),
                    Orders = orders,
                    Rectangle = new Rectangle(faceRect.Left, faceRect.Top, faceRect.Width, faceRect.Height)
                };

                response.Add(ident);
            }

            return response.ToArray();
        }

        public async Task<Customer> RegisterNewCustomer(Customer customer, string photoUri)
		{
			customer = await RegisterCustomer(customer, photoUri, LoyalCustomerGroup);

			// Save into the database
			await CosmosClient.Instance.SaveCustomer(customer);

			return customer;
		}

		async Task<Customer> RegisterCustomer(Customer customer, string photoUri, string personGroupId)
		{
			try
			{
				CreatePersonResult personResult = await Service.CreatePersonAsync(personGroupId, customer.RegistrationName());
				customer.PersonId = personResult.PersonId;

				var faceResult = await Service.AddPersonFaceAsync(personGroupId, personResult.PersonId, photoUri);
				customer.FaceId = faceResult.PersistedFaceId;

				await Service.TrainPersonGroupAsync(personGroupId);
			}
			catch (FaceAPIException ex)
			{
				switch (ex.ErrorCode)
				{
					case "PersonGroupNotFound":
						break;
					case "FaceNotFound":
						break;
					case "PersonGroupNotTrained":
						break;
					case "BadArgument":
						break;
					default:
						break;
				}
				//errorDescription = "IdentifyFace: " + ex.ErrorCode;
				//currentLog.Info(errorDescription);
			}
			catch (Exception ex)
			{
				// Something bad
				throw ex;
			}

			return customer;
		}

		public async Task<Customer> AddCustomerFace(Customer customer, string photoUri)
		{
			var faceResult = await Service.AddPersonFaceAsync(LoyalCustomerGroup, customer.PersonId, photoUri);
			customer.FaceId = faceResult.PersistedFaceId;

			await Service.TrainPersonGroupAsync(LoyalCustomerGroup);

			return customer;
		}

		public async Task ResetFaceGroups()
		{
			try
			{
				var groups = await Service.ListPersonGroupsAsync();

				foreach (var group in groups)
				{
					await Service.DeletePersonGroupAsync(group.PersonGroupId);
				}

				await Service.CreatePersonGroupAsync(LoyalCustomerGroup, LoyalCustomerGroup);
				await Service.CreatePersonGroupAsync(AnonymousCustomerGroup, AnonymousCustomerGroup);
			}
			catch (FaceAPIException ex)
			{
				switch (ex.ErrorCode)
				{
					case "BadArgument":
						break;
					default:
						break;
				}
				//errorDescription = "IdentifyFace: " + ex.ErrorCode;
				//currentLog.Info(errorDescription);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        public async Task<Customer[]> IdentifyCustomers(Guid[] faceIds)
        {
            IdentifyResult[] results = await Service.IdentifyAsync(LoyalCustomerGroup, faceIds);

            // Confirm result
            if (results.Length > 0)
            {
                //var candidates = results
                //    .Where(f => f..ToString() == faceId.ToString())
                //    .Select(f => f.Candidates)
                //    .FirstOrDefault();

                //var topCandidate = candidates
                //    .OrderByDescending(c => c.Confidence)
                //    .FirstOrDefault();

                //if (topCandidate?.Confidence > .6)
                //    return CosmosClient.Instance.GetCustomerByPersonID(topCandidate.PersonId);
            }

            return null;
        }

		public async Task<Customer> IdentifyCustomerFace(Guid faceId)
		{
			try
			{
				IdentifyResult[] result = await Service.IdentifyAsync(LoyalCustomerGroup, new Guid[] { faceId });

				// Confirm result
				if (result.Length > 0)
				{
					var candidates = result
						.Where(f => f.FaceId.ToString() == faceId.ToString())
						.Select(f => f.Candidates)
						.FirstOrDefault();

					var topCandidate = candidates
						.OrderByDescending(c => c.Confidence)
						.FirstOrDefault();

					if (topCandidate?.Confidence > .6)
						return CosmosClient.Instance.GetCustomerByPersonID(topCandidate.PersonId);
				}

				// If NO customer, I need to create a new customer??

			}
			catch (FaceAPIException ex)
			{
				switch (ex.ErrorCode)
				{
					case "PersonGroupNotFound":
						break;
					case "FaceNotFound":
						break;
					case "PersonGroupNotTrained":
						break;
					case "BadArgument":
						break;
					default:
						break;
				}
				//errorDescription = "IdentifyFace: " + ex.ErrorCode;
				//currentLog.Info(errorDescription);
			}
			catch (Exception ex)
			{
				// Something bad
				throw ex;
			}

			return null;
		}

		public string ParseEmotions(EmotionScores scores)
		{
            if (scores == null)
                return null;
            
			return scores.ToRankedList().FirstOrDefault().Key;
		}
	}

	public static class FaceExtensionMethods
	{
		public static double Size(this FaceRectangle rect)
		{
			return rect.Width * rect.Height;
		}
	}
}
