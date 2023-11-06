using System.Threading.Tasks;
using Waher.Runtime.Settings;

namespace TAG.Payments.NeuroCredits.Configuration
{
    /// <summary>
    /// Contains the e-mail configuration.
    /// </summary>
    public class MailConfiguration
    {
        private static MailConfiguration current = null;

        /// <summary>
        /// Contains the e-mail configuration.
        /// </summary>
        public MailConfiguration()
        {
        }

        /// <summary>
        /// E-mail address
        /// </summary>
        public string Address
        {
            get;
            private set;
        }

        /// <summary>
        /// SMTP Host
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// SMTP Port
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// SMTP Account
        /// </summary>
        public string Account
        {
            get;
            private set;
        }

        /// <summary>
        /// SMTP Password
        /// </summary>
        public string Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Test e-mail recipient.
        /// </summary>
        public string TestRecipient
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
                    !string.IsNullOrEmpty(Address) &&
                    !string.IsNullOrEmpty(Host) &&
                    !string.IsNullOrEmpty(Account) &&
                    !string.IsNullOrEmpty(Password) &&
                    Port > 0 &&
                    Port <= 65535;
            }
        }

        /// <summary>
        /// Loads configuration settings.
        /// </summary>
        /// <returns>Configuration settings.</returns>
        public static async Task<MailConfiguration> Load()
        {
            MailConfiguration Result = new MailConfiguration();
            string Prefix = NeuroCreditsServiceProvider.ServiceId;

            Result.Address = await RuntimeSettings.GetAsync(Prefix + ".SMTP.Address", string.Empty);
            Result.Host = await RuntimeSettings.GetAsync(Prefix + ".SMTP.Host", string.Empty);
            Result.Port = (int)await RuntimeSettings.GetAsync(Prefix + ".SMTP.Port", 25);
            Result.Account = await RuntimeSettings.GetAsync(Prefix + ".SMTP.Account", string.Empty);
            Result.Password = await RuntimeSettings.GetAsync(Prefix + ".SMTP.Password", string.Empty);
            Result.TestRecipient = await RuntimeSettings.GetAsync(Prefix + ".SMTP.TestRecipient", string.Empty);

            return Result;
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        /// <returns>Configuration</returns>
        public static async Task<MailConfiguration> GetCurrent()
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
