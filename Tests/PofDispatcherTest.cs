using System;
using System.Threading;
using System.Threading.Tasks;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class PofDispatcherTest : NMockitoInstance {
      [Mock] private readonly PofStreamReader pofStreamReader = null;

      private readonly A a = new A();
      private readonly B b = new B();
      private readonly C c = new C();

      private PofDispatcherImpl testObj;

      public PofDispatcherTest() {
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);

         testObj = new PofDispatcherImpl(threadingProxy, pofStreamReader);
      }

      [Fact]
      public void Run() {
         var dummy = CreateMock<DummyClass>();

         testObj.RegisterHandler<A>(dummy.HandleA);
         testObj.RegisterHandler<B>(dummy.HandleB);

         var messagesProcessedLatch = new CountdownEvent(1);

         When(pofStreamReader.ReadAsync(Any<ICancellationToken>())).ThenReturn(
            Task.FromResult((object)a),
            Task.FromResult((object)b),
            Task.FromResult((object)c)
         );
         When(() => pofStreamReader.Dispose()).Exec(() => messagesProcessedLatch.Signal()).ThenReturn(null);

         testObj.Start();

         AssertTrue(messagesProcessedLatch.Wait(TimeSpan.FromSeconds(5)));
         Verify(dummy, Once(), Whenever()).HandleA(a);
         Verify(dummy, Once(), AfterPrevious()).HandleB(b);
         Verify(pofStreamReader, Once(), AfterPrevious()).Dispose();
         Verify(pofStreamReader, Times(3)).ReadAsync(Any<ICancellationToken>());
         VerifyNoMoreInteractions();
      }

      public class A { }
      public class B { }
      public class C { }

      public interface DummyClass {
         void HandleA(A parameter);
         void HandleB(B parameter);
      }
   }
}
