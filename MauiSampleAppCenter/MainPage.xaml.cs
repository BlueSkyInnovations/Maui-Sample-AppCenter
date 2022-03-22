namespace MauiSampleAppCenter;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnThrowExceptionClicked(object sender, EventArgs e)
	{
		Microsoft.AppCenter.Crashes.Crashes.GenerateTestCrash();
	}
}