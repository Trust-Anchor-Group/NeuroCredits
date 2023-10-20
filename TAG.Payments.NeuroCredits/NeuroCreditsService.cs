using Paiwise;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TAG.Payments.NeuroCredits.Data;
using Waher.Content;
using Waher.Events;
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
		private const string buyEDalerTemplateId = "2cc43dd6-4395-efdd-80d1-60ba31011f89@legal.";
		private const string sellEDalerTemplateId = "2cc43bab-749d-bf32-a82f-6ff8340c0860@legal.";
		//private const string buyEDalerTemplateId = "2cc53e98-7a90-b7a9-8c1e-cdd64219452d@legal.lab.tagroot.io";
		//private const string sellEDalerTemplateId = "2cc53ea8-7a90-b7af-8c1e-cdd6420a125f@legal.lab.tagroot.io";

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
		public static string ServiceId = typeof(NeuroCreditsService).FullName;

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
				MaxAmount = await Configuration.IsPersonAuthorized(PI.Jid, PI.PersonalNumber, PI.Country, Currency);
				if (MaxAmount > 0)
					return MaxAmount;
			}

			if (!PI.HasOrganizationalBillingInformation)
				return 0;

			if (Configuration.AllowOrganizations)
			{
				MaxAmount = await Configuration.IsOrganizationAuthorized(PI.Jid, PI.OrganizationNumber, PI.OrganizationCountry,
					PI.PersonalNumber, PI.Country, Currency);

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

			Amount = await ServiceConfiguration.IncrementPersonalDebt(Amount, PI.Jid, PI.PersonalNumber, PI.Country);

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
				DueInterest = Configuration.DueInterest,
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

			Log.Notice("Neuro-Credits™ bought.", Invoice.InvoiceNumber.ToString(), Invoice.Account, "InvoiceCreated", Invoice.GetTags());

			return new PaymentResult(Amount, Currency);
		}

		/// <summary>
		/// Gets available payment options for buying eDaler®.
		/// </summary>
		/// <param name="IdentityProperties">Properties engraved into the legal identity that will perform the request.</param>
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
			if (!string.IsNullOrEmpty(Domain) && !Gateway.IsDomain(Domain, true))
				return new IDictionary<CaseInsensitiveString, object>[0];

			string WalletCurrency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);
			double MaxAmount = await MaxCreditAmountAuthorized(PI, Configuration, WalletCurrency);

			return new IDictionary<CaseInsensitiveString, object>[]
			{
				new Dictionary<CaseInsensitiveString, object>()
				{
					{ "Max(Amount)", MaxAmount },
					{ "Min(DueDays)", Configuration.DueDays },
					{ "Max(DueDays)", Configuration.DueDays },
					{ "Min(DueInterest)", Configuration.DueInterest },
					{ "Max(DueInterest)", Configuration.DueInterest }
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

			PersonalInformation PI = new PersonalInformation(ID);

			return await ServiceConfiguration.CurrentPersonalDebt(PI.Jid, PI.PersonalNumber, PI.Country) > 0;
		}

		/// <summary>
		/// Processes payment for selling eDaler®.
		/// </summary>
		/// <param name="ContractParameters">Parameters available in the
		/// contract authorizing the payment.</param>
		/// <param name="IdentityProperties">Properties engraved into the
		/// legal identity signing the payment request.</param>
		/// <param name="Amount">Amount to sell.</param>
		/// <param name="Currency">Currency</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if payment has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service
		/// requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Result of operation.</returns>
		public async Task<PaymentResult> SellEDaler(IDictionary<CaseInsensitiveString, object> ContractParameters,
			IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties,
			decimal Amount, string Currency, string SuccessUrl, string FailureUrl, string CancelUrl, ClientUrlEventHandler ClientUrlCallback, object State)
		{
			if (Amount != Math.Floor(Amount))
				return new PaymentResult("Only whole amounts of Neuro-Credits™ permitted.");

			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return new PaymentResult("Neuro-Credits™ service not configured properly.");

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			string AccountName = XmppClient.GetAccount(PI.Jid);

			IEnumerable<Invoice> MatchingInvoices;
			Invoice MatchingInvoice = await Database.FindFirstIgnoreRest<Invoice>(new FilterAnd(
				new FilterFieldEqualTo("Account", AccountName),
				new FilterFieldEqualTo("IsPaid", false),
				new FilterFieldEqualTo("Amount", Amount), // TODO: Take due into account.
				new FilterFieldEqualTo("Currency", Currency)));

			if (!(MatchingInvoice is null))
				MatchingInvoices = new Invoice[] { MatchingInvoice };
			else
			{
				List<Invoice> PotentialInvoices = new List<Invoice>();
				PotentialInvoices.AddRange(await Database.Find<Invoice>(new FilterAnd(
					new FilterFieldEqualTo("Account", AccountName),
					new FilterFieldEqualTo("IsPaid", false),
					new FilterFieldLesserThan("Amount", Amount), // TODO: Take due into account.
					new FilterFieldEqualTo("Currency", Currency))));

				int NrInvoices = PotentialInvoices.Count;
				if (NrInvoices > 31)
					NrInvoices = 31;    // Too many combinations to process.

				MatchingInvoices = null;
				uint CombinationMax = 1U << NrInvoices;
				uint i, j;
				int k;

				for (i = 1; i < CombinationMax; i++)
				{
					decimal Sum = 0;

					j = i;
					k = 0;
					while (j != 0)
					{
						if ((j & 1) != 0)
							Sum += PotentialInvoices[k].Amount; // TODO: Take due into account.

						k++;
						j >>= 1;
					}

					if (Sum == Amount)
					{
						List<Invoice> Matches = new List<Invoice>();

						j = i;
						k = 0;
						while (j != 0)
						{
							if ((j & 1) != 0)
								Matches.Add(PotentialInvoices[k]);

							k++;
							j >>= 1;
						}

						MatchingInvoices = Matches.ToArray();
						break;
					}
				}

				if (MatchingInvoices is null)
				{
					IEnumerable<Invoice> ExistingInvoices = await Database.Find<Invoice>(new FilterAnd(
						new FilterFieldEqualTo("Account", AccountName),
						new FilterFieldEqualTo("IsPaid", false)));

					StringBuilder sb = new StringBuilder();
					bool First = true;

					sb.Append("No pending invoice or combination of pending invoices matching that amount was found.");

					foreach (Invoice InvoicePending in ExistingInvoices)
					{
						if (First)
						{
							First = false;
							sb.Append(" Pending amounts: ");
						}
						else
							sb.Append(", ");

						sb.Append(CommonTypes.Encode(InvoicePending.Amount)); // TODO: Take due into account.
						sb.Append(' ');
						sb.Append(InvoicePending.Currency.Value);
					}

					if (!First)
						sb.Append('.');

					return new PaymentResult(sb.ToString());
				}
			}

			Amount = await ServiceConfiguration.DecrementPersonalDebt(Amount, PI.Jid, PI.PersonalNumber, PI.Country);

			if (!ContractParameters.TryGetValue("ContractId", out object Obj) || !(Obj is string ContractId))
				ContractId = null;

			if (!ContractParameters.TryGetValue("Reference", out Obj) || !(Obj is string Reference))
				Reference = null;

			foreach (Invoice Invoice2 in MatchingInvoices)
			{
				Invoice2.IsPaid = true;
				Invoice2.Paid = DateTime.UtcNow;
				Invoice2.CancellationContractId = ContractId;
				Invoice2.ExternalReference = Reference;
			}

			await Database.Update(MatchingInvoices);

			foreach (Invoice Invoice2 in MatchingInvoices)
				Log.Notice("Neuro-Credits™ cancelled.", Invoice2.InvoiceNumber.ToString(), Invoice2.Account, "InvoiceCancelled", Invoice2.GetTags());

			return new PaymentResult(Amount, Currency);
		}

		/// <summary>
		/// Gets available payment options for selling eDaler®.
		/// </summary>
		/// <param name="IdentityProperties">Properties engraved into the legal identity that will perform the request.</param>
		/// <param name="SuccessUrl">Optional Success URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="FailureUrl">Optional Failure URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="CancelUrl">Optional Cancel URL the service provider can open on the client from a client web page, if getting options has succeeded.</param>
		/// <param name="ClientUrlCallback">Method to call if the payment service
		/// requests an URL to be displayed on the client.</param>
		/// <param name="State">State object to pass on the callback method.</param>
		/// <returns>Array of dictionaries, each dictionary representing a set of parameters that can be selected in the
		/// contract to sign.</returns>
		public async Task<IDictionary<CaseInsensitiveString, object>[]> GetPaymentOptionsForSellingEDaler(
			IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties,
			string SuccessUrl, string FailureUrl, string CancelUrl, ClientUrlEventHandler ClientUrlCallback, object State)
		{
			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return new IDictionary<CaseInsensitiveString, object>[0];

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			string AccountName = XmppClient.GetAccount(PI.Jid);
			string Domain = XmppClient.GetDomain(PI.Jid);
			if (!string.IsNullOrEmpty(Domain) && !Gateway.IsDomain(Domain, true))
				return new IDictionary<CaseInsensitiveString, object>[0];

			decimal MaxAmount = await ServiceConfiguration.CurrentPersonalDebt(PI.Jid, PI.PersonalNumber, PI.Country);
			string Currency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);

			Invoice MatchingInvoice = await Database.FindFirstIgnoreRest<Invoice>(new FilterAnd(
				new FilterFieldEqualTo("Account", AccountName),
				new FilterFieldEqualTo("IsPaid", false),
				new FilterFieldEqualTo("Currency", Currency)));

			Dictionary<CaseInsensitiveString, object> Options = new Dictionary<CaseInsensitiveString, object>()
			{
				{ "Max(Amount)", MaxAmount }
			};

			if (!(MatchingInvoice is null))
				Options["Amount"] = MatchingInvoice.Amount; // TODO: Take due into account.

			return new IDictionary<CaseInsensitiveString, object>[] { Options };
		}

		#endregion

	}
}
