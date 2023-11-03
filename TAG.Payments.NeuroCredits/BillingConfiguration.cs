using System.Threading.Tasks;
using Waher.Runtime.Settings;

namespace TAG.Payments.NeuroCredits
{
	/// <summary>
	/// Contains the billing configuration.
	/// </summary>
	public class BillingConfiguration
	{
		private static BillingConfiguration current = null;

		/// <summary>
		/// Contains the billing configuration.
		/// </summary>
		public BillingConfiguration()
		{
		}

		/// <summary>
		/// Sender of invoices
		/// </summary>
		public string Sender
		{
			get;
			private set;
		}

		/// <summary>
		/// Tax or VAT number of sender
		/// </summary>
		public string TaxNr
		{
			get;
			private set;
		}

		/// <summary>
		/// Contact e-mail address
		/// </summary>
		public string ContactEMail
		{
			get;
			private set;
		}

		/// <summary>
		/// Contact phone number
		/// </summary>
		public string ContactPhone
		{
			get;
			private set;
		}

		/// <summary>
		/// Bank account (local/bank/national format)
		/// </summary>
		public string BankAccount
		{
			get;
			private set;
		}

		/// <summary>
		/// IBAN account number
		/// </summary>
		public string Iban
		{
			get;
			private set;
		}

		/// <summary>
		/// Bank account holder's name
		/// </summary>
		public string BankAccountName
		{
			get;
			private set;
		}

		/// <summary>
		/// Bank Identifier (BIC or SWIFT)
		/// </summary>
		public string BankIdentifier
		{
			get;
			private set;
		}

		/// <summary>
		/// Bank name
		/// </summary>
		public string BankName
		{
			get;
			private set;
		}

		/// <summary>
		/// Branch address
		/// </summary>
		public string BranchAddress
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
					!string.IsNullOrEmpty(this.Sender) &&
					!string.IsNullOrEmpty(this.TaxNr) &&
					!string.IsNullOrEmpty(this.ContactEMail) &&
					!string.IsNullOrEmpty(this.BankAccount) &&
					!string.IsNullOrEmpty(this.BankAccountName) &&
					!string.IsNullOrEmpty(this.BankIdentifier) &&
					!string.IsNullOrEmpty(this.BankName) &&
					!string.IsNullOrEmpty(this.BranchAddress);
			}
		}

		/// <summary>
		/// Loads configuration settings.
		/// </summary>
		/// <returns>Configuration settings.</returns>
		public static async Task<BillingConfiguration> Load()
		{
			BillingConfiguration Result = new BillingConfiguration();
			string Prefix = NeuroCreditsServiceProvider.ServiceId;

			Result.Sender = await RuntimeSettings.GetAsync(Prefix + ".Billing.Sender", string.Empty);
			Result.TaxNr = await RuntimeSettings.GetAsync(Prefix + ".Billing.TaxNr", string.Empty);
			Result.ContactEMail = await RuntimeSettings.GetAsync(Prefix + ".Billing.ContactEMail", string.Empty);
			Result.ContactPhone = await RuntimeSettings.GetAsync(Prefix + ".Billing.ContactPhone", string.Empty);
			Result.BankAccount = await RuntimeSettings.GetAsync(Prefix + ".Billing.BankAccount", string.Empty);
			Result.Iban = await RuntimeSettings.GetAsync(Prefix + ".Billing.Iban", string.Empty);
			Result.BankAccountName = await RuntimeSettings.GetAsync(Prefix + ".Billing.BankAccountName", string.Empty);
			Result.BankIdentifier = await RuntimeSettings.GetAsync(Prefix + ".Billing.BankIdentifier", string.Empty);
			Result.BankName = await RuntimeSettings.GetAsync(Prefix + ".Billing.BankName", string.Empty);
			Result.BranchAddress = await RuntimeSettings.GetAsync(Prefix + ".Billing.BranchAddress", string.Empty);

			return Result;
		}

		/// <summary>
		/// Gets the current configuration.
		/// </summary>
		/// <returns>Configuration</returns>
		public static async Task<BillingConfiguration> GetCurrent()
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
	}
}
