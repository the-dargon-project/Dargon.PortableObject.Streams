using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.Collections;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class PofDispatcherTests : NMockitoInstance {
      private readonly PofDispatcherImpl testObj;

      [Mock] private readonly PofStreamReader reader = null;
      [Mock] private readonly IConcurrentDictionary<Type, Action<object>> handlersByType = null;
      [Mock] private readonly IConcurrentSet<Action> shutdownHandlers = null;
      [Mock] private readonly ICancellationTokenSource dispatcherTaskCancellationTokenSource = null;

      public PofDispatcherTests() {
         testObj = new PofDispatcherImpl(reader, handlersByType, shutdownHandlers, dispatcherTaskCancellationTokenSource);
      }

      [Fact]
      public void ConstructorTest() {
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterHandler_NonGeneric_Test() {
         var parameterType = typeof(object);
         var handler = new Action<object>(o => { });
         testObj.RegisterHandler(parameterType, handler);
         Verify(handlersByType).TryAdd(parameterType, handler);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void RegisterHandler_Generic_Test() {
         object parameterObject = new object();
         bool handlerExecuted = false;
         var handler = new Action<object>(o => { handlerExecuted = o == parameterObject; });

         testObj.RegisterHandler(handler);

         var captor = new ArgumentCaptor<Action<object>>();
         Verify(handlersByType).TryAdd(Eq(typeof(object)), captor.GetParameter());
         VerifyNoMoreInteractions();

         AssertFalse(handlerExecuted);
         captor.Value(parameterObject);
         AssertTrue(handlerExecuted);
      }

      [Fact]
      public void RegisterShutdownHandler_Test() {
         var handler = new Action(() => { });
         testObj.RegisterShutdownHandler(handler);
         Verify(shutdownHandlers).Add(handler);
         VerifyNoMoreInteractions();
      }
   }
}
