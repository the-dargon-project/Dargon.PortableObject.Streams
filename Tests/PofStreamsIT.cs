using System.IO;
using System.Linq;
using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.PortableObject.Streams {
   public class PofStreamsIT : NMockitoInstance {
      private readonly IStreamFactory streamFactory;
      private readonly IThreadingProxy threadingProxy;
      private readonly IPofSerializer serializer;

      public PofStreamsIT() {
         streamFactory = new StreamFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);

         serializer = new PofSerializer(new PofContext().With(x => {
            x.RegisterPortableObjectType(1, typeof(TestClass));
         }));
      }

      [Fact]
      public void ReadTest() {
         const int kMessageCount = 1024 * 64;
         const string kMessage = "herp";

         using (var ms = streamFactory.CreateMemoryStream()) {
            for (var i = 0; i < kMessageCount; i++) {
               serializer.Serialize(ms.GetWriter(), new TestClass(kMessage));
            }
            ms.Position = 0;

            var factory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, serializer);
            var pofStream = factory.CreatePofStream(ms);
            var reader = pofStream.Reader;
            var tasks = Util.Generate(kMessageCount, i => reader.ReadAsync());
            tasks.Select(t => (TestClass)t.Result).ForEach(x => AssertEquals(x.Value, kMessage));
         }
      }

      [Fact]
      public void WriteTest() {
         const int kMessageCount = 1024 * 64;
         const string kMessage = "herp";

         using (var ms = streamFactory.CreateMemoryStream()) {
            var factory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, serializer);
            var pofStream = factory.CreatePofStream(ms);
            var writer = pofStream.Writer;
            var writingTasks = Util.Generate(kMessageCount, i => writer.WriteAsync(new TestClass(kMessage)));
            writingTasks.ForEach(x => x.Wait());

            ms.Position = 0;

            for (var i = 0; i < kMessageCount; i++) {
               var obj = serializer.Deserialize<TestClass>(ms.GetReader());
               AssertEquals(kMessage, obj.Value);
            }
         }
      }

      public class TestClass : IPortableObject {
         private string value;

         public TestClass() { }

         public TestClass(string value) {
            this.value = value;
         }

         public string Value { get { return value; } }

         public void Serialize(IPofWriter writer) {
            writer.WriteString(0, value);
         }

         public void Deserialize(IPofReader reader) {
            value = reader.ReadString(0);
         }
      }
   }
}
