using System;
using ItzWarty;

namespace Dargon.PortableObjects.Streams {
   public class DispatcherUnhandledPortableObjectException : Exception {
      public DispatcherUnhandledPortableObjectException(Type type) : base(GenerateErrorMessage(type)) {
      }

      private static string GenerateErrorMessage(Type type) {
         return "Dispatcher did not have handler for Portable Object of type '{0}'.".F(type.FullName);
      }
   }
}
