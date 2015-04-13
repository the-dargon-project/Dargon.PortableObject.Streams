using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using NLog;

namespace Dargon.PortableObjects.Streams {
   public interface PofDispatcher : IDisposable {
      void RegisterHandler(Type t, Action<object> handler);
      void RegisterHandler<T>(Action<T> handler);

      void Start();
   }

   public class PofDispatcherImpl : PofDispatcher {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IThreadingProxy threadingProxy;
      private readonly PofStreamReader reader;
      private readonly IConcurrentDictionary<Type, Action<object>> handlersByType;
      private readonly ICancellationTokenSource dispatcherTaskCancellationTokenSource;
      private Task dispatcherTask;

      public PofDispatcherImpl(
         IThreadingProxy threadingProxy,
         PofStreamReader reader
      ) : this(threadingProxy, reader, new ConcurrentDictionary<Type, Action<object>>()) {
      }

      public PofDispatcherImpl(IThreadingProxy threadingProxy, PofStreamReader reader, IConcurrentDictionary<Type, Action<object>> handlersByType) {
         this.threadingProxy = threadingProxy;
         this.reader = reader;
         this.handlersByType = handlersByType;

         this.dispatcherTaskCancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
      }

      public void RegisterHandler(Type t, Action<object> handler) {
         this.handlersByType.TryAdd(t, handler);
      }

      public void RegisterHandler<T>(Action<T> handler) {
         this.handlersByType.TryAdd(typeof(T), x => handler((T)x));
      }

      public void Start() {
         this.dispatcherTask = Run();
      }

      private async Task Run() {
         var cancellationToken = this.dispatcherTaskCancellationTokenSource.Token;
         try {
            while (!cancellationToken.IsCancellationRequested) {
               var obj = await reader.ReadAsync(cancellationToken);
               var type = obj.GetType();
               var handler = handlersByType.GetValueOrDefault(type);
               if (handler != null) {
                  handler(obj);
               } else {
                  throw new DispatcherUnhandledPortableObjectException(type);
               }
            }
         } catch (SocketException e) {
            // TODO: Audit Event
            Console.WriteLine(e);
         } catch (DispatcherUnhandledPortableObjectException e) {
            logger.Error("Dispatcher caught unhandled pof exception", e);
            Console.WriteLine(e);
         } catch (OperationCanceledException e) {
            logger.Info("Dispatcher caught operation cancelled exception", e);
            Console.WriteLine(e);
         } catch (Exception e) {
            logger.Error("Dispatcher caught unexpected exception", e);
            Console.WriteLine(e);
         } finally {
            reader.Dispose();
         }
      }

      public void Dispose() {
         dispatcherTaskCancellationTokenSource.Cancel();
         if (dispatcherTask != null) {
            dispatcherTask.Wait();
         }
         reader.Dispose();
      }
   }
}
