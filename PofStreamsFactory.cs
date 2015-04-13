using System.IO;
using ItzWarty.IO;
using ItzWarty.Threading;

namespace Dargon.PortableObjects.Streams {
   public interface PofStreamsFactory {
      PofStream CreatePofStream(Stream stream);
      PofStream CreatePofStream(IStream stream);
      PofDispatcher CreateDispatcher(PofStream stream);
      PofDispatcher CreateDispatcher(PofStreamReader reader);
   }

   public class PofStreamsFactoryImpl : PofStreamsFactory {
      private readonly IThreadingProxy threadingProxy;
      private readonly IStreamFactory streamFactory;
      private readonly IPofSerializer serializer;

      public PofStreamsFactoryImpl(IThreadingProxy threadingProxy, IStreamFactory streamFactory, IPofSerializer serializer) {
         this.threadingProxy = threadingProxy;
         this.streamFactory = streamFactory;
         this.serializer = serializer;
      }

      public PofStream CreatePofStream(Stream stream) {
         return CreatePofStream(streamFactory.CreateFromStream(stream));
      }

      public PofStream CreatePofStream(IStream stream) {
         var reader = new PofStreamReaderImpl(serializer, stream);
         var writer = new PofStreamWriterImpl(serializer, stream);
         return new PofStreamImpl(stream, reader, writer);
      }

      public PofDispatcher CreateDispatcher(PofStream stream) {
         return new PofDispatcherImpl(threadingProxy, stream.Reader);
      }

      public PofDispatcher CreateDispatcher(PofStreamReader reader) {
         return new PofDispatcherImpl(threadingProxy, reader);
      }
   }
}