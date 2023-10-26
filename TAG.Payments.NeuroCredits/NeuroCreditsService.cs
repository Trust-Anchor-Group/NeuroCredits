﻿using Paiwise;
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
		private const string buyEDalerTemplateIdDev = "2ccae7c5-bea4-cdad-ec97-0d1b02016989@legal.";    // For local development, you need to republish the contracts on the local development neuron,
		private const string sellEDalerTemplateIdDev = "2ccae7dd-bea4-cdb1-ec97-0d1b0277db08@legal.";   // and replace these values with your local Contract IDs. Do not check those IDs into the repo.
		private const string buyEDalerTemplateIdProd = "2ccae830-50ee-276d-7030-d6ad4fa694bb@legal.lab.tagroot.io";
		private const string sellEDalerTemplateIdProd = "2ccae845-50ee-2774-7030-d6ad4f0f34e3@legal.lab.tagroot.io";

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
		public string BuyEDalerTemplateContractId => string.IsNullOrEmpty(Gateway.Domain) ? buyEDalerTemplateIdDev : buyEDalerTemplateIdProd;

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

			GenericObject ID = await GetLegalID(AccountName);
			if (ID is null)
				return false;

			PersonalInformation PI = new PersonalInformation(ID);
			string WalletCurrency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);

			CreditDetails Details = await MaxCreditAmountAuthorized(PI, Configuration, WalletCurrency);

			return Details.HasCredit;
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
		/// <returns>Credit details.</returns>
		internal static async Task<CreditDetails> MaxCreditAmountAuthorized(PersonalInformation PI, ServiceConfiguration Configuration, string Currency)
		{
			AccountConfiguration AccountConfiguration;
			OrganizationConfiguration OrganizationConfiguration;
			decimal MaxAmount;
			decimal CurrentDebt;

			if (Configuration.AllowOrganizations && PI.HasOrganizationalBillingInformation)
			{
				(MaxAmount, CurrentDebt, OrganizationConfiguration) = await Configuration.IsOrganizationAuthorized(PI.Jid, PI.OrganizationNumber, PI.OrganizationCountry,
					PI.PersonalNumber, PI.Country, Currency);

				if (MaxAmount > 0)
				{
					return new CreditDetails()
					{
						HasCredit = true,
						AccountConfiguration = null,
						OrganizationConfiguration = OrganizationConfiguration,
						Configuration = Configuration,
						Currency = Currency,
						MaxCredit = MaxAmount,
						Debt = CurrentDebt
					};
				}
			}

			if (Configuration.AllowPrivatePersons && PI.HasPersonalBillingInformation)
			{
				(MaxAmount, CurrentDebt, AccountConfiguration) = await Configuration.IsPersonAuthorized(PI.Jid, PI.PersonalNumber, PI.Country, Currency);
				if (MaxAmount > 0)
				{
					return new CreditDetails()
					{
						HasCredit = true,
						AccountConfiguration = AccountConfiguration,
						OrganizationConfiguration = null,
						Configuration = Configuration,
						Currency = Currency,
						MaxCredit = MaxAmount,
						Debt = CurrentDebt
					};
				}
			}

			return new CreditDetails()
			{
				HasCredit = false,
				Configuration = Configuration
			};
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
			CreditDetails Details = await MaxCreditAmountAuthorized(PI, Configuration, Currency);

			if (!Details.HasCredit)
				return new PaymentResult("Not authorized to buy Neuro-Credits™. Contact your operator to receive authorization to buy Neuro-Credits™. If there are outstanding payments, you might need to cleared those first.");

			if (Amount > Details.MaxCredit)
				return new PaymentResult("Amount exceeds maximum allowed amount.");

			Duration ExpectedPeriod = Details.Period;
			decimal ExpectedPeriodInterest = Details.PeriodInterest;
			DateTime ExpectedInitialDueDate = Details.InitialDueDate;
			decimal MaxInstallments = Details.MaxInstallments;

			if (!ContractParameters.TryGetValue("Period", out object Obj) || !(Obj is Duration Period) || Period != ExpectedPeriod)
				return new PaymentResult("Period of contract unexpected.");

			if (!ContractParameters.TryGetValue("PeriodInterest", out Obj) || !(Obj is decimal PeriodInterest) || PeriodInterest != ExpectedPeriodInterest)
				return new PaymentResult("Period interest of contract unexpected.");

			if (!ContractParameters.TryGetValue("InitialDueDate", out Obj) || !(Obj is DateTime InitialDueDate) || InitialDueDate.Subtract(ExpectedInitialDueDate).TotalDays > 1)
				return new PaymentResult("Initial due date of contract unexpected.");

			if (!ContractParameters.TryGetValue("Installments", out Obj) || !(Obj is decimal Installments) || Installments < 1 || Installments > MaxInstallments || Installments != Math.Floor(Installments))
				return new PaymentResult("Number of installments of contract unexpected.");

			if (!ContractParameters.TryGetValue("ContractId", out Obj) || !(Obj is string ContractId))
				ContractId = null;

			decimal Price = Math.Ceiling(Amount * (100 + PeriodInterest) / 100);

			if (Details.OrganizationalCredit)
				Price = await ServiceConfiguration.IncrementOrganizationalDebt(Price, PI.OrganizationNumber, PI.OrganizationCountry);
			else
				Price = await ServiceConfiguration.IncrementPersonalDebt(Price, PI.PersonalNumber, PI.Country);

			int NrInstallments = (int)Math.Ceiling(Installments);
			int Installment;
			decimal AmountLeft = Price;
			DateTime DueDate = InitialDueDate;

			for (Installment = 1; Installment <= NrInstallments; Installment++)
			{
				decimal InstallmentAmount = Math.Ceiling(AmountLeft / (NrInstallments - Installment + 1));
				AmountLeft -= InstallmentAmount;
				AmountLeft = Math.Ceiling(AmountLeft * (100 + PeriodInterest) * 0.01M);

				Invoice Invoice = new Invoice()
				{
					InvoiceNumber = await RuntimeCounters.IncrementCounter(NeuroCreditsServiceProvider.ServiceId + ".NrInvoices"),
					Installment = Installment,
					NrInstallments = NrInstallments,
					Account = XmppClient.GetAccount(PI.Jid),
					IsPaid = false,
					Amount = InstallmentAmount,
					LateFees = 0,
					NrReminders = 0,
					Currency = Currency,
					DueDate = DueDate,
					Period = Period,
					PeriodInterest = PeriodInterest,
					Created = DateTime.UtcNow,
					Paid = DateTime.MinValue,
					InvoiceDate = DateTime.MinValue,
					LastReminder = DateTime.MinValue,
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

				DueDate += Period;
			}

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
			CreditDetails Details = await MaxCreditAmountAuthorized(PI, Configuration, WalletCurrency);

			return new IDictionary<CaseInsensitiveString, object>[]
			{
				new Dictionary<CaseInsensitiveString, object>()
				{
					{ "Period", Details.Period },
					{ "PeriodInterest", Details.PeriodInterest },
					{ "Max(Amount)", Details.MaxCredit },
					{ "Min(Period)", Details.Period },
					{ "Max(Period)", Details.Period },
					{ "Min(PeriodInterest)", Details.PeriodInterest },
					{ "Max(PeriodInterest)", Details.PeriodInterest },
					{ "Currency", Details.Currency },
					{ "Min(Currency)", Details.Currency },
					{ "Max(Currency)", Details.Currency },
					{ "Installments", 1 },
					{ "Max(Installments)", Details.MaxInstallments }
				}
			};
		}

		#endregion

		#region ISellEDalerService

		/// <summary>
		/// Contract ID of Template, for selling e-Daler
		/// </summary>
		public string SellEDalerTemplateContractId => string.IsNullOrEmpty(Gateway.Domain) ? sellEDalerTemplateIdDev : sellEDalerTemplateIdProd;

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

			decimal Debt = await ServiceConfiguration.CurrentPersonalDebt(PI.PersonalNumber, PI.Country);

			if (PI.HasOrganizationalBillingInformation)
				Debt += await ServiceConfiguration.CurrentOrganizationDebt(PI.OrganizationNumber, PI.OrganizationCountry);

			return Debt > 0;
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
			decimal Ticks = Amount * 10000;
			if (Ticks != Math.Floor(Ticks))
				return new PaymentResult("Amount contains too many decimals.");

			ServiceConfiguration Configuration = await ServiceConfiguration.GetCurrent();
			if (!Configuration.IsWellDefined)
				return new PaymentResult("Neuro-Credits™ service not configured properly.");

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			decimal AmountLeft = Amount;
			decimal AmountPaid = 0;
			decimal Paid;

			if (!ContractParameters.TryGetValue("ContractId", out object Obj) || !(Obj is string ContractId))
				ContractId = null;

			if (!ContractParameters.TryGetValue("Reference", out Obj) || !(Obj is string Reference))
				Reference = null;

			IEnumerable<Invoice> PersonalUnpaidInvoices = await Database.Find<Invoice>(new FilterAnd(
				new FilterFieldEqualTo("PersonalNumber", PI.PersonalNumber),
				new FilterFieldEqualTo("Country", PI.Country),
				new FilterFieldEqualTo("IsPaid", false),
				new FilterFieldEqualTo("Currency", Currency)));

			foreach (Invoice Invoice in PersonalUnpaidInvoices)
			{
				if (AmountLeft >= Invoice.AmountLeft)
				{
					Paid = await ServiceConfiguration.DecrementPersonalDebt(Invoice.AmountLeft, PI.PersonalNumber, PI.Country);
					AmountLeft -= Paid;
					AmountPaid += Paid;

					Invoice.AmountPaid += Paid;
					Invoice.IsPaid = true;
					Invoice.Paid = DateTime.UtcNow;
					Invoice.CancellationContractId = ContractId;
					Invoice.ExternalReference = Reference;

					Log.Notice("Neuro-Credits™ cancelled.", Invoice.InvoiceNumber.ToString(), PI.Jid, "InvoiceCancelled", Invoice.GetTags());
				}
				else
				{
					Paid = await ServiceConfiguration.DecrementPersonalDebt(AmountLeft, PI.PersonalNumber, PI.Country);
					AmountLeft = 0;
					AmountPaid += Paid;

					Invoice.AmountPaid += Paid;

					Log.Informational("Neuro-Credits™ partially paid.", Invoice.InvoiceNumber.ToString(), PI.Jid, "InvoicePartialPayment", Invoice.GetTags());
				}

				await Database.Update(Invoice);

				if (AmountLeft <= 0)
					return new PaymentResult(AmountPaid, Currency);
			}

			if (PI.HasOrganizationalBillingInformation)
			{
				IEnumerable<Invoice> OrganizationalUnpaidInvoices = await Database.Find<Invoice>(new FilterAnd(
					new FilterFieldEqualTo("OrganizationNumber", PI.OrganizationNumber),
					new FilterFieldEqualTo("OrganizationCountry", PI.OrganizationCountry),
					new FilterFieldEqualTo("IsPaid", false),
					new FilterFieldEqualTo("Currency", Currency)));

				foreach (Invoice Invoice in OrganizationalUnpaidInvoices)
				{
					if (AmountLeft >= Invoice.AmountLeft)
					{
						Paid = await ServiceConfiguration.DecrementOrganizationalDebt(Invoice.AmountLeft, PI.OrganizationNumber, PI.OrganizationCountry);
						AmountLeft -= Paid;
						AmountPaid += Paid;

						Invoice.AmountPaid += Paid;
						Invoice.IsPaid = true;
						Invoice.Paid = DateTime.UtcNow;
						Invoice.CancellationContractId = ContractId;
						Invoice.ExternalReference = Reference;

						Log.Notice("Neuro-Credits™ cancelled.", Invoice.InvoiceNumber.ToString(), PI.Jid, "InvoiceCancelled", Invoice.GetTags());
					}
					else
					{
						Paid = await ServiceConfiguration.DecrementOrganizationalDebt(AmountLeft, PI.OrganizationNumber, PI.OrganizationCountry);
						AmountLeft = 0;
						AmountPaid += Paid;

						Invoice.AmountPaid += Paid;

						Log.Informational("Neuro-Credits™ partially paid.", Invoice.InvoiceNumber.ToString(), PI.Jid, "InvoicePartialPayment", Invoice.GetTags());
					}

					await Database.Update(Invoice);

					if (AmountLeft <= 0)
						return new PaymentResult(AmountPaid, Currency);
				}
			}

			return new PaymentResult(AmountPaid, Currency);
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

			decimal MaxAmount = await ServiceConfiguration.CurrentPersonalDebt(PI.PersonalNumber, PI.Country);
			string Currency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);

			if (PI.HasOrganizationalBillingInformation)
				MaxAmount += await ServiceConfiguration.CurrentOrganizationDebt(PI.OrganizationNumber, PI.OrganizationCountry);

			return new IDictionary<CaseInsensitiveString, object>[]
			{
				new Dictionary<CaseInsensitiveString, object>()
				{
					{ "Amount", MaxAmount },
					{ "Max(Amount)", MaxAmount },
					{ "Currency", Currency },
					{ "Min(Currency)", Currency },
					{ "Max(Currency)", Currency }
				}
			};
		}

		#endregion

	}
}
