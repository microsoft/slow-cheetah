namespace SlowCheetah.VisualStudio.Exceptions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class TransformFailedException : Exception {
        public TransformFailedException() { }
        public TransformFailedException(string message) : base(message) { }
        public TransformFailedException(string message, Exception inner) : base(message, inner) { }
        protected TransformFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
