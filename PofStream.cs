using System;
using System.Threading.Tasks;
using ItzWarty.IO;
using ItzWarty.Threading;

namespace Dargon.PortableObjects.Streams {
   public interface PofStream : IDisposable {
      PofStreamReader Reader { get; }
      PofStreamWriter Writer { get; }

      object Read();
      Task<object> ReadAsync();
      Task<object> ReadAsync(ICancellationToken cancellationToken);

      void Write(object obj);
      Task WriteAsync(object obj);
      Task WriteAsync(object obj, ICancellationToken cancellationToken);
   }

   public class PofStreamImpl : PofStream {
      private readonly IStream stream;
      private readonly PofStreamReader reader;
      private readonly PofStreamWriter writer;
      private bool disposed = false;

      public PofStreamImpl(IStream stream, PofStreamReader reader, PofStreamWriter writer) {
         this.stream = stream;
         this.reader = reader;
         this.writer = writer;
      }

      public PofStreamReader Reader => reader;
      public PofStreamWriter Writer => writer;

      public object Read() => reader.Read();
      public Task<object> ReadAsync() => reader.ReadAsync();
      public Task<object> ReadAsync(ICancellationToken cancellationToken) => reader.ReadAsync(cancellationToken);

      public void Write(object obj) => writer.Write(obj);
      public Task WriteAsync(object obj) => writer.WriteAsync(obj);
      public Task WriteAsync(object obj, ICancellationToken cancellationToken) => writer.WriteAsync(obj, cancellationToken);

      public void Dispose() {
         if (!disposed) {
            disposed = true;
            stream.Dispose();
            reader.Dispose();
            writer.Dispose();
         }
      }
   }
}