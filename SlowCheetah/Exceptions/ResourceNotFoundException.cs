namespace SlowCheetah.VisualStudio.Exceptions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    [Serializable]
    public class ResourceNotFoundException : Exception {
        public ResourceNotFoundException() { }
        public ResourceNotFoundException(string message) : base(message) { }
        public ResourceNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ResourceNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
