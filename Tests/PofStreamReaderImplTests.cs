using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.IO;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class PofStreamReaderImplTests : NMockitoInstance {
      [Mock] private readonly IPofSerializer serializer = null;
      [Mock] private readonly IStream stream = null;

      private readonly PofStreamReaderImpl testObj;

      public PofStreamReaderImplTests() {
         testObj = new PofStreamReaderImpl(serializer, stream);
      }

      [Fact]
      public void Properties_ReflectConstructorArguments_Test() {
         AssertEquals(stream, testObj.BaseStream);
         VerifyNoMoreInteractions();
      }
   }
}
