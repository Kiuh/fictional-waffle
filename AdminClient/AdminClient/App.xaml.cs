using AdminClient.Authorization;
using AdminClient.Authorization.Login;
using AdminClient.Management;

namespace AdminClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new LoginView();
        }
    }
}