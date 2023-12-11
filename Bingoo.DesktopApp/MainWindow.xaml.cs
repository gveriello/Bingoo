using Microsoft.Extensions.DependencyInjection;
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
        }
    }
}
