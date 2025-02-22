﻿using Paiwise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TAG.Payments.NeuroCredits.Configuration;
using TAG.Payments.NeuroCredits.Data;
using Waher.Content;
using Waher.Content.Html.Css;
using Waher.Content.Markdown;
using Waher.Content.Multipart;
using Waher.Events;
using Waher.IoTGateway;
using Waher.IoTGateway.Setup;
using Waher.Networking.XMPP;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Counters;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Runtime.Settings;
using Waher.Script;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace TAG.Payments.NeuroCredits
{
	/// <summary>
	/// Serivce for Neuro-Credits™
	/// </summary>
	public class NeuroCreditsService : IBuyEDalerService, ISellEDalerService
	{
		private const string buyEDalerTemplateIdProd = "2e528bff-b68d-43cb-841b-617a27918722@legal.paiwise.tagroot.io";
		private const string sellEDalerTemplateIdProd = "2e528c19-b68d-43cd-841b-617a27b6766d@legal.paiwise.tagroot.io";

		/// <summary>
		/// ID for contract template for buying Neuro-Credits™.
		/// </summary>
		public static string BuyEDalerTemplateId => GetBuyEDalerTemplateId().Result;

		/// <summary>
		/// Gets the ID for contract template for buying Neuro-Credits™.
		/// </summary>
		public static Task<string> GetBuyEDalerTemplateId()
		{
			if (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain))
				return RuntimeSettings.GetAsync(NeuroCreditsServiceProvider.ServiceId + ".Dev.BuyEDalerTemplateId", string.Empty);
			else 
				return Task.FromResult(buyEDalerTemplateIdProd);
		}

		/// <summary>
		/// ID for contract template for cancelling Neuro-Credits™.
		/// </summary>
		public static string SellEDalerTemplateId => GetSellEDalerTemplateId().Result;

		/// <summary>
		/// Gets the ID for contract template for cancelling Neuro-Credits™.
		/// </summary>
		public static Task<string> GetSellEDalerTemplateId()
		{
			if (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain))
				return RuntimeSettings.GetAsync(NeuroCreditsServiceProvider.ServiceId + ".Dev.SellEDalerTemplateId", string.Empty);
			else
				return Task.FromResult(sellEDalerTemplateIdProd);
		}

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
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			return ServiceConfiguration.SupportsCurrency(Currency);
		}

		#endregion

		#region IBuyEDalerService

		/// <summary>
		/// Contract ID of Template, for buying e-Daler
		/// </summary>
		public string BuyEDalerTemplateContractId => BuyEDalerTemplateId;

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
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			if (!ServiceConfiguration.IsWellDefined)
				return false;

			BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();
			if (!BillingConfiguration.IsWellDefined)
				return false;

			GenericObject ID = await GetLegalID(AccountName);
			if (ID is null)
				return false;

			if (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain) && string.IsNullOrEmpty(await GetBuyEDalerTemplateId()))
				return false;

			PersonalInformation PI = new PersonalInformation(ID);
			string WalletCurrency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);

			CreditDetails Details = await MaxCreditAmountAuthorized(PI, ServiceConfiguration, WalletCurrency);

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
			decimal Amount, string Currency, string SuccessUrl, string FailureUrl, string CancelUrl, EventHandlerAsync<ClientUrlEventArgs> ClientUrlCallback, object State)
		{
			if (Amount != Math.Floor(Amount))
				return new PaymentResult("Only whole amounts of Neuro-Credits™ permitted.");

			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			if (!ServiceConfiguration.IsWellDefined)
				return new PaymentResult("Neuro-Credits™ service not configured properly.");

			BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();
			if (!BillingConfiguration.IsWellDefined)
				return new PaymentResult("Neuro-Credits™ service not configured properly.");

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			CreditDetails Details = await MaxCreditAmountAuthorized(PI, ServiceConfiguration, Currency);

			if (!Details.HasCredit)
				return new PaymentResult("Not authorized to buy Neuro-Credits™. Contact your operator to receive authorization to buy Neuro-Credits™. If there are outstanding payments, you might need to cleared those first.");

			if (Amount > Details.MaxCredit)
				return new PaymentResult("Amount exceeds maximum allowed amount.");

			Duration ExpectedPeriod = Details.Period;
			decimal ExpectedPeriodInterest = Details.PeriodInterest;
			DateTime ExpectedInitialDueDate = Details.InitialDueDate;
			decimal MaxInstallments = Details.MaxInstallments;
			decimal ExpectedInvoiceFee = Details.Configuration.InvoiceFee;

			if (!ContractParameters.TryGetValue("Period", out object Obj) || !(Obj is Duration Period) || Period != ExpectedPeriod)
				return new PaymentResult("Period of contract unexpected.");

			if (!ContractParameters.TryGetValue("PeriodInterest", out Obj) || !(Obj is decimal PeriodInterest) || PeriodInterest != ExpectedPeriodInterest)
				return new PaymentResult("Period interest of contract unexpected.");

			if (!ContractParameters.TryGetValue("InvoiceFee", out Obj) || !(Obj is decimal InvoiceFee) || InvoiceFee != ExpectedInvoiceFee)
				return new PaymentResult("Invoice fee of contract unexpected.");

			if (!ContractParameters.TryGetValue("InitialDueDate", out Obj) || !(Obj is DateTime InitialDueDate) || InitialDueDate.Subtract(ExpectedInitialDueDate).TotalDays > 1)
				return new PaymentResult("Initial due date of contract unexpected.");

			if (!ContractParameters.TryGetValue("Installments", out Obj) || !(Obj is decimal Installments) || Installments < 1 || Installments > MaxInstallments || Installments != Math.Floor(Installments))
				return new PaymentResult("Number of installments of contract unexpected.");

			if (!ContractParameters.TryGetValue("ContractId", out Obj) || !(Obj is string ContractId))
				ContractId = null;

			if (!ContractParameters.TryGetValue("Message", out Obj) || !(Obj is string Message))
				Message = string.Empty;

			CaseInsensitiveString AccountName = XmppClient.GetAccount(PI.Jid);
			string EMail = await GetEMail(AccountName);
			decimal PurchaseAmount = Amount;
			decimal AmountLeft = Math.Ceiling(Amount * (100 + PeriodInterest) / 100);
			decimal Price = 0;
			int NrInstallments = (int)Math.Ceiling(Installments);
			int Installment;
			DateTime DueDate = InitialDueDate;
			List<Invoice> Invoices = new List<Invoice>();

			for (Installment = 1; Installment <= NrInstallments; Installment++)
			{
				decimal InstallmentAmount = Math.Ceiling(AmountLeft / (NrInstallments - Installment + 1));

				if (Details.OrganizationalCredit)
				{
					InvoiceFee = await ServiceConfiguration.IncrementOrganizationalDebt(InvoiceFee, PI.OrganizationNumber, PI.OrganizationCountry);
					InstallmentAmount = await ServiceConfiguration.IncrementOrganizationalDebt(InstallmentAmount, PI.OrganizationNumber, PI.OrganizationCountry);
				}
				else
				{
					InvoiceFee = await ServiceConfiguration.IncrementPersonalDebt(InvoiceFee, PI.PersonalNumber, PI.Country);
					InstallmentAmount = await ServiceConfiguration.IncrementPersonalDebt(InstallmentAmount, PI.PersonalNumber, PI.Country);
				}

				AmountLeft -= InstallmentAmount;
				AmountLeft = Math.Ceiling(AmountLeft * (100 + PeriodInterest) * 0.01M);

				DateTime CreationTime = DateTime.UtcNow;
				Invoice Invoice = new Invoice()
				{
					InvoiceNumber = await RuntimeCounters.IncrementCounter(NeuroCreditsServiceProvider.ServiceId + ".NrInvoices"),
					Installment = Installment,
					NrInstallments = NrInstallments,
					Account = AccountName,
					IsPaid = false,
					PurchaseAmount = PurchaseAmount,
					Amount = InstallmentAmount + InvoiceFee,
					InvoiceFee = InvoiceFee,
					Message = Message,
					LateFees = 0,
					NrReminders = 0,
					Currency = Currency,
					DueDate = DueDate,
					Period = Period,
					PeriodInterest = PeriodInterest,
					Created = CreationTime,
					InvoiceDate = CreationTime,
					Paid = DateTime.MinValue,
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
					Agent = PI.Agent,
					EMail = EMail,
					Salt = Gateway.NextBytes(32)
				};

				if (Details.OrganizationalCredit)
				{
					Invoice.OrganizationName = PI.OrganizationName;
					Invoice.Department = PI.Department;
					Invoice.Role = PI.Role;
					Invoice.OrganizationNumber = PI.OrganizationNumber;
					Invoice.OrganizationAddress = PI.OrganizationAddress;
					Invoice.OrganizationAddress2 = PI.OrganizationAddress2;
					Invoice.OrganizationArea = PI.OrganizationArea;
					Invoice.OrganizationCity = PI.OrganizationCity;
					Invoice.OrganizationPostalCode = PI.OrganizationPostalCode;
					Invoice.OrganizationRegion = PI.OrganizationRegion;
					Invoice.OrganizationCountry = PI.OrganizationCountry;
				}

				Invoices.Add(Invoice);
				Price += InstallmentAmount;
				DueDate += Period;

				Log.Notice("Neuro-Credits™ bought.", ContractId, AccountName, "InvoiceCreated", Invoice.GetTags());
			}

			foreach (Invoice Invoice in Invoices)
				Invoice.PurchasePrice = Price;

			await Database.Insert(Invoices.ToArray());
			await SendInvoices(Invoices, EMail, Details.OrganizationalCredit, BillingConfiguration);

			return new PaymentResult(Amount, Currency);
		}

		private static async Task<string> GetEMail(CaseInsensitiveString AccountName)
		{
			string EMail = null;

			// select top 1 EMail from "BrokerAccounts" where UserName=AccountName

			foreach (object Item in await Database.Find("BrokerAccounts", 0, 1, new FilterFieldEqualTo("UserName", AccountName)))
			{
				GenericObject GenObj = await Database.Generalize(Item);

				if (GenObj.TryGetFieldValue("EMail", out object Obj))
					EMail = Obj?.ToString();
			}

			return EMail;
		}

		private static async Task SendInvoices(IEnumerable<Invoice> Invoices, string EMail, bool OrganizationalCredit, BillingConfiguration BillingConfiguration)
		{
			string InvoiceTemplateFileName;
			string ReceiptTemplateFileName;
			string StylesFileName;

			StylesFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "MailStyles", "Default.css");

			if (OrganizationalCredit)
			{
				ReceiptTemplateFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "ReceiptTemplates", "DefaultOrganizationalReceipt.md");
				InvoiceTemplateFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "InvoiceTemplates", "DefaultOrganizationalInvoice.md");
			}
			else
			{
				ReceiptTemplateFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "ReceiptTemplates", "DefaultPersonalReceipt.md");
				InvoiceTemplateFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "InvoiceTemplates", "DefaultPersonalInvoice.md");
			}

			string ObjectId = null;
			try
			{
				string Styles = await Files.ReadAllTextAsync(StylesFileName);

				ObjectId = ReceiptTemplateFileName;
				string ReceiptMarkdown = await Files.ReadAllTextAsync(ReceiptTemplateFileName);

				ObjectId = InvoiceTemplateFileName;
				string InvoiceMarkdown = await Files.ReadAllTextAsync(InvoiceTemplateFileName);
				string Markdown;

				MailConfiguration MailConfiguration = await MailConfiguration.GetCurrent();

				foreach (Invoice Invoice in Invoices)
				{
					Variables Variables = new Variables()
					{
						["Invoice"] = Invoice,
						["Billing"] = BillingConfiguration,
					};
					MarkdownSettings Settings = new MarkdownSettings(null, false, Variables);

					if (Invoice.Installment == 1)
					{
						ObjectId = ReceiptTemplateFileName;
						Markdown = await MarkdownDocument.Preprocess(ReceiptMarkdown, Settings);
						await Gateway.SendNotification(Markdown);
						await SendEMail(EMail, "Receipt, Purchase of Neuro-Credits™", Markdown, Styles, null, null, null);
					}

					string Subject = "Invoice" + (Invoice.NrInstallments > 1 ? " " + Invoice.Installment.ToString() : string.Empty) +
						", Purchase of Neuro-Credits™";

					ObjectId = InvoiceTemplateFileName;
					Markdown = await MarkdownDocument.Preprocess(InvoiceMarkdown, Settings);
					await Gateway.SendNotification(Markdown);

					if (MailConfiguration.IsWellDefined && !string.IsNullOrEmpty(EMail))
					{
						MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
						string HTML = await Doc.GenerateHTML();
						string Text = await Doc.GeneratePlainText();

						int i = HTML.IndexOf("<html");
						if (i > 0)
							HTML = HTML[i..];

						StringBuilder Reminder = new StringBuilder();
						DateTime TP = DateTime.UtcNow;
						TimeZoneInfo TZ = TimeZoneInfo.Local;

						AppendCalendar(Reminder, "BEGIN:VCALENDAR");
						AppendCalendar(Reminder, "PRODID:-//Trust Anchor Group AB//Neuro-Credits//EN");
						AppendCalendar(Reminder, "VERSION:2.0");
						AppendCalendar(Reminder, "CALSCALE:GREGORIAN");
						AppendCalendar(Reminder, "METHOD:REQUEST");
						AppendCalendarTimeZone(Reminder, TZ, TP);

						AppendCalendar(Reminder, "BEGIN:VEVENT");
						AppendCalendar(Reminder, "DTSTART;TZID=" + TZ.Id + ":" + Invoice.DueDate.Year.ToString("D4") +
							Invoice.DueDate.Month.ToString("D2") + Invoice.DueDate.Day.ToString("D2") + "T100000");
						AppendCalendar(Reminder, "DTEND;TZID=" + TZ.Id + ":" + Invoice.DueDate.Year.ToString("D4") +
							Invoice.DueDate.Month.ToString("D2") + Invoice.DueDate.Day.ToString("D2") + "T103000");
						AppendCalendar(Reminder, "DTSTAMP:" + TP.Year.ToString("D4") + TP.Month.ToString("D2") +
							TP.Day.ToString("D2") + "T" + TP.Hour.ToString("D2") + TP.Minute.ToString("D2") +
							TP.Second.ToString("D2") + "Z");

						string Organizer = DomainConfiguration.Instance.HumanReadableName;
						if (string.IsNullOrEmpty(Organizer))
							Organizer = Gateway.Domain;

						AppendCalendar(Reminder, "ORGANIZER;CN=" + Organizer + ":mailto:" + MailConfiguration.Address);
						AppendCalendar(Reminder, "UID:" + Invoice.ObjectId + "_" + Invoice.NrReminders.ToString());
						AppendCalendar(Reminder, "ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=" +
							Invoice.PersonalName + ":mailto:" + EMail);
						AppendCalendar(Reminder, "DESCRIPTION:" + Text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\\n"));
						AppendCalendar(Reminder, "X-ALT-DESC;FMTTYPE=text/html:" + HTML.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\\n"));
						AppendCalendar(Reminder, "LAST-MODIFIED:" + TP.Year.ToString("D4") + TP.Month.ToString("D2") +
							TP.Day.ToString("D2") + "T" + TP.Hour.ToString("D2") + TP.Minute.ToString("D2") + TP.Second.ToString("D2") + "Z");
						AppendCalendar(Reminder, "LOCATION:");
						AppendCalendar(Reminder, "SEQUENCE:0");
						AppendCalendar(Reminder, "STATUS:CONFIRMED");
						AppendCalendar(Reminder, "SUMMARY:" + Subject);
						AppendCalendar(Reminder, "TRANSP:TRANSPARENT");
						AppendCalendar(Reminder, "BEGIN:VALARM");
						AppendCalendar(Reminder, "DESCRIPTION:" + Subject);
						AppendCalendar(Reminder, "ACTION:DISPLAY");
						AppendCalendar(Reminder, "TRIGGER:-PT10M");
						AppendCalendar(Reminder, "END:VALARM");
						AppendCalendar(Reminder, "END:VEVENT");
						AppendCalendar(Reminder, "END:VCALENDAR");

						await SendEMail(EMail, Subject, Markdown, Styles, Reminder.ToString(), "Reminder.ics", "REQUEST");
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex, ObjectId);
			}
		}

		private static void AppendCalendarTimeZone(StringBuilder Reminder, TimeZoneInfo TZ, DateTime TP)
		{
			TimeSpan TS;

			AppendCalendar(Reminder, "BEGIN:VTIMEZONE");
			AppendCalendar(Reminder, "TZID:" + TZ.Id);
			foreach (TimeZoneInfo.AdjustmentRule Rule in TZ.GetAdjustmentRules())
			{
				if (Rule.DateStart > TP.Date || Rule.DateEnd < TP.Date)
					continue;

				AppendCalendar(Reminder, "BEGIN:STANDARD");
				TS = TZ.BaseUtcOffset + Rule.DaylightDelta;
				AppendCalendar(Reminder, "TZOFFSETFROM:" + TS.Hours.ToString("D2") + TS.Minutes.ToString("D2"));
				TS = TZ.BaseUtcOffset;
				AppendCalendar(Reminder, "TZOFFSETTO:" + TS.Hours.ToString("D2") + TS.Minutes.ToString("D2"));
				AppendCalendar(Reminder, "TZNAME:" + TS.Hours.ToString("D2") + (TS.Minutes == 0 ? string.Empty : TS.Minutes.ToString("D2")));

				AppendCalendar(Reminder, "DTSTART:" + Rule.DateStart.Year.ToString("D4") + Rule.DateStart.Month.ToString("D2") +
					Rule.DateStart.Day.ToString("D2") + "T100000");
				AppendCalendar(Reminder, "END:STANDARD");

				AppendCalendar(Reminder, "BEGIN:DAYLIGHT");
				TS = TZ.BaseUtcOffset + Rule.DaylightDelta;
				AppendCalendar(Reminder, "TZOFFSETFROM:" + TS.Hours.ToString("D2") + TS.Minutes.ToString("D2"));
				AppendCalendar(Reminder, "TZOFFSETTO:" + TS.Hours.ToString("D2") + TS.Minutes.ToString("D2"));
				AppendCalendar(Reminder, "TZNAME:" + TS.Hours.ToString("D2") + (TS.Minutes == 0 ? string.Empty : TS.Minutes.ToString("D2")));

				AppendCalendar(Reminder, "DTSTART:" + Rule.DateStart.Year.ToString("D4") +
					Rule.DaylightTransitionStart.Month.ToString("D2") + Rule.DaylightTransitionStart.Day.ToString("D2") + "T" +
					Rule.DaylightTransitionStart.TimeOfDay.Hour.ToString("D2") +
					Rule.DaylightTransitionStart.TimeOfDay.Minute.ToString("D2") +
					Rule.DaylightTransitionStart.TimeOfDay.Second.ToString("D2"));
				AppendCalendar(Reminder, "END:DAYLIGHT");
			}

			AppendCalendar(Reminder, "END:VTIMEZONE");
		}

		private static async Task SendCancellations(IEnumerable<Invoice> Invoices, string EMail, bool OrganizationalCredit, BillingConfiguration BillingConfiguration)
		{
			string CancellationTemplateFileName;
			string StylesFileName;

			StylesFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "MailStyles", "Default.css");

			if (OrganizationalCredit)
				CancellationTemplateFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "CancellationTemplates", "DefaultOrganizationalCancellation.md");
			else
				CancellationTemplateFileName = Path.Combine(Gateway.RootFolder, "NeuroCredits", "CancellationTemplates", "DefaultPersonalCancellation.md");

			try
			{
				string Styles = await Files.ReadAllTextAsync(StylesFileName);
				string CancellationMarkdown = await Files.ReadAllTextAsync(CancellationTemplateFileName);
				string Markdown;

				MailConfiguration MailConfiguration = await MailConfiguration.GetCurrent();

				foreach (Invoice Invoice in Invoices)
				{
					Variables Variables = new Variables()
					{
						["Invoice"] = Invoice,
						["Billing"] = BillingConfiguration,
					};
					MarkdownSettings Settings = new MarkdownSettings(null, false, Variables);

					Markdown = await MarkdownDocument.Preprocess(CancellationMarkdown, Settings);
					await Gateway.SendNotification(Markdown);

					if (MailConfiguration.IsWellDefined && !string.IsNullOrEmpty(EMail))
					{
						MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
						string HTML = await Doc.GenerateHTML();
						string Text = await Doc.GeneratePlainText();
						string Subject = "Cancellation of Invoice" + (Invoice.NrInstallments > 1 ? " " + Invoice.Installment.ToString() : string.Empty) +
							", Purchase of Neuro-Credits™";

						int i = HTML.IndexOf("<html");
						if (i > 0)
							HTML = HTML[i..];

						StringBuilder Reminder = new StringBuilder();
						DateTime TP = DateTime.UtcNow;
						TimeZoneInfo TZ = TimeZoneInfo.Local;

						AppendCalendar(Reminder, "BEGIN:VCALENDAR");
						AppendCalendar(Reminder, "PRODID:-//Trust Anchor Group AB//Neuro-Credits//EN");
						AppendCalendar(Reminder, "VERSION:2.0");
						AppendCalendar(Reminder, "CALSCALE:GREGORIAN");
						AppendCalendar(Reminder, "METHOD:CANCEL");
						AppendCalendarTimeZone(Reminder, TZ, TP);

						AppendCalendar(Reminder, "BEGIN:VEVENT");
						AppendCalendar(Reminder, "DTSTART;TZID=" + TZ.Id + ":" + Invoice.DueDate.Year.ToString("D4") +
							Invoice.DueDate.Month.ToString("D2") + Invoice.DueDate.Day.ToString("D2") + "T100000");
						AppendCalendar(Reminder, "DTEND;TZID=" + TZ.Id + ":" + Invoice.DueDate.Year.ToString("D4") +
							Invoice.DueDate.Month.ToString("D2") + Invoice.DueDate.Day.ToString("D2") + "T103000");
						AppendCalendar(Reminder, "DTSTAMP:" + TP.Year.ToString("D4") + TP.Month.ToString("D2") +
							TP.Day.ToString("D2") + "T" + TP.Hour.ToString("D2") + TP.Minute.ToString("D2") +
							TP.Second.ToString("D2") + "Z");
						AppendCalendar(Reminder, "UID:" + Invoice.ObjectId + "_" + Invoice.NrReminders.ToString());
						AppendCalendar(Reminder, "DESCRIPTION:Invoice has been paid.");
						AppendCalendar(Reminder, "LAST-MODIFIED:" + TP.Year.ToString("D4") + TP.Month.ToString("D2") +
							TP.Day.ToString("D2") + "T" + TP.Hour.ToString("D2") + TP.Minute.ToString("D2") + TP.Second.ToString("D2") + "Z");
						AppendCalendar(Reminder, "LOCATION:");
						AppendCalendar(Reminder, "SEQUENCE:0");
						AppendCalendar(Reminder, "STATUS:CANCELLED");
						AppendCalendar(Reminder, "SUMMARY:" + Subject);
						AppendCalendar(Reminder, "END:VEVENT");
						AppendCalendar(Reminder, "END:VCALENDAR");

						await SendEMail(EMail, Subject, Markdown, Styles, Reminder.ToString(), "Cancellation.ics", "CANCEL");
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex, CancellationTemplateFileName);
			}
		}

		private static void AppendCalendar(StringBuilder Calendar, string Row)
		{
			int RowLen = 75;
			int i = 0;
			int Left = Row.Length;

			while (true)
			{
				if (Left > RowLen)
				{
					Calendar.AppendLine(Row.Substring(i, RowLen));
					i += RowLen;
					Left -= RowLen;
					RowLen = 74;
					Calendar.Append(' ');
				}
				else
				{
					Calendar.AppendLine(Row[i..]);
					break;
				}
			}
		}

		/// <summary>
		/// Sends an e-mail to a recipient, using the account configured for the service.
		/// </summary>
		/// <param name="EMail">Recipient e-mail address.</param>
		/// <param name="Subject">Subject header.</param>
		/// <param name="Markdown">Markdown content.</param>
		public static async Task SendTestEMail(string EMail, string Subject, string Markdown)
		{
			bool Result = await SendEMail(EMail, Subject, Markdown, null, null, null, null);
			string[] TabIDs = ClientEvents.GetTabIDsForLocation("/NeuroCredits/Mail.md");
			if (TabIDs.Length > 0)
				await ClientEvents.PushEvent(TabIDs, "TestMailSent", CommonTypes.Encode(Result), true);
		}

		/// <summary>
		/// Sends an e-mail to a recipient, using the account configured for the service.
		/// </summary>
		/// <param name="EMail">Recipient e-mail address.</param>
		/// <param name="Subject">Subject header.</param>
		/// <param name="Markdown">Markdown content.</param>
		/// <param name="Styles">Styles to use in the formatted message.</param>
		/// <param name="CalendarReminder">Calendar reminder entry.</param>
		public static async Task<bool> SendEMail(string EMail, string Subject, string Markdown, string Styles, string CalendarReminder,
			string CalendarFileName, string CalendarMethod)
		{
			try
			{
				MailConfiguration Configuration = await MailConfiguration.GetCurrent();
				if (!Configuration.IsWellDefined)
					return false;

				List<ObjectValue> Attachments = new List<ObjectValue>();

				if (!string.IsNullOrEmpty(Styles))
					Attachments.Add(new ObjectValue(new CssDocument(Styles)));

				if (!string.IsNullOrEmpty(CalendarReminder))
				{
					EmbeddedContent Entry = new EmbeddedContent()
					{
						Raw = Encoding.UTF8.GetBytes(CalendarReminder),
						ContentType = "text/calendar; method=\"" + CalendarMethod + "\"; component=\"VEVENT\"; charset=\"utf-8\"",
						FileName = CalendarFileName,
						Disposition = ContentDisposition.Attachment
					};

					Attachments.Add(new ObjectValue(Entry));
				}

				Variables Variables = new Variables()
				{
					["Host"] = new StringValue(Configuration.Host),
					["Port"] = new DoubleNumber(Configuration.Port),
					["UserName"] = new StringValue(Configuration.Account),
					["Password"] = new StringValue(Configuration.Password),
					["From"] = new StringValue(Configuration.Address),
					["To"] = new StringValue(EMail),
					["Subject"] = new StringValue(Subject),
					["Markdown"] = new StringValue(Markdown),
					["Attachments"] = new ObjectVector(Attachments.ToArray())
				};

				await Expression.EvalAsync("SendMail(Host,Port,UserName,Password,From,To,Subject,Markdown,Attachments)", Variables);

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
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
			string SuccessUrl, string FailureUrl, string CancelUrl, EventHandlerAsync<ClientUrlEventArgs> ClientUrlCallback, object State)
		{
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			if (!ServiceConfiguration.IsWellDefined)
				return new IDictionary<CaseInsensitiveString, object>[0];

			BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();
			if (!BillingConfiguration.IsWellDefined)
				return new IDictionary<CaseInsensitiveString, object>[0];

			PersonalInformation PI = new PersonalInformation(IdentityProperties);
			string AccountName = XmppClient.GetAccount(PI.Jid);
			string Domain = XmppClient.GetDomain(PI.Jid);
			if (!string.IsNullOrEmpty(Domain) && !Gateway.IsDomain(Domain, true))
				return new IDictionary<CaseInsensitiveString, object>[0];

			string WalletCurrency = await ServiceConfiguration.GetCurrencyOfAccount(AccountName);
			CreditDetails Details = await MaxCreditAmountAuthorized(PI, ServiceConfiguration, WalletCurrency);

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
					{ "InvoiceFee", Details.Configuration.InvoiceFee },
					{ "Min(InvoiceFee)", Details.Configuration.InvoiceFee },
					{ "Max(InvoiceFee)", Details.Configuration.InvoiceFee },
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
		public string SellEDalerTemplateContractId => SellEDalerTemplateId;

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
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			if (!ServiceConfiguration.IsWellDefined)
				return false;

			if (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain) && string.IsNullOrEmpty(await GetSellEDalerTemplateId()))
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
			decimal Amount, string Currency, string SuccessUrl, string FailureUrl, string CancelUrl, EventHandlerAsync<ClientUrlEventArgs> ClientUrlCallback, object State)
		{
			decimal Ticks = Amount * 10000;
			if (Ticks != Math.Floor(Ticks))
				return new PaymentResult("Amount contains too many decimals.");

			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			if (!ServiceConfiguration.IsWellDefined)
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
			LinkedList<Invoice> PaidPersonalInvoices = null;
			LinkedList<Invoice> PaidOrganizationalInvoices = null;

			foreach (Invoice Invoice in PersonalUnpaidInvoices)
			{
				if (AmountLeft >= Invoice.AmountLeft)
				{
					if (Invoice.IsOrganizational)
					{
						Paid = await ServiceConfiguration.DecrementOrganizationalDebt(Invoice.AmountLeft, Invoice.OrganizationNumber, Invoice.OrganizationCountry);
						Add(ref PaidOrganizationalInvoices, Invoice);
					}
					else
					{
						Paid = await ServiceConfiguration.DecrementPersonalDebt(Invoice.AmountLeft, Invoice.PersonalNumber, Invoice.Country);
						Add(ref PaidPersonalInvoices, Invoice);
					}

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
					if (Invoice.IsOrganizational)
						Paid = await ServiceConfiguration.DecrementOrganizationalDebt(AmountLeft, Invoice.OrganizationNumber, Invoice.OrganizationCountry);
					else
						Paid = await ServiceConfiguration.DecrementPersonalDebt(AmountLeft, Invoice.PersonalNumber, Invoice.Country);

					AmountLeft = 0;
					AmountPaid += Paid;

					Invoice.AmountPaid += Paid;

					Log.Informational("Neuro-Credits™ partially paid.", Invoice.InvoiceNumber.ToString(), PI.Jid, "InvoicePartialPayment", Invoice.GetTags());
				}

				await Database.Update(Invoice);

				if (AmountLeft <= 0)
					break;
			}

			if (AmountLeft > 0 && PI.HasOrganizationalBillingInformation)
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
						if (Invoice.IsOrganizational)
						{
							Paid = await ServiceConfiguration.DecrementOrganizationalDebt(Invoice.AmountLeft, Invoice.OrganizationNumber, Invoice.OrganizationCountry);
							Add(ref PaidOrganizationalInvoices, Invoice);
						}
						else
						{
							Paid = await ServiceConfiguration.DecrementPersonalDebt(Invoice.AmountLeft, Invoice.PersonalNumber, Invoice.Country);
							Add(ref PaidPersonalInvoices, Invoice);
						}

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
						if (Invoice.IsOrganizational)
							Paid = await ServiceConfiguration.DecrementOrganizationalDebt(AmountLeft, Invoice.OrganizationNumber, Invoice.OrganizationCountry);
						else
							Paid = await ServiceConfiguration.DecrementPersonalDebt(AmountLeft, Invoice.PersonalNumber, Invoice.Country);

						AmountLeft = 0;
						AmountPaid += Paid;

						Invoice.AmountPaid += Paid;

						Log.Informational("Neuro-Credits™ partially paid.", Invoice.InvoiceNumber.ToString(), PI.Jid, "InvoicePartialPayment", Invoice.GetTags());
					}

					await Database.Update(Invoice);

					if (AmountLeft <= 0)
						break;
				}
			}

			if (!(PaidPersonalInvoices is null) || !(PaidOrganizationalInvoices is null))
			{
				BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();
				string AccountName = XmppClient.GetAccount(PI.Jid);
				string EMail = await GetEMail(AccountName);

				if (!(PaidPersonalInvoices is null))
					await SendCancellations(PaidPersonalInvoices, EMail, false, BillingConfiguration);

				if (!(PaidOrganizationalInvoices is null))
					await SendCancellations(PaidOrganizationalInvoices, EMail, true, BillingConfiguration);
			}

			return new PaymentResult(AmountPaid, Currency);
		}

		private static void Add(ref LinkedList<Invoice> Invoices, Invoice Invoice)
		{
			Invoices ??= new LinkedList<Invoice>();
			Invoices.AddLast(Invoice);
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
			string SuccessUrl, string FailureUrl, string CancelUrl, EventHandlerAsync<ClientUrlEventArgs> ClientUrlCallback, object State)
		{
			ServiceConfiguration ServiceConfiguration = await ServiceConfiguration.GetCurrent();
			if (!ServiceConfiguration.IsWellDefined)
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

		#region Reminders

		internal static async Task SendReminders()
		{
			Dictionary<CaseInsensitiveString, LinkedList<Invoice>> PersonalByAccount = null;
			Dictionary<CaseInsensitiveString, LinkedList<Invoice>> OrganizationalByAccount = null;

			foreach (Invoice Invoice in await Database.Find<Invoice>(new FilterAnd(
				new FilterFieldEqualTo("IsPaid", false),
				new FilterFieldLesserThan("DueDate", DateTime.Today.AddDays(-1)))))
			{
				if (Invoice.IsOrganizational)
					AddReminder(ref OrganizationalByAccount, Invoice);
				else
					AddReminder(ref PersonalByAccount, Invoice);
			}

			await SendReminders(PersonalByAccount);
			await SendReminders(OrganizationalByAccount);
		}

		private static void AddReminder(ref Dictionary<CaseInsensitiveString, LinkedList<Invoice>> ByAccount, Invoice Invoice)
		{
			ByAccount ??= new Dictionary<CaseInsensitiveString, LinkedList<Invoice>>();

			if (!ByAccount.TryGetValue(Invoice.Account, out LinkedList<Invoice> Invoices))
			{
				Invoices = new LinkedList<Invoice>();
				ByAccount[Invoice.Account] = Invoices;
			}

			Invoices.AddLast(Invoice);
		}

		private static async Task SendReminders(Dictionary<CaseInsensitiveString, LinkedList<Invoice>> ByAccount)
		{
			if (!(ByAccount is null))
			{
				BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();

				foreach (KeyValuePair<CaseInsensitiveString, LinkedList<Invoice>> P in ByAccount)
				{
					CaseInsensitiveString AccountName = P.Key;
					LinkedList<Invoice> Invoices = P.Value;
					DateTime TP = DateTime.UtcNow;

					try
					{
						foreach (Invoice Invoice in Invoices)
						{
							Invoice.LateFees += Math.Ceiling(Invoice.AmountLeft * Invoice.PeriodInterest / 100) + Invoice.InvoiceFee;
							Invoice.DueDate += Invoice.Period;
							Invoice.InvoiceDate = TP;
							Invoice.LastReminder = TP;
							Invoice.NrReminders++;
						}

						await Database.Update(Invoices);

						string EMail = await GetEMail(AccountName);
						await SendInvoices(Invoices, EMail, Invoices.First.Value.IsOrganizational, BillingConfiguration);
					}
					catch (Exception ex)
					{
						Log.Critical(ex, AccountName);
					}
				}
			}
		}

		internal static async Task ResendInvoice(Invoice Invoice)
		{
			BillingConfiguration BillingConfiguration = await BillingConfiguration.GetCurrent();
			CaseInsensitiveString AccountName = Invoice.Account;
			string EMail = await GetEMail(AccountName);
			await SendInvoices(new Invoice[] { Invoice }, EMail, Invoice.IsOrganizational, BillingConfiguration);
		}

		#endregion

		#region Payments

		/// <summary>
		/// Callback method that the checkout will call when a process for buying eDaler has concluded.
		/// </summary>
		/// <param name="Result">Result of eDaler-purchase attempt.</param>
		/// <param name="State">State object passed on to from checkout.</param>
		public static async Task PaymentReceived(PaymentResult Result, object State)
		{
			if (!Result.Ok)
				return;

			if (!int.TryParse(State?.ToString() ?? string.Empty, out int InvoiceNumber))
			{
				Log.Warning("A payment for an invoice has been made. But invoice reference is not a valid invoice number.",
					string.Empty, string.Empty, "InvoiceNotFound",
					new KeyValuePair<string, object>("Invoice", State),
					new KeyValuePair<string, object>("Amount", Result.Amount),
					new KeyValuePair<string, object>("Currency", Result.Currency));

				return;
			}

			Invoice Invoice = await Database.FindFirstIgnoreRest<Invoice>(new FilterFieldEqualTo("InvoiceNumber", InvoiceNumber));
			if (Invoice is null)
			{
				Log.Warning("A payment for an invoice has been made. But no such invoice was found.",
					string.Empty, string.Empty, "InvoiceNotFound",
					new KeyValuePair<string, object>("Invoice", InvoiceNumber),
					new KeyValuePair<string, object>("Amount", Result.Amount),
					new KeyValuePair<string, object>("Currency", Result.Currency));

				return;
			}

			if (Invoice.Currency != Result.Currency)
			{
				Log.Warning("A payment for an invoice has been made, in a different currency than the currency used by the invoice.",
					string.Empty, string.Empty, "InvoiceCurrencyMismatch",
					new KeyValuePair<string, object>("Invoice", InvoiceNumber),
					new KeyValuePair<string, object>("Amount", Result.Amount),
					new KeyValuePair<string, object>("Currency", Result.Currency));

				return;
			}

			decimal AmountLeft = Result.Amount;
			decimal Paid;
			decimal AmountPaid = 0;

			if (AmountLeft >= Invoice.AmountLeft)
			{
				if (Invoice.IsOrganizational)
					Paid = await ServiceConfiguration.DecrementOrganizationalDebt(Invoice.AmountLeft, Invoice.OrganizationNumber, Invoice.OrganizationCountry);
				else
					Paid = await ServiceConfiguration.DecrementPersonalDebt(Invoice.AmountLeft, Invoice.PersonalNumber, Invoice.Country);

				AmountLeft -= Paid;
				AmountPaid += Paid;

				Invoice.AmountPaid += Paid;
				Invoice.IsPaid = true;
				Invoice.Paid = DateTime.UtcNow;

				Log.Notice("Neuro-Credits™ cancelled.", Invoice.InvoiceNumber.ToString(), string.Empty, "InvoiceCancelled", Invoice.GetTags());
			}
			else
			{
				if (Invoice.IsOrganizational)
					Paid = await ServiceConfiguration.DecrementOrganizationalDebt(AmountLeft, Invoice.OrganizationNumber, Invoice.OrganizationCountry);
				else
					Paid = await ServiceConfiguration.DecrementPersonalDebt(AmountLeft, Invoice.PersonalNumber, Invoice.Country);

				AmountLeft = 0;
				AmountPaid += Paid;

				Invoice.AmountPaid += Paid;

				Log.Informational("Neuro-Credits™ partially paid.", Invoice.InvoiceNumber.ToString(), string.Empty, "InvoicePartialPayment", Invoice.GetTags());
			}

			await Database.Update(Invoice);

			string[] TabIDs = ClientEvents.GetTabIDsForLocation("/NeuroCredits/PayInvoice.md",
				new KeyValuePair<string, string>("Nr", InvoiceNumber.ToString()));

			if (TabIDs.Length > 0)
				await ClientEvents.PushEvent(TabIDs, "Reload", string.Empty);

			TabIDs = ClientEvents.GetTabIDsForLocation("/NeuroCredits/Invoice.md",
				new KeyValuePair<string, string>("Nr", InvoiceNumber.ToString()));

			if (TabIDs.Length > 0)
				await ClientEvents.PushEvent(TabIDs, "Reload", string.Empty);

			if (AmountLeft > 0)
			{
				Log.Warning("A payment for an invoice has been made. The amount exceeded the amount of the invoice.",
					string.Empty, string.Empty, "InvoiceAmountExceeded",
					new KeyValuePair<string, object>("Invoice", InvoiceNumber),
					new KeyValuePair<string, object>("TotalAmount", Result.Amount),
					new KeyValuePair<string, object>("Currency", Result.Currency),
					new KeyValuePair<string, object>("InvoicePayment", AmountPaid),
					new KeyValuePair<string, object>("AmountLeft", AmountLeft));
			}
		}

		#endregion

	}
}
