using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;
using NetTrace;
using DAL;

[TraceTags,
 // Optional Description of the enum to be used in the trace dialog
 EnumDesc("Tags in NetTraceTester")]
internal enum t
{
    // Optionally, user can give descriptions for
    // trace tags which will be used in the trace tags dialog.
    // Any tags which don't have a description
    // will use the enum name in the dialog.

    [TagDesc("This is a test")]
    Test,

    [TagDesc("This message won't print")]
    TestOff
}

namespace Genealogic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Configuration for .net projects is just a place to get a bunch of
            // key indexed values from to affect the running of the program.  There are
            // a number of places these values can some from.  The most common in Core is
            // from appsettings.json.  They can also be specified in environment variables
            // or from a json file specific to a build config type such as DEBUG or PRODUCTION.
            // "Building" a ConfigurationBuilder means to specify these various sources and
            // who trumps who.  So if "connectionString" is defined both in appsettings.json and
            // in an environment variable named "connectionString" which will we ultimately
            // choose from? Here, that is all determined in the BuildConfig() function below.
            var configBuilder = new ConfigurationBuilder();
            BuildConfig(configBuilder);
            var config = configBuilder.Build();

            // The reason we wanted all the config information is because the logger gets some of
            // it's information from the config file so we have to pass down the ConfigurationBuilder
            // object to the Configuration() method when setting up the logger.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                // WriteTo is specified in the appsettings.json file
                //.WriteTo.Debug()
                .CreateLogger();

            // Dependency Injection depends on logging so all the above had to come before we set up
            // DI here.
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                    {
                        // The interface and the class to retrieve for that interface
                        services.AddSingleton<INetTrace, NetTrace.NetTrace>();
                        services.AddSingleton<IGData, GData>();
                    })
                .UseSerilog(Log.Logger)
                .Build();

            // Get whatever class corresponds to IGreetingService from DI
            var dal = ActivatorUtilities.CreateInstance<GData>(host.Services);
            dal.GetData();
            InitializeComponent();
        }

        // Determine where we get our configuration information from
        private static void BuildConfig(IConfigurationBuilder builder)
        {
            // These add files and sources to retrieve config info from.
            // stuff added later will override the stuff added earlier.  So we initially
            // get our configuration from appsettings.json.  Then, if we have, for instance,
            // an appsettings.Production.json file then it's configuration overrides the 
            // stuff in appsettings.json.  Finally, any environment variables trumps both
            // those files.
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
