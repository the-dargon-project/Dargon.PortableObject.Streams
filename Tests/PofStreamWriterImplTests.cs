using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.IO;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class PofStreamWriterImplTests : NMockitoInstance {
      private readonly PofStreamWriterImpl testObj;

      [Mock] private readonly IPofSerializer serializer = null;
      [Mock] private readonly IStream stream = null;
      [Mock] private readonly IBinaryWriter writer = null;

      public PofStreamWriterImplTests() {
         testObj = new PofStreamWriterImpl(serializer, stream);

         When(stream.Writer).ThenReturn(writer);
      }

      [Fact]
      public void Write_DelegatesToSerializer() {
         var serializable = new object();
         testObj.Write(serializable);
         Verify(stream).Writer.Wrap();
         Verify(serializer).Serialize(writer, serializable);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Dispose_FirstInvocation_DelegatesToStream_Test() {
         testObj.Dispose();
         Verify(stream, Once()).Dispose();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void Dispose_SecondInvocation_DoesNothing_Test() {
         Dispose_FirstInvocation_DelegatesToStream_Test();

         testObj.Dispose();
         VerifyNoMoreInteractions();
      }
   }
}
