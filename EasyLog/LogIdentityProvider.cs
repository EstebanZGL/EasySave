using System;

namespace EasyLog
{
    /// <summary>
    /// Interface for providing identity information for logs
    /// </summary>
    public interface ILogIdentityProvider
    {
        /// <summary>
        /// Gets the machine name to use in logs
        /// </summary>
        string GetMachineName();

        /// <summary>
        /// Gets the user name to use in logs
        /// </summary>
        string GetUserName();
    }

    /// <summary>
    /// Default implementation that uses system environment values
    /// </summary>
    public class DefaultLogIdentityProvider : ILogIdentityProvider
    {
        /// <summary>
        /// Gets the machine name from the system environment
        /// </summary>
        public string GetMachineName()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Gets the user name from the system environment
        /// </summary>
        public string GetUserName()
        {
            return Environment.UserName;
        }
    }

    /// <summary>
    /// Static provider that delegates to a configured implementation
    /// </summary>
    public static class LogIdentityProvider
    {
        private static ILogIdentityProvider _provider = new DefaultLogIdentityProvider();

        /// <summary>
        /// Configure the identity provider to use
        /// </summary>
        public static void Configure(ILogIdentityProvider provider)
        {
            _provider = provider ?? new DefaultLogIdentityProvider();
        }

        /// <summary>
        /// Gets the machine name to use in logs
        /// </summary>
        public static string GetMachineName()
        {
            return _provider.GetMachineName();
        }

        /// <summary>
        /// Gets the user name to use in logs
        /// </summary>
        public static string GetUserName()
        {
            return _provider.GetUserName();
        }
    }
}