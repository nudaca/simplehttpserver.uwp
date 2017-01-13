// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Pickers;
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
        HttpServer Server;

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

        private async void btnPickerImage_Click(object sender, RoutedEventArgs e)
        {
            StartServer();

            // var myPictures = await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Log("Picked photo: " + file.Path, "Success");
                Server.File = file;
            }
            else
            {
                Log("Operation cancelled.", "Error");
            }
        }

        private async void btnPickerVideo_Click(object sender, RoutedEventArgs e)
        {
            StartServer();

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".wmv");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Log("Picked video: " + file.Path, "Success");
                Server.File = file;
            }
            else
            {
                Log("Operation cancelled.", "Error");
            }
        }

        private async void btnPickerSite_Click(object sender, RoutedEventArgs e)
        {
            StartServer();

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".html");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Log("Picked site: " + file.Path, "Success");
                Server.File = file;
            }
            else
            {
                Log("Operation cancelled.", "Error");
            }
        }

        private async void Log(string message, string title)
        {
            await new MessageDialog(message, title).ShowAsync();
        }

        private async void StartServer()
        {
            await Task.Run(() =>
            {
                Server = new HttpServer(8080);
                Server.StartServer();
            });
        }
    }
}
