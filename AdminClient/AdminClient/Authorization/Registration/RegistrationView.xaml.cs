namespace AdminClient.Authorization.Registration;

public partial class RegistrationView : ContentPage
{
	public RegistrationView()
	{
		BindingContext = new RegistrationModel();

		InitializeComponent();
	}
}