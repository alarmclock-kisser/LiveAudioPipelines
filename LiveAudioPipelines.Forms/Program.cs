using LiveAudioPipelines.Shared;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace LiveAudioPipelines.Forms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Read configuration from appsettings.json using Microsoft.Extensions.Configuration
            AppSettings settings = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build()
                .Get<AppSettings>() ?? new AppSettings();

            if (string.IsNullOrEmpty(settings.LogFilePath))
            {
                settings.LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            }

            // Init StaticLogger
            StaticLogger.InitializeLogFiles(settings.LogFilePath, settings.CreateLogFile, settings.MaxLogFiles);

            Application.EnableVisualStyles();


            ApplicationConfiguration.Initialize();
            Application.Run(new WindowMain(settings));
        }
    }
}