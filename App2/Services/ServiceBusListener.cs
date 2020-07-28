using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App2.Services
{
    public class ServiceBusListener
    {
        private bool isInitialized;

        private ISubscriptionClient _subscriptionClient;


        public void Init(string serviceBusUri, string serviceBusToken, string topicName, string subscriptionName)
        {
            if (isInitialized)
            {
                //log warning trying to reinitialize already initialized
                return;
            }
            var connectionBuilder = new ServiceBusConnectionStringBuilder(serviceBusUri, topicName, serviceBusToken);

            _subscriptionClient = new SubscriptionClient(connectionBuilder, subscriptionName);
            RegisterOnMessageHandlerAndReceiveMessages();
            isInitialized = true;
        }

        public async void Unsubscribe()
        {
            if (!isInitialized)
            {
                //warning trying to unsubsribe from uninitialized bus
                return;
            }


            await _subscriptionClient.CloseAsync();
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = true
            };

            // Register the function that processes messages.
            _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            // Broadcast to message center

            var messageData = Encoding.UTF8.GetString(message.Body);

            Device.BeginInvokeOnMainThread(async () =>
                await Shell.Current.DisplayAlert("Message Received", messageData, "Cancel")
            );

        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Device.BeginInvokeOnMainThread(async () =>
                await Shell.Current.DisplayAlert("Error Received", exceptionReceivedEventArgs.Exception.Message, "Cancel")
            );


            return Task.CompletedTask;
        }
    }
}
