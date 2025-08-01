using ArslanProjectManager.MobileUI.ViewModels;

namespace ArslanProjectManager.MobileUI.Views;

public partial class ProfileEditPage : ContentPage
{
	private readonly ProfileEditViewModel _viewModel;
	
	public ProfileEditPage(ProfileEditViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		Loaded += async (s, e) => await _viewModel.LoadProfileAsync();
	}
}