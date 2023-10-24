using System;
using Waher.Content;
using Waher.Persistence.Attributes;

namespace TAG.Payments.NeuroCredits.Data
{
	/// <summary>
	/// Individual account configuration. Such will supercede general permissions.
	/// </summary>
	[CollectionName("NeuroCreditAccounts")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Account")]
	[Index("PersonalNumber", "Country")]
	[ArchivingTime(3653)]
	public class AccountConfiguration
	{
		/// <summary>
		/// Individual account configuration. Such will supercede general permissions.
		/// </summary>
		public AccountConfiguration()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Account name.
		/// </summary>
		public string Account { get; set; }

		/// <summary>
		/// Personal Number
		/// </summary>
		public string PersonalNumber { get; set; }

		/// <summary>
		/// Country
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// Max credit awarded this account.
		/// </summary>
		public decimal MaxCredit { get; set; }

		/// <summary>
		/// Invoices must be paid within this period.
		/// </summary>
		public Duration Period { get; set; }

		/// <summary>
		/// When record was created.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When record was created.
		/// </summary>
		public DateTime Updated { get; set; }

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

	}
}
