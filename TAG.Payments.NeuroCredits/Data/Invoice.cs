﻿using System;
using System.Collections.Generic;
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
	[Index("Account", "IsPaid", "InvoiceNumber")]
	[Index("Account", "IsPaid", "Amount")]
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
				new KeyValuePair<string, object>("Currency", this.Currency),
				new KeyValuePair<string, object>("DueDate", this.DueDate),
				new KeyValuePair<string, object>("Created", this.Created)
			};

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
	}
}