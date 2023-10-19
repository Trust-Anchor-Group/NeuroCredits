using Paiwise;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TAG.Payments.NeuroCredits.Data;
using Waher.IoTGateway;
using Waher.Networking.XMPP;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Counters;
using Waher.Runtime.Inventory;

namespace TAG.Payments.NeuroCredits
{
	/// <summary>
	/// Serivce for Neuro-Credits™
	/// </summary>
	public class NeuroCreditsService : IBuyEDalerService, ISellEDalerService
	{
		private const string buyEDalerTemplateId = "2cc2dc30-7a90-4ade-8c1e-cdd642e60077@legal.lab.tagroot.io";
		private const string sellEDalerTemplateId = "2cc2dc49-7a90-4ae5-8c1e-cdd642aa520d@legal.lab.tagroot.io";

		private readonly NeuroCreditsServiceProvider provider;

		/// <summary>
		/// Serivce for Neuro-Credits™
		/// </summary>
		/// <param name="Provider">Reference to the service provider.</param>
		public NeuroCreditsService(NeuroCreditsServiceProvider Provider)
		{
			this.provider = Provider;
		}

		#region IServiceProvider

		/// <summary>
		/// ID of service
		/// </summary>
		public string Id => ServiceId;

		/// <summary>
		/// ID of service.
		/// </summary>
		public static string ServiceId = typeof(NeuroCreditsService).Namespace;

		/// <summary>
		/// Name of service
		/// </summary>
		public string Name => "Neuro-Credits™";

		/// <summary>
		/// Icon URL
		/// </summary>
		public string IconUrl => string.Empty;

		/// <summary>
		/// Width of icon, in pixels.
		/// </summary>
		public int IconWidth => 0;

		/// <summary>
		/// Height of icon, in pixels
		/// </summary>
		public int IconHeight => 0;

		#endregion

		#region IProcessingSupport<CaseInsensitiveString>

		/// <summary>
		/// How well a currency is supported
		/// </summary>
		/// <param name="Currency">Currency</param>
		/// <returns>Support</returns>
		public Grade Supports(CaseInsensitiveString Currency)
		{
			return SupportsCurrency(Currency).Result ? Grade.Ok : Grade.NotAtAll;
		}

		private static async Task<bool> SupportsCurrency(CaseInsensitiveString Currency)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			return Configuration.SupportsCurrency(Currency);
		}

		#endregion

		#region IBuyEDalerService

		/// <summary>
		/// Contract ID of Template, for buying e-Daler
		/// </summary>
		public string BuyEDalerTemplateContractId => buyEDalerTemplateId;

		/// <summary>
		/// Reference to the service provider.
		/// </summary>
		public IBuyEDalerServiceProvider BuyEDalerServiceProvider => this.provider;

		/// <summary>
		/// If the service provider can be used to process a request to buy eDaler®
		/// of a certain amount, for a given account.
		/// </summary>
		/// <param name="AccountName">Account Name</param>
		/// <returns>If service provider can be used.</returns>
		public async Task<bool> CanBuyEDaler(CaseInsensitiveString AccountName)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return false;

			// ID:=select generic top 1 * from "LegalIdentities" where Account=AccountName and State="Approved" order by Created desc;

			GenericObject ID = await GetLegalID(AccountName);
			if (ID is null)
				return false;

			PersonalInformation PI = new PersonalInformation(ID);
			string WalletCurrency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);

			return await MaxCreditAmountAuthorized(PI, Configuration, WalletCurrency) > 0;
		}

		private static async Task<GenericObject> GetLegalID(CaseInsensitiveString AccountName)
		{
			// ID:=select generic top 1 * from "LegalIdentities" where Account=AccountName and State="Approved" order by Created desc;

			foreach (GenericObject Obj in await Database.Find<GenericObject>("LegalIdentities", 0, 1, new FilterAnd(
				new FilterFieldEqualTo("Account", AccountName),
				new FilterFieldEqualTo("State", "Approved")), "-Created"))
			{
				return Obj;
			}

			return null;
		}

		/// <summary>
		/// If the service provider can be used to process a request to buy eDaler®
		/// of a certain amount, for a given account.
		/// </summary>
		/// <param name="PI">Personal Information about account.</param>
		/// <param name="Configuration">Current configuration.</param>
		/// <param name="Currency">Currency</param>
		/// <returns>Amount of Neuro-Credits™ authorized.</returns>
		internal static async Task<double> MaxCreditAmountAuthorized(PersonalInformation PI, ServiceConfiguration Configuration, string Currency)
		{
			double MaxAmount;

			if (!PI.HasPersonalBillingInformation)
				return 0;

			if (Configuration.AllowPrivatePersons)
			{
				MaxAmount = await Configuration.IsPersonAuthorized(PI.Jid, PI.PersonalNumber, Currency);
				if (MaxAmount > 0)
					return MaxAmount;
			}

			if (!PI.HasOrganizationalBillingInformation)
				return 0;

			if (Configuration.AllowOrganizations)
			{
				MaxAmount = await Configuration.IsOrganizationAuthorized(PI.Jid, PI.OrganizationNumber, PI.PersonalNumber, Currency);
				if (MaxAmount > 0)
					return MaxAmount;
			}

			return 0;
		}

		/// <summary>
		/// Processes payment for buying eDaler®.
		/// </summary>
		/// <param name="ContractParameters">Parameters available in the
		/// contract authorizing the payment.</param>
		/// <param name="IdentityProperties">Properties engraved into the
		/// legal identity signing the payment request.</param>
		/// <param name="Amount">Amount to be paid.</param>
		/// <param name="Currency">Currency</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service
		/// requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Result of operation.</returns>
		public async Task<PaymentResult> BuyEDaler(IDictionary<CaseInsensitiveString, object> ContractParameters,
			IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties,
			decimal Amount, string Currency, string SuccessUrl, string FailureUrl, string CancelUrl, ClientUrlEventHandler ClientUrlCallback, object State)
		{
			if (Amount != Math.Floor(Amount))
				return new PaymentResult("Only whole amounts of Neuro-Credits™ permitted.");

			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return new PaymentResult("Neuro-Credits™ service not configured properly.");

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			double MaxAmount = await MaxCreditAmountAuthorized(PI, Configuration, Currency);

			if (MaxAmount <= 0)
				return new PaymentResult("Not authorized to buy Neuro-Credits™. Contact your operator to receive authorization to buy Neuro-Credits™. If there are outstanding payments, you might need to cleared those first.");

			if ((double)Amount > MaxAmount)
				return new PaymentResult("Amount exceeds maximum allowed amount.");

			Amount = await ServiceConfiguration.IncrementPersonalDebt(Amount, PI.Jid, PI.PersonalNumber);

			if (!ContractParameters.TryGetValue("DueDate", out object Obj) || !(Obj is DateTime DueDate))
				DueDate = DateTime.Today.AddDays(30);

			if (!ContractParameters.TryGetValue("ContractId", out Obj) || !(Obj is string ContractId))
				ContractId = null;

			Invoice Invoice = new Invoice()
			{
				InvoiceNumber = await RuntimeCounters.IncrementCounter(NeuroCreditsServiceProvider.ServiceId + ".NrInvoices"),
				Account = XmppClient.GetAccount(PI.Jid),
				IsPaid = false,
				Amount = Amount,
				Currency = Currency,
				DueDate = DueDate,
				Created = DateTime.UtcNow,
				Paid = DateTime.MinValue,
				NeuroCreditsContractId = ContractId,
				CancellationContractId = null,
				ExternalReference = null,
				FirstName = PI.FirstName,
				MiddleName = PI.MiddleName,
				LastName = PI.LastName,
				PersonalNumber = PI.PersonalNumber,
				Address = PI.Address,
				Address2 = PI.Address2,
				Area = PI.Area,
				City = PI.City,
				PostalCode = PI.PostalCode,
				Region = PI.Region,
				Country = PI.Country,
				Jid = PI.Jid,
				PhoneNumber = PI.PhoneNumber,
				OrganizationName = PI.OrganizationName,
				Department = PI.Department,
				Role = PI.Role,
				OrganizationNumber = PI.OrganizationNumber,
				OrganizationAddress = PI.OrganizationAddress,
				OrganizationAddress2 = PI.OrganizationAddress2,
				OrganizationArea = PI.OrganizationArea,
				OrganizationCity = PI.OrganizationCity,
				OrganizationPostalCode = PI.OrganizationPostalCode,
				OrganizationRegion = PI.OrganizationRegion,
				OrganizationCountry = PI.OrganizationCountry
			};

			await Database.Insert(Invoice);

			return new PaymentResult(Amount, Currency);
		}

		/// <summary>
		/// Gets available payment options for buying eDaler®.
		/// </summary>
		/// <param name="IdentityProperties">Properties engraved into the legal identity that will performm the request.</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service
		/// requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Array of dictionaries, each dictionary representing a set of parameters that can be selected in the
		/// contract to sign.</returns>
		public async Task<IDictionary<CaseInsensitiveString, object>[]> GetPaymentOptionsForBuyingEDaler(
			IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties,
			string SuccessUrl, string FailureUrl, string CancelUrl, ClientUrlEventHandler ClientUrlCallback, object State)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return new IDictionary<CaseInsensitiveString, object>[0];

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			string AccountName = XmppClient.GetAccount(PI.Jid);
			string Domain = XmppClient.GetDomain(PI.Jid);
			if (!Gateway.IsDomain(Domain, true))
				return new IDictionary<CaseInsensitiveString, object>[0];

			string WalletCurrency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);
			double MaxAmount = await MaxCreditAmountAuthorized(PI, Configuration, WalletCurrency);

			return new IDictionary<CaseInsensitiveString, object>[]
			{
				new Dictionary<CaseInsensitiveString, object>()
				{
					{ "Max(Amount)", MaxAmount }
				}
			};
		}

		#endregion

		#region ISellEDalerService

		/// <summary>
		/// Contract ID of Template, for selling e-Daler
		/// </summary>
		public string SellEDalerTemplateContractId => sellEDalerTemplateId;

		/// <summary>
		/// Reference to the service provider.
		/// </summary>
		public ISellEDalerServiceProvider SellEDalerServiceProvider => this.provider;

		/// <summary>
		/// If the service provider can be used to process a request to buy eDaler®
		/// of a certain amount, for a given account.
		/// </summary>
		/// <param name="AccountName">Account Name</param>
		/// <returns>If service provider can be used.</returns>
		public async Task<bool> CanSellEDaler(CaseInsensitiveString AccountName)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return false;

			// ID:=select generic top 1 * from "LegalIdentities" where Account=AccountName and State="Approved" order by Created desc;

			GenericObject ID = await GetLegalID(AccountName);
			if (ID is null)
				return false;


		}

		#endregion

	}
}
