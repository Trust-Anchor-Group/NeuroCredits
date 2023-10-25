using System;
using Waher.Content;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace TAG.Payments.NeuroCredits.Data
{
	/// <summary>
	/// Individual organization configuration. Such will supercede general permissions.
	/// </summary>
	[CollectionName("NeuroCreditOrganizations")]
	[TypeName(TypeNameSerialization.None)]
	[Index("OrganizationName")]
	[Index("OrganizationNumber", "OrganizationCountry")]
	[ArchivingTime(3653)]
	public class OrganizationConfiguration
	{
		/// <summary>
		/// Individual organization configuration. Such will supercede general permissions.
		/// </summary>
		public OrganizationConfiguration()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Organization name.
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Organization Number
		/// </summary>
		public string OrganizationNumber { get; set; }

		/// <summary>
		/// Organization Country
		/// </summary>
		public string OrganizationCountry { get; set; }

		/// <summary>
		/// Array of personal numbers authorized within the organization. If empty, all persons are authorized.
		/// </summary>
		public string[] PersonalNumbers { get; set; }

		/// <summary>
		/// Array of personal countries authorized within the organization (corresponding to the personal numbers). 
		/// If empty, all persons are authorized.
		/// </summary>
		public string[] PersonalCountries { get; set; }

		/// <summary>
		/// Max credit awarded this account.
		/// </summary>
		public decimal MaxCredit { get; set; }

		/// <summary>
		/// Currency of <see cref="MaxCredit"/>.
		/// </summary>
		public CaseInsensitiveString Currency { get; set; }

		/// <summary>
		/// Invoices must be paid within this period.
		/// </summary>
		public Duration Period { get; set; }

		/// <summary>
		/// After the due date, this interest rate will be added to the debt.
		/// </summary>
		public decimal PeriodInterest { get; set; }

		/// <summary>
		/// Number of installments.
		/// </summary>
		public int MaxInstallments { get; set; }

		/// <summary>
		/// When record was created.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When record was created.
		/// </summary>
		public DateTime Updated { get; set; }
	}
}
