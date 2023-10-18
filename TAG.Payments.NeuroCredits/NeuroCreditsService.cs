using Paiwise;

namespace TAG.Payments.NeuroCredits
{
	/// <summary>
	/// Serivce for Neuro-Credits™
	/// </summary>
	public class NeuroCreditsService : IBuyEDalerService
	{
		private const string buyEDalerTemplateId = "2cc29f51-7a90-41b3-8c1e-cdd6425bbb2c@legal.lab.tagroot.io";

		private readonly NeuroCreditsServiceProvider provider;

		/// <summary>
		/// Serivce for Neuro-Credits™
		/// </summary>
		/// <param name="Provider">Reference to the service provider.</param>
		public NeuroCreditsService(NeuroCreditsServiceProvider Provider)
		{
			this.provider = Provider;
		}

		#region IServiceProvider

		/// <summary>
		/// ID of service
		/// </summary>
		public string Id => ServiceId;

		/// <summary>
		/// ID of service.
		/// </summary>
		public static string ServiceId = typeof(NeuroCreditsService).Namespace;

		/// <summary>
		/// Name of service
		/// </summary>
		public string Name => "Neuro-Credits™";

		/// <summary>
		/// Icon URL
		/// </summary>
		public string IconUrl => string.Empty;

		/// <summary>
		/// Width of icon, in pixels.
		/// </summary>
		public int IconWidth => 0;

		/// <summary>
		/// Height of icon, in pixels
		/// </summary>
		public int IconHeight => 0;

		#endregion

	}
}
