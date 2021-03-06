﻿using KNXcontrol.Services;
using KNXcontrol.ViewModels;
using Model.Models;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventArgs = System.EventArgs;

namespace KNXcontrol.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TypesOverviewPage : ContentPage
    {
        readonly TypesViewModel viewModel;

        public TypesOverviewPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new TypesViewModel();

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Types?.Count == 0)
                viewModel.IsBusy = true;
        }
        /// <summary>
        /// Prompts user to confirm deletion, sends selected type to the service to be deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Delete_Clicked(object sender, EventArgs e)
        {
            var id = ((Button)sender).CommandParameter.ToString();

            bool answer = await DisplayAlert("Brisanje tipa", "Jeste li sigurni da želite obrisati tip?", "Da", "Odustani");

            if (answer)
            {
                var result = await DependencyService.Get<IConnector>().DeleteType(id);
                if (result)
                {
                    DependencyService.Get<IToastService>().ShowToast("Tip je uspješno obrisan!");
                    var deletedItem = viewModel.Types.ToList().Find(x => x._id.ToString() == id);
                    viewModel.Types.Remove(deletedItem);
                }
            }
        }
        /// <summary>
        /// On doubleTap opens new modal to update type data, uses MessagingCenter to retrieve data and pass to service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTypeSelection(object sender, EventArgs e)
        {
            var type = ((TappedEventArgs)e).Parameter as Type;

            await Navigation.PushModalAsync(new NavigationPage(new NewTypePage(type)));
            MessagingCenter.Subscribe<NewRoomPage, Type>(this, "UPDATE", (page, newType) =>
            {
                var oldType = viewModel.Types.FirstOrDefault(x => x._id == type._id);
                int i = viewModel.Types.IndexOf(oldType);
                viewModel.Types[i] = newType;
            });
        }
        /// <summary>
        /// Opens new modal for creating a type, uses MessagingCenter to retrieve data and pass it to service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddType_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new NewTypePage(null)));
            MessagingCenter.Subscribe<NewTypePage, Type>(this, "CREATE", (page, type) =>
            {
                viewModel.Types.Add(type);
            });
        }
    }
}