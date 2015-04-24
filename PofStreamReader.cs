using System;
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
            byte[] lengthBuffer = await ReadBytesAsync(kInt32ByteCount);
            var length = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] dataBuffer = await ReadBytesAsync(length);
            using (var ms = new MemoryStream(dataBuffer))
            using (var msReader = new BinaryReader(ms)) {
               return serializer.Deserialize(msReader, SerializationFlags.Lengthless, null);
            }
         }
      }

      private async Task<byte[]> ReadBytesAsync(int count) {
         byte[] buffer = new byte[count];
         int bytesReadTotal = 0;
         while(bytesReadTotal != count) {
            var bytesRead = await stream.ReadAsync(buffer, bytesReadTotal, count - bytesReadTotal);
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
