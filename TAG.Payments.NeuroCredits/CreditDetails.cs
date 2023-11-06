using System;
using TAG.Payments.NeuroCredits.Configuration;
using TAG.Payments.NeuroCredits.Data;
using Waher.Content;

namespace TAG.Payments.NeuroCredits
{
    /// <summary>
    /// Contains credit-detail information for a user.
    /// </summary>
    internal class CreditDetails
	{
		/// <summary>
		/// If credit is authorized
		/// </summary>
		public bool HasCredit { get; set; }

		/// <summary>
		/// Maximum spot credit authorized.
		/// </summary>
		public decimal MaxCredit { get; set; }

		/// <summary>
		/// Current debt
		/// </summary>
		public decimal Debt { get; set; }

		/// <summary>
		/// Currency of <see cref="MaxCredit"/> and <see cref="Debt"/>
		/// </summary>
		public string Currency { get; set; }

		/// <summary>
		/// Account configuration, if personal credit.
		/// </summary>
		public AccountConfiguration AccountConfiguration { get; set; }

		/// <summary>
		/// Organization configuration, if organizational credit.
		/// </summary>
		public OrganizationConfiguration OrganizationConfiguration { get; set; }

		/// <summary>
		/// Generic configuration.
		/// </summary>
		public ServiceConfiguration Configuration { get; set; }

		/// <summary>
		/// If the credit details represents a personal credit.
		/// </summary>
		public bool PersonalCredit => !(this.AccountConfiguration is null);

		/// <summary>
		/// If the credit details represents an organizational credit.
		/// </summary>
		public bool OrganizationalCredit => !(this.OrganizationConfiguration is null);

		/// <summary>
		/// Period of credit.
		/// </summary>
		public Duration Period
		{
			get
			{
				return
					this.OrganizationConfiguration?.Period ??
					this.AccountConfiguration?.Period ??
					this.Configuration.Period;
			}
		}

		/// <summary>
		/// Period interest of credit.
		/// </summary>
		public decimal PeriodInterest
		{
			get
			{
				return
					this.OrganizationConfiguration?.PeriodInterest ??
					this.AccountConfiguration?.PeriodInterest ??
					this.Configuration.PeriodInterest;
			}
		}

		/// <summary>
		/// Maximum number of installments for credit.
		/// </summary>
		public int MaxInstallments
		{
			get
			{
				return
					this.OrganizationConfiguration?.MaxInstallments ??
					this.AccountConfiguration?.MaxInstallments ??
					this.Configuration.MaxInstallments;
			}
		}

		/// <summary>
		/// Initial due-date.
		/// </summary>
		public DateTime InitialDueDate => DateTime.Today.AddDays(1) + this.Period;
	}
}
