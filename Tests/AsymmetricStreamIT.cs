using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class AsymmetricStreamIT : NMockitoInstance {
      private readonly IStreamFactory streamFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly IPofSerializer serializer;

      public AsymmetricStreamIT() {
         streamFactory = new StreamFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);

         serializer = new PofSerializer(new PofContext().With(x => {
            x.RegisterPortableObjectType(1, typeof(PofStreamsIT.TestClass));
         }));
      }

      [Fact]
      public void Run() {
         const string kTestData = "Hello, World!";
         var factory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, serializer);
         var ms1 = streamFactory.CreateMemoryStream();
         var ms2 = streamFactory.CreateMemoryStream();
         
         // Write test data into ms1
         var helperStream = factory.CreatePofStream(ms1);
         helperStream.Write(kTestData);
         ms1.Position = 0;

         // Read from ms1 stream with mainStream
         var mainStream = factory.CreatePofStream(ms1.Reader, ms2.Writer);
         AssertEquals(kTestData, mainStream.Read<string>());

         // Write to ms2 stream with mainStream
         mainStream.Write(kTestData);

         // Assert the data was written to ms2 as expected
         ms2.Position = 0;
         helperStream = factory.CreatePofStream(ms2);
         AssertEquals(kTestData, helperStream.Read<string>());
      }
   }
}
