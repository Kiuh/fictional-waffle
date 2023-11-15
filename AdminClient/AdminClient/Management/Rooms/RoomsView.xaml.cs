namespace AdminClient.Management.Rooms;

public partial class RoomsView : ContentPage
{
	public RoomsView()
	{
		var model  = new RoomsModel();
		model.Init();

		BindingContext = model;

		InitializeComponent();
	}
}