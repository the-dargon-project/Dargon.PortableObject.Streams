using System.Threading.Tasks;
using ItzWarty.IO;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class PofStreamTests : NMockitoInstance {
      private readonly PofStream testObj;

      [Mock] private readonly IStream stream = null;
      [Mock] private readonly PofStreamReader reader = null;
      [Mock] private readonly PofStreamWriter writer = null;

      public PofStreamTests() {
         this.testObj = new PofStreamImpl(stream, reader, writer);
      }

      [Fact]
      public void Reader_ReflectsConstructerArgumentsTest() {
         AssertEquals(reader, testObj.Reader);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Writer_ReflectsConstructerArgumentsTest() {
         AssertEquals(writer, testObj.Writer);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Read_DelegatesToReaderTest() {
         object returnValue = new object();
         When(reader.Read()).ThenReturn(returnValue);
         AssertEquals(returnValue, testObj.Read());
         Verify(reader).Read();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ReadAsync_DelegatesToReaderTest() {
         Task<object> returnValue = Task.FromResult(new object());
         When(reader.ReadAsync()).ThenReturn(returnValue);
         AssertEquals(returnValue, testObj.ReadAsync());
         Verify(reader).ReadAsync();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ReadAsync_WithCancellationToken_DelegatesToReaderTest() {
         var cancellationToken = CreateMock<ICancellationToken>();
         Task<object> returnValue = Task.FromResult(new object());
         When(reader.ReadAsync(cancellationToken)).ThenReturn(returnValue);
         AssertEquals(returnValue, testObj.ReadAsync(cancellationToken));
         Verify(reader).ReadAsync(cancellationToken);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Write_DelegatesToReaderTest() {
         object writtenValue = new object();
         testObj.Write(writtenValue);
         Verify(writer).Write(writtenValue);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void WriteAsync_DelegatesToReaderTest() {
         Task returnValue = Task.Delay(0);
         object writtenValue = new object();
         When(writer.WriteAsync(writtenValue)).ThenReturn(returnValue);
         AssertEquals(returnValue, testObj.WriteAsync(writtenValue));
         Verify(writer).WriteAsync(writtenValue);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void WriteAsync_WithCancellationToken_DelegatesToReaderTest() {
         var cancellationToken = CreateMock<ICancellationToken>();
         Task returnValue = Task.Delay(0);
         object writtenValue = new object();
         When(writer.WriteAsync(writtenValue, cancellationToken)).ThenReturn(returnValue);
         AssertEquals(returnValue, testObj.WriteAsync(writtenValue, cancellationToken));
         Verify(writer).WriteAsync(writtenValue, cancellationToken);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Dispose_DelegatesToDependenciesOnFirstRun_Test() {
         testObj.Dispose();
         Verify(stream).Dispose();
         Verify(reader).Dispose();
         Verify(writer).Dispose();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Dispose_DoesNothingAfterFirstRun_Test() {
         testObj.Dispose();
         ClearInteractions();

         testObj.Dispose();
         VerifyNoMoreInteractions();
      }
   }
}
