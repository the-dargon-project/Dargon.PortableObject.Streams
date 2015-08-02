using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;
using NMockito;
using Xunit;

namespace Dargon.PortableObjects.Streams {
   public class RyuPackageTests : NMockitoInstance {
      [Fact]
      public void Run() {
         var ryu = new RyuFactory().Create();
         ryu.Setup();
         AssertTrue(ryu.Get<PofStreamsFactory>() is PofStreamsFactoryImpl);
      }
   }
}
