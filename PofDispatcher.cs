using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Threading;
using NLog;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Dargon.PortableObjects.Streams {
   public interface PofDispatcher : IDisposable {
      void RegisterHandler(Type t, Action<object> handler);
      void RegisterHandler<T>(Action<T> handler);
      void RegisterShutdownHandler(Action handler);

      void Start();
   }

   public class PofDispatcherImpl : PofDispatcher {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly PofStreamReader reader;
      private readonly IConcurrentDictionary<Type, Action<object>> handlersByType;
      private readonly IConcurrentSet<Action> shutdownHandlers;
      private readonly ICancellationTokenSource dispatcherTaskCancellationTokenSource;

      private Task dispatcherTask;
      private bool disposed = false;

      public PofDispatcherImpl(
         IThreadingProxy threadingProxy,
         PofStreamReader reader
      ) : this(threadingProxy, reader, new ConcurrentDictionary<Type, Action<object>>(), new ConcurrentSet<Action>()) {
      }

      public PofDispatcherImpl(
         IThreadingProxy threadingProxy, 
         PofStreamReader reader, 
         IConcurrentDictionary<Type, Action<object>> handlersByType, 
         IConcurrentSet<Action> shutdownHandlers
      ) : this(
         reader,
         handlersByType,
         shutdownHandlers,
         threadingProxy.CreateCancellationTokenSource()
      ) { }

      public PofDispatcherImpl(PofStreamReader reader, IConcurrentDictionary<Type, Action<object>> handlersByType, IConcurrentSet<Action> shutdownHandlers, ICancellationTokenSource dispatcherTaskCancellationTokenSource) {
         this.reader = reader;
         this.handlersByType = handlersByType;
         this.shutdownHandlers = shutdownHandlers;
         this.dispatcherTaskCancellationTokenSource = dispatcherTaskCancellationTokenSource;
      }

      public void RegisterHandler(Type t, Action<object> handler) {
         this.handlersByType.TryAdd(t, handler);
      }

      public void RegisterHandler<T>(Action<T> handler) {
         this.handlersByType.TryAdd(typeof(T), x => handler((T)x));
      }

      public void RegisterShutdownHandler(Action handler) {
         this.shutdownHandlers.Add(handler);
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
               Action<object> handler;
               if (handlersByType.TryGetValue(type, out handler)) {
                  handler(obj);
               } else {
                  throw new DispatcherUnhandledPortableObjectException(type);
               }
            }
         } catch (SocketException e) {
            logger.Error("Caught socket exception", e);
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
            shutdownHandlers.ForEach(x => x.Invoke());
            reader.Dispose();
         }
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            dispatcherTaskCancellationTokenSource.Cancel();
            if (dispatcherTask != null) {
               dispatcherTask.Wait();
            }
            reader.Dispose();
         }
      }
   }
}
