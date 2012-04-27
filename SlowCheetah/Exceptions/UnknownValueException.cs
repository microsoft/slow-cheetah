namespace SlowCheetah.VisualStudio.Exceptions {
    using System;

    [Serializable]
    public class UnknownValueException : Exception {
        public UnknownValueException() { }
        public UnknownValueException(string message) : base(message) { }
        public UnknownValueException(string message, Exception inner) : base(message, inner) { }
        protected UnknownValueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
