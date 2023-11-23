using AdminClient.Authorization;
using AdminClient.Management;

namespace AdminClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AuthorizationView();
        }
    }
}