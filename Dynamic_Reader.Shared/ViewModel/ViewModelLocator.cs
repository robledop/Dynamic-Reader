/*
  In App.xaml:
  <Application.Resources>
	  <vm:ViewModelLocatorTemplate xmlns:vm="using:MvvmLight1.ViewModel"
								   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System.Diagnostics.CodeAnalysis;
using Dynamic_Reader.Interfaces;
using Dynamic_Reader.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace Dynamic_Reader.ViewModel
{
	public class ViewModelLocator
	{
		static ViewModelLocator()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			if (ViewModelBase.IsInDesignModeStatic)
			{
				//SimpleIoc.Default.Register<IDataService, DesignDataService>();
			}

			SimpleIoc.Default.Register<IDataService, DataService>();
			SimpleIoc.Default.Register<ISdCardService, SdCardService>();
			SimpleIoc.Default.Register<IDialogService, DialogService>();
			SimpleIoc.Default.Register<INavigationService, NavigationService>();
			SimpleIoc.Default.Register<IImporter, BookImporter>();
			SimpleIoc.Default.Register<MainViewModel>();
			SimpleIoc.Default.Register<BookViewModel>();
		}

		[SuppressMessage("Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "This non-static member is needed for data binding purposes.")]
		public MainViewModel Main
		{
			get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
		}

		public BookViewModel Book
		{
			get { return ServiceLocator.Current.GetInstance<BookViewModel>(); }
		}


		public static void Cleanup()
		{
		}
	}
}