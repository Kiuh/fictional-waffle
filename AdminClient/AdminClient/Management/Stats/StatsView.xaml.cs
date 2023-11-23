namespace AdminClient.Management.Stats;

public partial class StatsView : ContentPage
{
	public StatsView()
	{
		var model = new StatsModel();
		model.Init();

		BindingContext = model;
		
		InitializeComponent();
	}
}