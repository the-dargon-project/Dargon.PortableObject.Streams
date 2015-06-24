using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ItzWarty.IO;
using ItzWarty.Threading;
using Nito.AsyncEx;

namespace Dargon.PortableObjects.Streams {
   public interface PofStreamReader : IDisposable {
      object Read();
      Task<object> ReadAsync();
      Task<object> ReadAsync(ICancellationToken cancellationToken);

      T Read<T>();
      Task<T> ReadAsync<T>();
      Task<T> ReadAsync<T>(ICancellationToken cancellationToken);
   }

   public class PofStreamReaderImpl : PofStreamReader {
      private readonly AsyncLock asyncLock = new AsyncLock();
      private readonly IPofSerializer serializer;
      private readonly IStream stream;
      private bool disposed = false;

      public PofStreamReaderImpl(IPofSerializer serializer, IStream stream) {
         this.serializer = serializer;
         this.stream = stream;
      }

      public object Read() {
         return serializer.Deserialize(stream.GetReader());
      }

      public Task<object> ReadAsync() {
         return ReadAsync(null);
      }

      public async Task<object> ReadAsync(ICancellationToken cancellationToken) {
         using (await asyncLock.LockAsync(cancellationToken == null ? CancellationToken.None : cancellationToken.__InnerToken)) {
            const int kInt32ByteCount = 4;
            byte[] lengthBuffer = await ReadBytesAsync(kInt32ByteCount, cancellationToken);
            var length = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] dataBuffer = await ReadBytesAsync(length, cancellationToken);
            using (var ms = new MemoryStream(dataBuffer))
            using (var msReader = new BinaryReader(ms)) {
               return serializer.Deserialize(msReader, SerializationFlags.Lengthless, null);
            }
         }
      }

      public T Read<T>() { return (T)Read(); }

      public Task<T> ReadAsync<T>() { return ReadAsync<T>(null); }

      public Task<T> ReadAsync<T>(ICancellationToken cancellationToken) {
         var tcs = new TaskCompletionSource<T>();
         ReadAsync(cancellationToken).ContinueWith(t => {
            if (t.IsCanceled) {
               tcs.TrySetCanceled();
            } else if (t.IsFaulted) {
               Debug.Assert(t.Exception != null, "t.Exception != null");
               tcs.TrySetException(t.Exception.InnerExceptions);
            } else {
               tcs.TrySetResult((T)t.Result);
            } 
         }, TaskContinuationOptions.ExecuteSynchronously);
         return tcs.Task;
      }

      private async Task<byte[]> ReadBytesAsync(int count, ICancellationToken cancellationToken) {
         byte[] buffer = new byte[count];
         int bytesReadTotal = 0;
         while(bytesReadTotal != count) {
            var bytesRead = await stream.ReadAsync(buffer, bytesReadTotal, count - bytesReadTotal, cancellationToken);
            bytesReadTotal += bytesRead;
            if (bytesRead == 0) {
               throw new EndOfStreamException();
            }
         }
         return buffer;
      }

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            stream.Dispose();
         }
      }
   }
}
