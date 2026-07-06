using MathGrapher.Core.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MathGrapher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var connectionString = ConfigurationManager.ConnectionStrings["MathGrapherConnection"]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Строка подключения не найдена в App.config", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            DatabaseHelper.Initialize(connectionString);
        }
    }
}