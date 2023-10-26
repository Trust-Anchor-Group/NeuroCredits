using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Script;

namespace TAG.Payments.NeuroCredits.Data
{
	/// <summary>
	/// Represents an invoice.
	/// </summary>
	[CollectionName("NeuroCreditInvoices")]
	[TypeName(TypeNameSerialization.None)]
	[Index("InvoiceNumber")]
	[Index("IsPaid", "DueDate", "InvoiceNumber")]
	[Index("PersonalNumber", "Country", "IsPaid", "InvoiceNumber")]
	[Index("OrganizationNumber", "OrganizationCountry", "IsPaid", "InvoiceNumber")]
	[ObsoleteMethod(nameof(UpgradePropertyValue))]
	[ArchivingTime(3653)]
	public class Invoice
	{
		/// <summary>
		/// Represents an invoice.
		/// </summary>
		public Invoice()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
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
		/// Any late fees.
		/// </summary>
		public decimal LateFees { get; set; }

		/// <summary>
		/// Amount that has been paid.
		/// </summary>
		public decimal AmountPaid { get; set; }

		/// <summary>
		/// Total amount (<see cref="Amount"/> + <see cref="LateFees"/>).
		/// </summary>
		public decimal TotalAmount => this.Amount + this.LateFees;

		/// <summary>
		/// Amount left to pay (<see cref="TotalAmount"/> - <see cref="AmountPaid"/>).
		/// </summary>
		public decimal AmountLeft => this.TotalAmount - this.AmountPaid;

		/// <summary>
		/// Number of reminders sent.
		/// </summary>
		public int NrReminders { get; set; }

		/// <summary>
		/// Currency
		/// </summary>
		public CaseInsensitiveString Currency { get; set; }

		/// <summary>
		/// When invoice is due
		/// </summary>
		public DateTime DueDate { get; set; }

		/// <summary>
		/// Invoice period, on which the <see cref="DueDate"/> is calculated.
		/// </summary>
		public Duration Period { get; set; }

		/// <summary>
		/// Due interest
		/// </summary>
		public decimal PeriodInterest { get; set; }

		/// <summary>
		/// Installment number.
		/// </summary>
		public int Installment { get; set; }

		/// <summary>
		/// Number of installments made for credit.
		/// </summary>
		public int NrInstallments { get; set; }

		/// <summary>
		/// When invoice was created
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When invoice was paid
		/// </summary>
		public DateTime Paid { get; set; }

		/// <summary>
		/// When invoice was sent.
		/// </summary>
		public DateTime InvoiceDate { get; set; }

		/// <summary>
		/// When last reminder was sent.
		/// </summary>
		public DateTime LastReminder { get; set; }

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

		/// <summary>
		/// Gets invoice-related tags.
		/// </summary>
		/// <returns>Array of tags.</returns>
		public KeyValuePair<string, object>[] GetTags()
		{
			List<KeyValuePair<string, object>> Result = new List<KeyValuePair<string, object>>
			{
				new KeyValuePair<string, object>("InvoiceNumber", this.InvoiceNumber),
				new KeyValuePair<string, object>("Account", this.Account),
				new KeyValuePair<string, object>("IsPaid", this.IsPaid),
				new KeyValuePair<string, object>("Amount", this.Amount),
				new KeyValuePair<string, object>("LateFees", this.LateFees),
				new KeyValuePair<string, object>("TotalAmount", this.TotalAmount),
				new KeyValuePair<string, object>("AmountLeft", this.AmountLeft),
				new KeyValuePair<string, object>("Currency", this.Currency),
				new KeyValuePair<string, object>("DueDate", this.DueDate),
				new KeyValuePair<string, object>("Period", this.Period.ToString()),
				new KeyValuePair<string, object>("PeriodInterest", this.PeriodInterest),
				new KeyValuePair<string, object>("Created", this.Created)
			};

			if (this.NrInstallments > 1)
			{
				Result.Add(new KeyValuePair<string, object>("Installment", this.Installment));
				Result.Add(new KeyValuePair<string, object>("NrInstallments", this.NrInstallments));
			}

			if (this.Paid > DateTime.MinValue)
				Result.Add(new KeyValuePair<string, object>("Paid", this.Paid));

			if (!string.IsNullOrEmpty(this.NeuroCreditsContractId)) 
				Result.Add(new KeyValuePair<string, object>("NeuroCreditsContractId", this.NeuroCreditsContractId));
			
			if (!string.IsNullOrEmpty(this.CancellationContractId)) 
				Result.Add(new KeyValuePair<string, object>("CancellationContractId", this.CancellationContractId));
			
			if (!string.IsNullOrEmpty(this.ExternalReference)) 
				Result.Add(new KeyValuePair<string, object>("ExternalReference", this.ExternalReference));
			
			if (!string.IsNullOrEmpty(this.FirstName)) 
				Result.Add(new KeyValuePair<string, object>("FirstName", this.FirstName));
			
			if (!string.IsNullOrEmpty(this.MiddleName)) 
				Result.Add(new KeyValuePair<string, object>("MiddleName", this.MiddleName));
			
			if (!string.IsNullOrEmpty(this.LastName)) 
				Result.Add(new KeyValuePair<string, object>("LastName", this.LastName));
			
			if (!string.IsNullOrEmpty(this.PersonalNumber)) 
				Result.Add(new KeyValuePair<string, object>("PersonalNumber", this.PersonalNumber));
			
			if (!string.IsNullOrEmpty(this.Address)) 
				Result.Add(new KeyValuePair<string, object>("Address", this.Address));
			
			if (!string.IsNullOrEmpty(this.Address2)) 
				Result.Add(new KeyValuePair<string, object>("Address2", this.Address2));
			
			if (!string.IsNullOrEmpty(this.Area)) 
				Result.Add(new KeyValuePair<string, object>("Area", this.Area));
			
			if (!string.IsNullOrEmpty(this.City)) 
				Result.Add(new KeyValuePair<string, object>("City", this.City));
			
			if (!string.IsNullOrEmpty(this.PostalCode)) 
				Result.Add(new KeyValuePair<string, object>("PostalCode", this.PostalCode));
			
			if (!string.IsNullOrEmpty(this.Region)) 
				Result.Add(new KeyValuePair<string, object>("Region", this.Region));
			
			if (!string.IsNullOrEmpty(this.Country)) 
				Result.Add(new KeyValuePair<string, object>("Country", this.Country));
			
			if (!string.IsNullOrEmpty(this.Jid)) 
				Result.Add(new KeyValuePair<string, object>("Jid", this.Jid));
			
			if (!string.IsNullOrEmpty(this.PhoneNumber)) 
				Result.Add(new KeyValuePair<string, object>("PhoneNumber", this.PhoneNumber));
			
			if (!string.IsNullOrEmpty(this.OrganizationName)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationName", this.OrganizationName));
			
			if (!string.IsNullOrEmpty(this.Department)) 
				Result.Add(new KeyValuePair<string, object>("Department", this.Department));
			
			if (!string.IsNullOrEmpty(this.Role)) 
				Result.Add(new KeyValuePair<string, object>("Role", this.Role));
			
			if (!string.IsNullOrEmpty(this.OrganizationNumber)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationNumber", this.OrganizationNumber));
			
			if (!string.IsNullOrEmpty(this.OrganizationAddress)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationAddress", this.OrganizationAddress));
			
			if (!string.IsNullOrEmpty(this.OrganizationAddress2)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationAddress2", this.OrganizationAddress2));
			
			if (!string.IsNullOrEmpty(this.OrganizationArea)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationArea", this.OrganizationArea));
			
			if (!string.IsNullOrEmpty(this.OrganizationCity)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationCity", this.OrganizationCity));
			
			if (!string.IsNullOrEmpty(this.OrganizationPostalCode)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationPostalCode", this.OrganizationPostalCode));
			
			if (!string.IsNullOrEmpty(this.OrganizationRegion)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationRegion", this.OrganizationRegion));
			
			if (!string.IsNullOrEmpty(this.OrganizationCountry)) 
				Result.Add(new KeyValuePair<string, object>("OrganizationCountry", this.OrganizationCountry));

			return Result.ToArray();
		}

		/// <summary>
		/// Upgrades properties persisted in earlier version.
		/// </summary>
		/// <param name="Properties">Obsolete properties.</param>
		public void UpgradePropertyValue(Dictionary<string, object> Properties)
		{
			foreach (KeyValuePair<string,object> Property in Properties)
			{
				switch (Property.Key)
				{
					case "DueInterest":
						this.PeriodInterest = Expression.ToDecimal(Property.Value);
						break;

					case "DueDays":
						this.Period = new Duration(false, 0, 0, (int)Expression.ToDouble(Property.Value), 0, 0, 0);
						break;
				}
			}
		}

	}
}
