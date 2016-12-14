// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WebServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HttpServer server;
        private readonly StorageFolder installLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;

        public MainPage()
        {
            InitializeComponent();
        }

        // Example of a Background Task, not tested
        private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            string myTaskName = "First Task";

            // check if task is already registered
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == myTaskName)
                {
                    Log("Task already registered", "Caution");
                    return;
                }
            }

            // Windows Phone app must call this to use trigger types (see MSDN)
            await BackgroundExecutionManager.RequestAccessAsync();

            // register a new task
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder { Name = "First Task", TaskEntryPoint = "WebServerTask.BackgroundTask" };

            taskBuilder.SetTrigger(new TimeTrigger(15, true));
            //taskBuilder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
            
            BackgroundTaskRegistration myFirstTask = taskBuilder.Register();

            await (new MessageDialog("Task registered")).ShowAsync();
        }

        private async void pickerBtb_Click(object sender, RoutedEventArgs e)
        {
            //Set a result to return to the caller
            var returnMessage = new ValueSet();
            server = new HttpServer(8080);
            server.StartServer();
            returnMessage.Add("Status", "Success");

            // var myPictures = await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Log("Picked photo: " + file.Path, "Success");
                server.FilePath = file.Path;
            }
            else
            {
                Log("Operation cancelled.", "Error");
            }
        }

        private async void pickerVideoBtn_Click(object sender, RoutedEventArgs e)
        {
            //Set a result to return to the caller
            var returnMessage = new ValueSet();
            server = new HttpServer(8080);
            server.StartServer();
            returnMessage.Add("Status", "Success");

            Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.HomeGroup;
            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".wmv");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Log("Picked video: " + file.Path, "Success");
                server.FilePath = file.Path;
            }
            else
            {
                Log("Operation cancelled.", "Error");
            }
        }

        private static async void Log(string message, string title)
        {
            await new MessageDialog(message, title).ShowAsync();
        }
    }
}
