using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminClient.Authorization.Login
{
	public partial class LoginView : ContentPage
	{
		public LoginView()
		{
			BindingContext = new LoginModel();

			InitializeComponent();
		}
	}
}

