using System.Threading.Tasks;
using Waher.IoTGateway;
using Waher.Networking.XMPP;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Persistence;
using Waher.Runtime.Settings;
using Waher.Runtime.Counters;

namespace TAG.Payments.NeuroCredits
{
	/// <summary>
	/// Contains the service configuration.
	/// </summary>
	public class ServiceConfiguration
	{
		private static ServiceConfiguration current = null;

		/// <summary>
		/// Contains the service configuration.
		/// </summary>
		public ServiceConfiguration()
		{
		}

		/// <summary>
		/// List of approved countries
		/// </summary>
		public string[] Countries
		{
			get;
			private set;
		}

		/// <summary>
		/// List of approved currencies
		/// </summary>
		public string[] Currencies
		{
			get;
			private set;
		}

		/// <summary>
		/// If private persons are allowed to use the service.
		/// </summary>
		public bool AllowPrivatePersons
		{
			get;
			private set;
		}

		/// <summary>
		/// If organizations are allowed to use the service.
		/// </summary>
		public bool AllowOrganizations
		{
			get;
			private set;
		}

		/// <summary>
		/// If configuration is well-defined.
		/// </summary>
		public bool IsWellDefined
		{
			get
			{
				return
					!(this.Countries is null) &&
					this.Countries.Length > 0 &&
					!(this.Currencies is null) &&
					this.Currencies.Length > 0;
			}
		}

		/// <summary>
		/// Loads configuration settings.
		/// </summary>
		/// <returns>Configuration settings.</returns>
		public static async Task<ServiceConfiguration> Load()
		{
			ServiceConfiguration Result = new ServiceConfiguration();
			string Prefix = NeuroCreditsServiceProvider.ServiceId;

			Result.Countries = (await RuntimeSettings.GetAsync(Prefix + ".Countries", string.Empty)).Split(',');
			Result.Currencies = (await RuntimeSettings.GetAsync(Prefix + ".Currencies", string.Empty)).Split(',');
			Result.AllowPrivatePersons = await RuntimeSettings.GetAsync(Prefix + ".PrivatePersons", false);
			Result.AllowOrganizations = await RuntimeSettings.GetAsync(Prefix + ".Organizations", false);

			return Result;
		}

		/// <summary>
		/// Gets the current configuration.
		/// </summary>
		/// <returns>Configuration</returns>
		public static async Task<ServiceConfiguration> GetCurrent()
		{
			if (current is null)
				current = await Load();

			return current;
		}

		/// <summary>
		/// Invalidates the current configuration, triggering a reload of the
		/// configuration for the next operation.
		/// </summary>
		public static void InvalidateCurrent()
		{
			current = null;
		}

		/// <summary>
		/// Default currency of broker.
		/// </summary>
		/// <returns></returns>
		public static Task<string> GetDefaultCurrency()
		{
			return RuntimeSettings.GetAsync("DefaultCurrency", string.Empty);
		}

		/// <summary>
		/// Gets the currency used by the wallet of a given account.
		/// </summary>
		/// <param name="AccountName">Name of account.</param>
		/// <returns>Currency of wallet.</returns>
		public static async Task<string> GetCurrencyOfAccount(string AccountName)
		{
			// select generic top 1 Currency from "eDalerWallets" where Account=AccountName

			GenericObject Wallet = null;

			foreach (GenericObject Obj in await Database.Find<GenericObject>("eDalerWallets", 0, 1,
				new FilterFieldEqualTo("Account", AccountName)))
			{
				Wallet = Obj;
				break;
			}

			if (Wallet is null)
				return null;

			if (!Wallet.TryGetFieldValue("Currency", out object Currency))
				return null;

			return Currency?.ToString();
		}

		/// <summary>
		/// Checks if a person is authorized to buy Neuro-Credits™.
		/// </summary>
		/// <param name="Jid">JID of account.</param>
		/// <param name="PersonalNumber">Personal number of account.</param>
		/// <param name="Currency">Currency</param>
		/// <returns>Amount authorized, using the default currency of the broker.</returns>
		public async Task<double> IsPersonAuthorized(string Jid, string PersonalNumber, string Currency)
		{
			if (string.IsNullOrEmpty(Jid) || string.IsNullOrEmpty(PersonalNumber))
				return 0;

			string Domain = XmppClient.GetDomain(Jid);
			if (!string.IsNullOrEmpty(Domain) && !Gateway.IsDomain(Domain, true))
				return 0;

			if (!this.SupportsCurrency(Currency))
				return 0;

			string Account = XmppClient.GetAccount(Jid);
			string WalletCurrency = await GetCurrencyOfAccount(Account);
			if (string.IsNullOrEmpty(WalletCurrency) || WalletCurrency != Currency)
				return 0;

			double Amount = await RuntimeSettings.GetAsync(PersonalKey(Account, PersonalNumber), 0d);
			decimal CurrentDebt = await CurrentPersonalDebt(Jid, PersonalNumber);

			Amount -= (double)CurrentDebt;

			return Amount;
		}

		private static string PersonalKey(string Account, string PersonalNumber)
		{
			return NeuroCreditsServiceProvider.ServiceId + ".Accounts." + Account + "." + PersonalNumber;
		}

		/// <summary>
		/// Gets the current personal debt.
		/// </summary>
		/// <param name="Jid">JID of account.</param>
		/// <param name="PersonalNumber">Personal number of account.</param>
		/// <returns>Current personal debt, using the default currency of the broker.</returns>
		public static async Task<decimal> CurrentPersonalDebt(string Jid, string PersonalNumber)
		{
			string Account = XmppClient.GetAccount(Jid);
			long Count = await RuntimeCounters.GetCount(PersonalKey(Account, PersonalNumber));

			return 0.0001M * Count;
		}

		internal static async Task<decimal> IncrementPersonalDebt(decimal Amount, string Jid, string PersonalNumber)
		{
			string Account = XmppClient.GetAccount(Jid);
			long CountDelta = (long)(Amount * 10000);

			await RuntimeCounters.IncrementCounter(PersonalKey(Account, PersonalNumber), CountDelta);

			return 0.0001M * CountDelta;
		}

		internal static async Task<decimal> DecrementPersonalDebt(decimal Amount, string Jid, string PersonalNumber)
		{
			string Account = XmppClient.GetAccount(Jid);
			long CountDelta = (long)(Amount * 10000);

			await RuntimeCounters.DecrementCounter(PersonalKey(Account, PersonalNumber), CountDelta);

			return 0.0001M * CountDelta;
		}

		/// <summary>
		/// Checks if a person is authorized to buy Neuro-Credits™.
		/// </summary>
		/// <param name="Jid">JID of account.</param>
		/// <param name="OrganizationNumber">Organization number.</param>
		/// <param name="PersonalNumber">Personal number of person representing the organization.</param>
		/// <param name="Currency">Currency</param>
		/// <returns>Amount authorized, using the default currency of the broker.</returns>
		public async Task<double> IsOrganizationAuthorized(string Jid, string OrganizationNumber, string PersonalNumber, string Currency)
		{
			if (string.IsNullOrEmpty(Jid) || string.IsNullOrEmpty(OrganizationNumber) || string.IsNullOrEmpty(PersonalNumber))
				return 0;

			string Domain = XmppClient.GetDomain(Jid);
			if (!Gateway.IsDomain(Domain, true))
				return 0;

			if (!this.SupportsCurrency(Currency))
				return 0;

			string Account = XmppClient.GetAccount(Jid);
			string WalletCurrency = await GetCurrencyOfAccount(Account);
			if (string.IsNullOrEmpty(WalletCurrency) || WalletCurrency != Currency)
				return 0;

			double Amount = await RuntimeSettings.GetAsync(OrganizationKey(OrganizationNumber, PersonalNumber), 0d);
			decimal CurrentDebt = await CurrentOrganizationDebt(OrganizationNumber, PersonalNumber);

			Amount -= (double)CurrentDebt;

			return Amount;
		}

		private static string OrganizationKey(string OrganizationNumber, string PersonalNumber)
		{
			return NeuroCreditsServiceProvider.ServiceId + ".Organization." + OrganizationNumber + "." + PersonalNumber;
		}

		/// <summary>
		/// Gets the current organization debt.
		/// </summary>
		/// <param name="OrganizationNumber">Organization number.</param>
		/// <param name="PersonalNumber">Personal number of account.</param>
		/// <returns>Current personal debt, using the default currency of the broker.</returns>
		public static async Task<decimal> CurrentOrganizationDebt(string OrganizationNumber, string PersonalNumber)
		{
			long Count = await RuntimeCounters.GetCount(OrganizationKey(OrganizationNumber, PersonalNumber));

			return 0.0001M * Count;
		}

		/// <summary>
		/// Checks if a currency is supported.
		/// </summary>
		/// <param name="Currency">Currency</param>
		/// <returns>If currency is supported.</returns>
		public bool SupportsCurrency(CaseInsensitiveString Currency)
		{
			if (!this.IsWellDefined)
				return false;

			foreach (string s in this.Currencies)
			{
				if (s.Trim().ToLower() == Currency.LowerCase)
					return true;
			}

			return false;
		}
	}
}
