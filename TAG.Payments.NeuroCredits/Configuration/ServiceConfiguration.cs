using System.Threading.Tasks;
using Waher.IoTGateway;
using Waher.Networking.XMPP;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Persistence;
using Waher.Runtime.Settings;
using Waher.Runtime.Counters;
using Waher.Content;
using TAG.Payments.NeuroCredits.Data;
using System;

namespace TAG.Payments.NeuroCredits.Configuration
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
        /// Default max limit for persons.
        /// </summary>
        public decimal DefaultMaxPersonalLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// Default max limit for organizations.
        /// </summary>
        public decimal DefaultMaxOrganizationalLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// Fixed invoice fee.
        /// </summary>
        public decimal InvoiceFee
        {
            get;
            private set;
        }

        /// <summary>
        /// Invoices must be paid within this period.
        /// </summary>
        public Duration Period
        {
            get;
            private set;
        }

        /// <summary>
        /// After the due date, this interest rate will be added to the debt.
        /// </summary>
        public decimal PeriodInterest
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of installments.
        /// </summary>
        public int MaxInstallments
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
                    this.Currencies.Length > 0 &&
                    this.PeriodInterest > 0 &&
                    this.MaxInstallments > 1 &&
                    this.InvoiceFee >= 0 &&
                    this.Period.Seconds == 0 &&
                    this.Period.Minutes == 0 &&
                    this.Period.Hours == 0 &&
                    (this.Period.Days > 0 ||
                    this.Period.Months > 0 ||
					this.Period.Years > 0);
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
            Result.DefaultMaxPersonalLimit = (decimal)await RuntimeSettings.GetAsync(Prefix + ".DefaultPersonalLimit", 0d);
            Result.DefaultMaxOrganizationalLimit = (decimal)await RuntimeSettings.GetAsync(Prefix + ".DefaultOrganizationalLimit", 0d);
            Result.PeriodInterest = (decimal)await RuntimeSettings.GetAsync(Prefix + ".PeriodInterest", 2d);
            Result.InvoiceFee = (decimal)await RuntimeSettings.GetAsync(Prefix + ".InvoiceFee", 0d);
            Result.MaxInstallments = (int)await RuntimeSettings.GetAsync(Prefix + ".MaxInstallments", 6d);

            if (Duration.TryParse(await RuntimeSettings.GetAsync(Prefix + ".Period", "P1M"), out Duration D))
                Result.Period = D;
            else
                Result.Period = new Duration(false, 0, 1, 0, 0, 0, 0);

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
        /// <param name="Country">Country of person</param>
        /// <param name="Currency">Currency</param>
        /// <returns>Amount left authorized, current debt, using the default currency of the broker.</returns>
        public async Task<(decimal, decimal, AccountConfiguration)> IsPersonAuthorized(string Jid, string PersonalNumber, string Country, string Currency)
        {
            if (string.IsNullOrEmpty(Jid) || string.IsNullOrEmpty(PersonalNumber))
                return (0, 0, null);

            string Domain = XmppClient.GetDomain(Jid);
            if (!string.IsNullOrEmpty(Domain) && !Gateway.IsDomain(Domain, true))
                return (0, 0, null);

            if (!this.SupportsCountry(Country))
                return (0, 0, null);

            if (!this.SupportsCurrency(Currency))
                return (0, 0, null);

            string Account = XmppClient.GetAccount(Jid);
            string WalletCurrency = await GetCurrencyOfAccount(Account);
            if (string.IsNullOrEmpty(WalletCurrency) || WalletCurrency != Currency)
                return (0, 0, null);

            AccountConfiguration Configuration = await Database.FindFirstIgnoreRest<AccountConfiguration>(
                new FilterFieldEqualTo("Account", Account));

            decimal Amount;

            if (Configuration is null)
            {
                if (this.AllowPrivatePersons)
                    Amount = this.DefaultMaxPersonalLimit;
                else
                    return (0, 0, null);
            }
            else
                Amount = Configuration.MaxCredit;

            decimal CurrentDebt = await CurrentPersonalDebt(PersonalNumber, Country);

            Amount -= CurrentDebt;

            return (Amount, CurrentDebt, Configuration);
        }

        /// <summary>
        /// Gets the current personal debt.
        /// </summary>
        /// <param name="Jid">JID of account.</param>
        /// <param name="PersonalNumber">Personal number of account.</param>
        /// <param name="Country">Country of person.</param>
        /// <returns>Current personal debt, using the default currency of the broker.</returns>
        public static async Task<decimal> CurrentPersonalDebt(string PersonalNumber, string Country)
        {
            long Count = await RuntimeCounters.GetCount(PersonalDebtKey(PersonalNumber, Country));

            return 0.0001M * Count;
        }

        private static string PersonalDebtKey(string PersonalNumber, string Country)
        {
            return NeuroCreditsServiceProvider.ServiceId + ".Person." + Country + "." + PersonalNumber;
        }

        internal static async Task<decimal> IncrementPersonalDebt(decimal Amount, string PersonalNumber, string Country)
        {
            long CountDelta = (long)(Amount * 10000);

            await RuntimeCounters.IncrementCounter(PersonalDebtKey(PersonalNumber, Country), CountDelta);

            return 0.0001M * CountDelta;
        }

        internal static async Task<decimal> DecrementPersonalDebt(decimal Amount, string PersonalNumber, string Country)
        {
            long CountDelta = (long)(Amount * 10000);

            await RuntimeCounters.DecrementCounter(PersonalDebtKey(PersonalNumber, Country), CountDelta);

            return 0.0001M * CountDelta;
        }

        /// <summary>
        /// Checks if a person is authorized to buy Neuro-Credits™.
        /// </summary>
        /// <param name="Jid">JID of account.</param>
        /// <param name="OrganizationNumber">Organization number.</param>
        /// <param name="OrganizationCountry">Country of organization.</param>
        /// <param name="PersonalNumber">Personal number of person representing the organization.</param>
        /// <param name="PersonCountry">Country of person.</param>
        /// <param name="Currency">Currency</param>
        /// <returns>Amount left authorized, current debt, using the default currency of the broker.</returns>
        public async Task<(decimal, decimal, OrganizationConfiguration)> IsOrganizationAuthorized(string Jid, string OrganizationNumber,
            string OrganizationCountry, string PersonalNumber, string PersonCountry, string Currency)
        {
            if (string.IsNullOrEmpty(Jid) || string.IsNullOrEmpty(OrganizationNumber) || string.IsNullOrEmpty(PersonalNumber))
                return (0, 0, null);

            string Domain = XmppClient.GetDomain(Jid);
            if (!string.IsNullOrEmpty(Domain) && !Gateway.IsDomain(Domain, true))
                return (0, 0, null);

            if (!this.SupportsCurrency(Currency))
                return (0, 0, null);

            string Account = XmppClient.GetAccount(Jid);
            string WalletCurrency = await GetCurrencyOfAccount(Account);
            if (string.IsNullOrEmpty(WalletCurrency) || WalletCurrency != Currency)
                return (0, 0, null);

            OrganizationConfiguration Configuration = await Database.FindFirstIgnoreRest<OrganizationConfiguration>(new FilterAnd(
                new FilterFieldEqualTo("OrganizationNumber", OrganizationNumber),
                new FilterFieldEqualTo("OrganizationCountry", OrganizationCountry)));

            decimal Amount;

            if (Configuration is null)
            {
                if (this.AllowOrganizations)
                    Amount = this.DefaultMaxOrganizationalLimit;
                else
                    return (0, 0, null);
            }
            else
            {
                Amount = Configuration.MaxCredit;

                if ((Configuration.PersonalNumbers?.Length ?? 0) > 0)
                {
                    int i = Array.IndexOf(Configuration.PersonalNumbers, PersonalNumber);

                    if (i < 0 || i >= (Configuration.PersonalCountries?.Length ?? 0))
                        return (0, 0, null);

                    if (PersonCountry.ToUpper() != Configuration.PersonalCountries[i].ToUpper())
                        return (0, 0, null);
                }
            }

            decimal CurrentDebt = await CurrentOrganizationDebt(OrganizationNumber, OrganizationCountry);

            Amount -= CurrentDebt;

            return (Amount, CurrentDebt, Configuration);
        }

        private static string OrganizationDebtKey(string OrganizationNumber, string OrganizationCountry)
        {
            return NeuroCreditsServiceProvider.ServiceId + ".Organization." + OrganizationCountry + "." + OrganizationNumber;
        }

        /// <summary>
        /// Gets the current organization debt.
        /// </summary>
        /// <param name="OrganizationNumber">Organization number.</param>
        /// <param name="OrganizationCountry">Country of organization.</param>
        /// <returns>Current personal debt, using the default currency of the broker.</returns>
        public static async Task<decimal> CurrentOrganizationDebt(string OrganizationNumber, string OrganizationCountry)
        {
            long Count = await RuntimeCounters.GetCount(OrganizationDebtKey(OrganizationNumber, OrganizationCountry));

            return 0.0001M * Count;
        }

        internal static async Task<decimal> IncrementOrganizationalDebt(decimal Amount, string OrganizationNumber, string OrganizationCountry)
        {
            long CountDelta = (long)(Amount * 10000);

            await RuntimeCounters.IncrementCounter(OrganizationDebtKey(OrganizationNumber, OrganizationCountry), CountDelta);

            return 0.0001M * CountDelta;
        }

        internal static async Task<decimal> DecrementOrganizationalDebt(decimal Amount, string OrganizationNumber, string OrganizationCountry)
        {
            long CountDelta = (long)(Amount * 10000);

            await RuntimeCounters.DecrementCounter(OrganizationDebtKey(OrganizationNumber, OrganizationCountry), CountDelta);

            return 0.0001M * CountDelta;
        }

        /// <summary>
        /// Checks if a country is supported.
        /// </summary>
        /// <param name="Country">Country</param>
        /// <returns>If country is supported.</returns>
        public bool SupportsCountry(CaseInsensitiveString Country)
        {
            if (!this.IsWellDefined)
                return false;

            foreach (string s in this.Countries)
            {
                if (s.Trim().ToLower() == Country.LowerCase)
                    return true;
            }

            return false;
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
