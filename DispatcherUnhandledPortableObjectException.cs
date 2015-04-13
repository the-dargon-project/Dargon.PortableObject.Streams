using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;

namespace Dargon.PortableObject.Streams {
   public class DispatcherUnhandledPortableObjectException : Exception {
      public DispatcherUnhandledPortableObjectException(Type type) : base(GenerateErrorMessage(type)) {
      }

      private static string GenerateErrorMessage(Type type) {
         return "Dispatcher did not have handler for Portable Object of type '{0}'.".F(type.FullName);
      }
   }
}
