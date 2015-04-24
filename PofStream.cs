using System;
using ItzWarty.IO;

namespace Dargon.PortableObjects.Streams {
   public interface PofStream : IDisposable {
      PofStreamReader Reader { get; }
      PofStreamWriter Writer { get; }
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

      public PofStreamReader Reader { get { return reader; } }
      public PofStreamWriter Writer { get { return writer; } }

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