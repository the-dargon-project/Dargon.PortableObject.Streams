using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ItzWarty.IO;
using ItzWarty.Threading;
using Nito.AsyncEx;
using NLog;

namespace Dargon.PortableObjects.Streams {
   public interface PofStreamWriter : IDisposable {
      void Write(object obj);
      Task WriteAsync(object obj);
      Task WriteAsync(object obj, ICancellationToken cancellationToken);
   }

   public class PofStreamWriterImpl : PofStreamWriter {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly AsyncLock mutex = new AsyncLock();
      private readonly IPofSerializer serializer;
      private readonly IStream stream;
      private bool disposed = false;

      public PofStreamWriterImpl(IPofSerializer serializer, IStream stream) {
         this.serializer = serializer;
         this.stream = stream;
      }

      public void Write(object obj) {
         serializer.Serialize(stream.Writer, obj);
      }
      
      public Task WriteAsync(object obj) {
         return WriteAsync(obj, null);
      }

      public async Task WriteAsync(object obj, ICancellationToken cancellationToken) {
         try {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms)) {
               serializer.Serialize(writer, obj);
               using (await mutex.LockAsync(cancellationToken == null ? CancellationToken.None : cancellationToken.__InnerToken)) {
                  await stream.WriteAsync(ms.GetBuffer(), 0, (int)ms.Length, cancellationToken);
               }
            }
         } catch (Exception e) {
            logger.Error(e);
            throw;
         }
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            stream.Dispose();
         }
      }
   }
}