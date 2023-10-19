using System;
using System.Collections.Generic;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace TAG.Payments.NeuroCredits
{
	/// <summary>
	/// Information about a person.
	/// </summary>
	public class PersonalInformation
	{
		/// <summary>
		/// Information about a person.
		/// </summary>
		/// <param name="Identity">Generic identity object.</param>
		public PersonalInformation(GenericObject Identity)
		{
			if (Identity.TryGetFieldValue("Properties", out object Obj) && Obj is Array A)
			{
				foreach (object Item in A)
				{
					if (Item is GenericObject Property &&
						Property.TryGetFieldValue("Name", out Obj) && (Obj is CaseInsensitiveString Name) &&
						Property.TryGetFieldValue("Value", out Obj))
					{
						if (Obj is CaseInsensitiveString Value)
							this.Set(Name, Value);
						else
							this.Set(Name, Obj?.ToString());
					}
				}
			}
		}

		/// <summary>
		/// Information about a person.
		/// </summary>
		/// <param name="IdentityProperties">Properties from a legal identity.</param>
		public PersonalInformation(IDictionary<CaseInsensitiveString, CaseInsensitiveString> IdentityProperties)
		{
			foreach (KeyValuePair<CaseInsensitiveString, CaseInsensitiveString> Property in IdentityProperties)
				this.Set(Property.Key, Property.Value);
		}

		private void Set(string Name, string Value)
		{
			switch (Name.ToUpper())
			{
				case "FIRST":
					this.FirstName = Value;
					break;

				case "MIDDLE":
					this.MiddleName = Value;
					break;

				case "LAST":
					this.LastName = Value;
					break;

				case "PNR":
					this.PersonalNumber = Value;
					break;

				case "ADDR":
					this.Address = Value;
					break;

				case "ADDR2":
					this.Address2 = Value;
					break;

				case "AREA":
					this.Area = Value;
					break;

				case "CITY":
					this.City = Value;
					break;

				case "ZIP":
					this.PostalCode = Value;
					break;

				case "REGION":
					this.Region = Value;
					break;

				case "COUNTRY":
					this.Country = Value;
					break;

				case "PHONE":
					this.PhoneNumber = Value;
					break;

				case "JID":
					this.Jid = Value;
					break;

				case "ORGNAME":
					this.OrganizationName = Value;
					break;

				case "ORGDEPT":
					this.Department = Value;
					break;

				case "ORGROLE":
					this.Role = Value;
					break;

				case "ORGNR":
					this.OrganizationNumber = Value;
					break;

				case "ORGADDR":
					this.OrganizationAddress = Value;
					break;

				case "ORGADDR2":
					this.OrganizationAddress2 = Value;
					break;

				case "ORGAREA":
					this.OrganizationArea = Value;
					break;

				case "ORGCITY":
					this.OrganizationCity = Value;
					break;

				case "ORGZIP":
					this.OrganizationPostalCode = Value;
					break;

				case "ORGREGION":
					this.OrganizationRegion = Value;
					break;

				case "ORGCOUNTRY":
					this.OrganizationCountry = Value;
					break;
			}
		}

		/// <summary>
		/// First name of person
		/// </summary>
		public string FirstName { get; private set; }

		/// <summary>
		/// Middle name of person
		/// </summary>
		public string MiddleName { get; private set; }

		/// <summary>
		/// Last name of person
		/// </summary>
		public string LastName { get; private set; }

		/// <summary>
		/// Personal Number
		/// </summary>
		public string PersonalNumber { get; private set; }

		/// <summary>
		/// Address (line 1) of person
		/// </summary>
		public string Address { get; private set; }

		/// <summary>
		/// Address (line 2) of person
		/// </summary>
		public string Address2 { get; private set; }

		/// <summary>
		/// Area of person
		/// </summary>
		public string Area { get; private set; }

		/// <summary>
		/// City of person
		/// </summary>
		public string City { get; private set; }

		/// <summary>
		/// Postal of person
		/// </summary>
		public string PostalCode { get; private set; }

		/// <summary>
		/// Region of person
		/// </summary>
		public string Region { get; private set; }

		/// <summary>
		/// Country of person
		/// </summary>
		public string Country { get; private set; }

		/// <summary>
		/// JID of person
		/// </summary>
		public string Jid { get; private set; }

		/// <summary>
		/// Phone number of person
		/// </summary>
		public string PhoneNumber { get; private set; }

		/// <summary>
		/// Organization Name
		/// </summary>
		public string OrganizationName { get; private set; }

		/// <summary>
		/// Department of person in organization
		/// </summary>
		public string Department { get; private set; }

		/// <summary>
		/// Role of person in organization
		/// </summary>
		public string Role { get; private set; }

		/// <summary>
		/// Organization number
		/// </summary>
		public string OrganizationNumber { get; private set; }

		/// <summary>
		/// Address (line 1) of organization
		/// </summary>
		public string OrganizationAddress { get; private set; }

		/// <summary>
		/// Address (line 2) of organization
		/// </summary>
		public string OrganizationAddress2 { get; private set; }

		/// <summary>
		/// Area of organization
		/// </summary>
		public string OrganizationArea { get; private set; }

		/// <summary>
		/// City of organization
		/// </summary>
		public string OrganizationCity { get; private set; }

		/// <summary>
		/// Postal code of organization
		/// </summary>
		public string OrganizationPostalCode { get; private set; }

		/// <summary>
		/// Region of organization
		/// </summary>
		public string OrganizationRegion { get; private set; }

		/// <summary>
		/// Country of organization
		/// </summary>
		public string OrganizationCountry { get; private set; }

		/// <summary>
		/// If sufficient information is available for personal billing.
		/// </summary>
		public bool HasPersonalBillingInformation
		{
			get
			{
				return
					!string.IsNullOrEmpty(this.FirstName) &&
					!string.IsNullOrEmpty(this.LastName) &&
					!string.IsNullOrEmpty(this.PersonalNumber) &&
					!string.IsNullOrEmpty(this.Address) &&
					!string.IsNullOrEmpty(this.City) &&
					!string.IsNullOrEmpty(this.PostalCode) &&
					!string.IsNullOrEmpty(this.Country) &&
					!string.IsNullOrEmpty(this.Jid);
			}
		}

		/// <summary>
		/// If sufficient information is available for organizational billing.
		/// </summary>
		public bool HasOrganizationalBillingInformation
		{
			get
			{
				return
					this.HasPersonalBillingInformation &&
					!string.IsNullOrEmpty(this.OrganizationName) &&
					!string.IsNullOrEmpty(this.OrganizationNumber) &&
					!string.IsNullOrEmpty(this.OrganizationAddress) &&
					!string.IsNullOrEmpty(this.OrganizationCity) &&
					!string.IsNullOrEmpty(this.OrganizationPostalCode) &&
					!string.IsNullOrEmpty(this.OrganizationCountry);
			}
		}
	}
}
