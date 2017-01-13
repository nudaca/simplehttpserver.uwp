// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Networking.Sockets;
using System.IO;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using HttpServer;

namespace WebServerTask
{
    public sealed class WebServerBGTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Associate a cancellation handler with the background task. 
            taskInstance.Canceled += OnCanceled;

            // Get the deferral object from the task instance
            serviceDeferral = taskInstance.GetDeferral();

            var appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appService != null &&
                appService.Name == "App2AppComService")
            {
                appServiceConnection = appService.AppServiceConnection;
                appServiceConnection.RequestReceived += OnRequestReceived;
            }
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            string command = message["Command"] as string;

            switch (command)
            {
                case "Initialize":
                    var messageDeferral = args.GetDeferral();
                    //Set a result to return to the caller
                    var returnMessage = new ValueSet();
                    Server server = new Server(8000, appServiceConnection);
                    IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                        (workItem) =>
                        {
                            server.StartServer();
                        });
                    returnMessage.Add("Status", "Success");
                    var responseStatus = await args.Request.SendResponseAsync(returnMessage);
                    messageDeferral.Complete();
                    break;
                case "Quit":
                    //Service was asked to quit. Give us service deferral
                    //so platform can terminate the background task
                    serviceDeferral.Complete();
                    break;
            }
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //Clean up and get ready to exit
        }

        BackgroundTaskDeferral serviceDeferral;
        AppServiceConnection appServiceConnection;
    }
}
