using PlayListSample.ViewModels;

namespace PlayListSample.Views;

public partial class VideoDetailsPage : ContentPage
{
	public VideoDetailsPage()
	{
		InitializeComponent();
		BindingContext= new VideoDetailsVM();
	}
}