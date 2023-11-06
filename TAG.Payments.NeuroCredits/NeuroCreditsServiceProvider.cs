using Paiwise;
using System;
using System.Threading.Tasks;
using TAG.Payments.NeuroCredits.Configuration;
using Waher.IoTGateway;
using Waher.Persistence;
using Waher.Runtime.Inventory;

namespace TAG.Payments.NeuroCredits
{
    /// <summary>
    /// Serivce provider for Neuro-Credits™
    /// </summary>
    public class NeuroCreditsServiceProvider : IConfigurableModule, IBuyEDalerServiceProvider, ISellEDalerServiceProvider
	{
		/// <summary>
		/// Serivce provider for Neuro-Credits™
		/// </summary>
		public NeuroCreditsServiceProvider()
		{
		}

		#region IModule

		/// <summary>
		/// Starts the service module.
		/// </summary>
		public Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the service module.
		/// </summary>
		public Task Stop()
		{
			return Task.CompletedTask;
		}

		#endregion

		#region IConfigurableModule

		/// <summary>
		/// Gets available configuration pages for the service.
		/// </summary>
		/// <returns>Configuration pages.</returns>
		public Task<IConfigurablePage[]> GetConfigurablePages()
		{
			return Task.FromResult(new IConfigurablePage[]
			{
				new ConfigurablePage("Neuro-Credits™", "/NeuroCredits/Settings.md", "Admin.Payments.NeuroCredits")
			}); ;
		}

		#endregion

		#region IServiceProvider

		/// <summary>
		/// ID of service
		/// </summary>
		public string Id => ServiceId;

		/// <summary>
		/// ID of service.
		/// </summary>
		public static string ServiceId = typeof(NeuroCreditsServiceProvider).Namespace;

		/// <summary>
		/// Name of service
		/// </summary>
		public string Name => "Neuro-Credits™";

		/// <summary>
		/// Icon URL
		/// </summary>
		public string IconUrl => Gateway.GetUrl("/NeuroCredits/Logo.png");

		/// <summary>
		/// Width of icon, in pixels.
		/// </summary>
		public int IconWidth => 452;

		/// <summary>
		/// Height of icon, in pixels
		/// </summary>
		public int IconHeight => 94;

		#endregion

		#region IBuyEDalerServiceProvider

		/// <summary>
		/// Gets available payment services.
		/// </summary>
		/// <param name="Currency">Currency to use.</param>
		/// <param name="Country">Country where service is to be used.</param>
		/// <returns>Available payment services.</returns>
		public async Task<IBuyEDalerService[]> GetServicesForBuyingEDaler(CaseInsensitiveString Currency, CaseInsensitiveString Country)
		{
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();

			if (ServiceConfiguration.IsWellDefined && BillingConfiguration.IsWellDefined)
			{
				return new IBuyEDalerService[]
				{
					new NeuroCreditsService(this)
				};
			}
			else
				return new IBuyEDalerService[0];
		}

		/// <summary>
		/// Gets a payment service.
		/// </summary>
		/// <param name="ServiceId">Service ID</param>
		/// <param name="Currency">Currency to use.</param>
		/// <param name="Country">Country where service is to be used.</param>
		/// <returns>Service, if found, null otherwise.</returns>
		public Task<IBuyEDalerService> GetServiceForBuyingEDaler(string ServiceId, CaseInsensitiveString Currency, CaseInsensitiveString Country)
		{
			Type T = Types.GetType(ServiceId);
			if (T is null)
				return Task.FromResult<IBuyEDalerService>(null);

			if (T.Assembly != typeof(NeuroCreditsServiceProvider).Assembly)
				return Task.FromResult<IBuyEDalerService>(null);

			if (!(Types.Instantiate(T, this) is IBuyEDalerService Service))
				return Task.FromResult<IBuyEDalerService>(null);

			return Task.FromResult(Service);
		}

		#endregion

		#region ISellEDalerServiceProvider

		/// <summary>
		/// Gets available payment services.
		/// </summary>
		/// <param name="Currency">Currency to use.</param>
		/// <param name="Country">Country where service is to be used.</param>
		/// <returns>Available payment services.</returns>
		public async Task<ISellEDalerService[]> GetServicesForSellingEDaler(CaseInsensitiveString Currency, CaseInsensitiveString Country)
		{
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();

			if (ServiceConfiguration.IsWellDefined)
			{
				return new ISellEDalerService[]
				{
					new NeuroCreditsService(this)
				};
			}
			else
				return new ISellEDalerService[0];
		}

		/// <summary>
		/// Gets a payment service.
		/// </summary>
		/// <param name="ServiceId">Service ID</param>
		/// <param name="Currency">Currency to use.</param>
		/// <param name="Country">Country where service is to be used.</param>
		/// <returns>Service, if found, null otherwise.</returns>
		public Task<ISellEDalerService> GetServiceForSellingEDaler(string ServiceId, CaseInsensitiveString Currency, CaseInsensitiveString Country)
		{
			Type T = Types.GetType(ServiceId);
			if (T is null)
				return Task.FromResult<ISellEDalerService>(null);

			if (T.Assembly != typeof(NeuroCreditsServiceProvider).Assembly)
				return Task.FromResult<ISellEDalerService>(null);

			if (!(Types.Instantiate(T, this) is ISellEDalerService Service))
				return Task.FromResult<ISellEDalerService>(null);

			return Task.FromResult(Service);
		}

		#endregion
	}
}
