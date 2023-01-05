using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Notification.Domain.Interfaces;

namespace Lambda.Default.Handler.EventBridge
{
    /// <summary>
    /// Default lambda handler with event bridge
    /// </summary>
    /// <typeparam name="T">Detail Type</typeparam>
    public abstract class DefaultLambdaHandlerEventBridge<T>
        where T : class, new()
    {
        /// <summary>
        /// Notification service
        /// </summary>
        public abstract INotificatorService NotificatorService { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        ILogger<DefaultLambdaHandlerEventBridge<T>> Logger { get; set; }
        
        #region Constructor
        
        /// <summary>
        /// Default lambda handler for event bridge
        /// </summary>
        /// <param name="logger">Logger object</param>
        protected DefaultLambdaHandlerEventBridge(ILogger<DefaultLambdaHandlerEventBridge<T>> logger)
        {
            Logger = logger;
        }
        #endregion

        /// <summary>
        /// Default lambda handler for event bridge
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="context">Context</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Throw argument null exception when the request or the detail is null</exception>
        public async Task DefaultLambdaHandlerForEventBridge(CloudWatchEvent<T> request, ILambdaContext context)
        {
            Logger.LogInformation("Starting method");

            if (request == null || request.Detail == null)
            {
                Logger.LogWarning("Request or detail is null");
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                Logger.LogInformation("Starting service method");
                await CallServiceMethod(request.Detail);
                Logger.LogInformation("Service method is done");

                if (NotificatorService.HasNotification)
                {
                    Logger.LogInformation("Starting OnSuccess method");
                    await OnSuccess();
                    Logger.LogInformation("On success completed");
                }
                else
                {
                    Logger.LogInformation("Starting on failure with notifications methods");
                    await OnFailureWithNotifications(NotificatorService);
                    Logger.LogInformation("Starting OnSuccess method");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error has occour");
                await OnError(ex);
                throw;
            }
            finally
            {
                Logger.LogInformation("End of the proccess");
                NotificatorService.ClearNotifications();
            }
        }

        /// <summary>
        /// On success of your services, called after the call service method
        /// </summary>
        /// <returns>Task</returns>
        public abstract Task OnSuccess();
        
        /// <summary>
        /// On failure of your services, it if called if has some notifications
        /// </summary>
        /// <param name="notificatorService">Notification service</param>
        /// <returns>Task</returns>
        public abstract Task OnFailureWithNotifications(INotificatorService notificatorService);
        
        /// <summary>
        /// On error, in case of exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public abstract Task OnError(Exception ex);
        
        /// <summary>
        /// Call your services here
        /// </summary>
        /// <param name="detail">Your detail input</param>
        /// <returns>Task</returns>
        public abstract Task CallServiceMethod(T detail);
    }
}