namespace SlowCheetah.VisualStudio.Exceptions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
   
    [Serializable]
    public class SlowCheetahCustomException : Exception {
        public SlowCheetahCustomException() { }
        public SlowCheetahCustomException(string message) : base(message) { }
        public SlowCheetahCustomException(string message, Exception inner) : base(message, inner) { }
        public SlowCheetahCustomException(string message, Exception inner,string customMessage) : base(message, inner) {
            this.CustomMessage = customMessage;
        }

        public string CustomMessage { get; private set; }

        protected SlowCheetahCustomException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
