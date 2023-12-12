using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Windows;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Bingoo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddBlazorWebViewDeveloperTools();
            serviceCollection.AddSpeechSynthesis();
            Resources.Add("services", serviceCollection.BuildServiceProvider());

            this.blazorWebView.HostPage = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "wwwroot", "index.html");
        }
    }
}
