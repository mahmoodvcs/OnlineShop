using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Infrustructure.Extensions;
using MahtaKala.Models.StaffModels;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
	public class EskadServiceHttpClient
	{
		private readonly ICurrentUserService currentUserService;
		private readonly ILogger<EskadServiceHttpClient> logger;
		private readonly DataContext dbContext;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IConfiguration configuration;
		public const long ESKAAD_SELLER_ID = 54;
		public const int MAHTA_STORAGE_NUMBER_IN_ESKAAD = 13;
		public static readonly string[] ESKAAD_AUTHORIZED_USERS = { "katouzian", "mosalli", "ali.d" };

		//public const string ESKAD_COMMUNICATOR_API_ADDRESS = "http://localhost:55742/api/EskadService/";//"host90:8088/api/EskadService/";
		private readonly string eskadCommunicationApiAddress;

		User User => currentUserService.User;

		public EskadServiceHttpClient(ICurrentUserService currentUserService
							, ILogger<EskadServiceHttpClient> logger
							, DataContext dbContext
							, IHttpClientFactory clientFactory
							, IConfiguration configuration)
		{
			this.currentUserService = currentUserService;
			this.logger = logger;
			this.dbContext = dbContext;
			httpClientFactory = clientFactory;
			this.configuration = configuration;
			eskadCommunicationApiAddress = configuration.GetSection("AppSettings")["EskadCommunicationApiAddress"];
		}

		private HttpClient GetHttpClient(string token)
		{
			var httpClient = httpClientFactory.CreateClient();
			httpClient.BaseAddress = new Uri(eskadCommunicationApiAddress);
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
			return httpClient;
		}

		public async Task<IQueryable<Merchandise>> CallGetEskadMerchandiseData(string token)//, bool onlyActive = true, bool removeIfNotInStock = true)
		{
			try
			{
				var httpClient = GetHttpClient(token);
				var response = await httpClient.GetAsync("GetEskadMerchandiseData");
				response.EnsureSuccessStatusCode();
				using var responseStream = await response.Content.ReadAsStreamAsync();
				var merchandise = await JsonSerializer.DeserializeAsync<IEnumerable<Merchandise>>(responseStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
				return merchandise.AsQueryable();
			}
			catch (Exception e)
			{
				var wholeMessage = e.DigOutWholeExceptionMessage();
				logger.LogError(wholeMessage);
				throw e;
			}
		}

		public async Task<IQueryable<EskaadOrderDraft>> CallGetEskaadOrderDrafts(string token)
		{
			var httpClient = GetHttpClient(token);
			var response = await httpClient.GetAsync("GetEskadOrderDraftData");
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			var orderDrafts = await JsonSerializer.DeserializeAsync<IEnumerable<EskaadOrderDraft>>(responseStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
			return orderDrafts.AsQueryable();
		}

		public async Task<bool> TodaysOrdersAreSealed()
		{
			var now = DateTime.Now;
			var ordersAreSealed = await dbContext.EskaadOrderDrafts.Where(x =>
					x.CreatedDate.Date.Equals(now.Date) && x.OrderIsSealed)
				.AnyAsync();
			return ordersAreSealed;
		}

		public async Task<(bool, string)> CallAddNewOrderItem(string token, string code, int quantity)
		{
			var httpClient = GetHttpClient(token);

			var response = await httpClient.PostAsync($"AddNewOrderItem?merchandiseCode={code}&quantity={quantity}", null);
			response.EnsureSuccessStatusCode();
			var responseString = await response.Content.ReadAsStringAsync();
			var definition = new { success = true, message = "" };
			var responseModel = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(responseString, definition);
			return (responseModel.success, responseModel.message);
		}


		public async Task<(bool, string)> CallDeleteOrderDraftItem(string token, long draftItemId)
		{
			var httpClient = GetHttpClient(token);

			var response = await httpClient.PostAsync($"DeleteOrderDraftItem?id={draftItemId}", null);
			response.EnsureSuccessStatusCode();
			var responseString = await response.Content.ReadAsStringAsync();
			var definition = new { success = true, message = "" };
			var responseAnonymousModel = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(responseString, definition);
			return (responseAnonymousModel.success, responseAnonymousModel.message);
		}

		public async Task<(bool, string)> CallPlaceOrdersForTodayOnEskad(string token)
		{
			try
			{
				var httpClient = GetHttpClient(token);

				var response = await httpClient.PostAsync("PlaceEskaadOrdersForToday", null);
				response.EnsureSuccessStatusCode();
				var responseString = await response.Content.ReadAsStringAsync();
				var definintion = new { success = true, message = "" };
				var responseAnonymousModel = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(responseString, definintion);
				return (responseAnonymousModel.success, responseAnonymousModel.message);
			}
			catch (Exception e)
			{
				var wholeMessage = e.DigOutWholeExceptionMessage();
				logger.LogError(wholeMessage);
				throw e;
			}
			
		}

		public async Task<IQueryable<EskaadSalesModel>> CallGetEskadSalesData(string token, string dateFilter = "")
		{
			try
			{
				var httpClient = GetHttpClient(token);
				var response = await httpClient.GetAsync("GetEskaadSalesData");
				response.EnsureSuccessStatusCode();
				using var responseStream = await response.Content.ReadAsStreamAsync();
				var eskadSalesList = await JsonSerializer.DeserializeAsync<IEnumerable<EskaadSalesModel>>(responseStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
				return eskadSalesList.AsQueryable();
			}
			catch (Exception e)
			{
				var wholeMessage = e.DigOutWholeExceptionMessage();
				logger.LogError(wholeMessage);
				throw e;
			}
		}
	}
}
