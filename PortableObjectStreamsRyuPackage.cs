using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon.PortableObjects.Streams {
   public class PortableObjectStreamsRyuPackage : RyuPackageV1 {
      public PortableObjectStreamsRyuPackage() {
         Singleton<PofStreamsFactory, PofStreamsFactoryImpl>();
      }
   }
}