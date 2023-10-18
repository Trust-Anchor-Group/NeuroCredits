using System;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace TAG.Payments.NeuroCredits.Data
{
	/// <summary>
	/// Represents an invoice.
	/// </summary>
	[CollectionName("NeuroCreditInvoices")]
	[TypeName(TypeNameSerialization.None)]
	[Index("InvoiceNumber")]
	[Index("DueDate", "InvoiceNumber")]
	[Index("Account", "Paid", "InvoiceNumber")]
	[Index("Account", "Paid", "Amount")]
	public class Invoice
	{
		/// <summary>
		/// Represents an invoice.
		/// </summary>
		public Invoice()
		{
		}

		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Invoice count.
		/// </summary>
		public long InvoiceNumber { get; set; }

		/// <summary>
		/// Account name.
		/// </summary>
		public string Account { get; set; }

		/// <summary>
		/// If invoice has been paid.
		/// </summary>
		public bool IsPaid { get; set; }

		/// <summary>
		/// Amount
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Currency
		/// </summary>
		public CaseInsensitiveString Currency { get; set; }

		/// <summary>
		/// When invoice is due
		/// </summary>
		public DateTime DueDate { get; set; }

		/// <summary>
		/// When invoice was created
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When invoice was paid
		/// </summary>
		public DateTime Paid { get; set; }

		/// <summary>
		/// Contract ID of when Neuro-Credits™ was bought.
		/// </summary>
		public string NeuroCreditsContractId { get; set; }

		/// <summary>
		/// Contract ID of when eDaler® was sold to cancel Neuro-Credits™.
		/// </summary>
		public string CancellationContractId { get; set; }

		/// <summary>
		/// A reference if Neuro-Credits™ was cancelled from an external source.
		/// </summary>
		public string ExternalReference { get; set; }

		/// <summary>
		/// First name of person
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Middle name of person
		/// </summary>
		public string MiddleName { get; set; }

		/// <summary>
		/// Last name of person
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// Personal Number
		/// </summary>
		public string PersonalNumber { get; set; }

		/// <summary>
		/// Address (line 1) of person
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// Address (line 2) of person
		/// </summary>
		public string Address2 { get; set; }

		/// <summary>
		/// Area of person
		/// </summary>
		public string Area { get; set; }

		/// <summary>
		/// City of person
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// Postal of person
		/// </summary>
		public string PostalCode { get; set; }

		/// <summary>
		/// Region of person
		/// </summary>
		public string Region { get; set; }

		/// <summary>
		/// Country of person
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// JID of person
		/// </summary>
		public string Jid { get; set; }

		/// <summary>
		/// Phone number of person
		/// </summary>
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Organization Name
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Department of person in organization
		/// </summary>
		public string Department { get; set; }

		/// <summary>
		/// Role of person in organization
		/// </summary>
		public string Role { get; set; }

		/// <summary>
		/// Organization number
		/// </summary>
		public string OrganizationNumber { get; set; }

		/// <summary>
		/// Address (line 1) of organization
		/// </summary>
		public string OrganizationAddress { get; set; }

		/// <summary>
		/// Address (line 2) of organization
		/// </summary>
		public string OrganizationAddress2 { get; set; }

		/// <summary>
		/// Area of organization
		/// </summary>
		public string OrganizationArea { get; set; }

		/// <summary>
		/// City of organization
		/// </summary>
		public string OrganizationCity { get; set; }

		/// <summary>
		/// Postal code of organization
		/// </summary>
		public string OrganizationPostalCode { get; set; }

		/// <summary>
		/// Region of organization
		/// </summary>
		public string OrganizationRegion { get; set; }

		/// <summary>
		/// Country of organization
		/// </summary>
		public string OrganizationCountry { get; set; }
	}
}
