// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;

namespace SlowCheetah.Exceptions
{
    [Serializable]
    public class TransformFailedException : Exception
    {
        public TransformFailedException() { }
        public TransformFailedException(string message) : base(message) { }
        public TransformFailedException(string message, Exception inner) : base(message, inner) { }
        protected TransformFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
