using System.Threading.Tasks;
using Waher.Runtime.Settings;

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
	}
}
