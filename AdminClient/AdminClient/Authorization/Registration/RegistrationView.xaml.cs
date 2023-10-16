namespace AdminClient.Authorization.Registration;

public partial class RegistrationView : ContentView
{
	public RegistrationView()
	{
		BindingContext = new RegistrationModel();

		InitializeComponent();
	}
}