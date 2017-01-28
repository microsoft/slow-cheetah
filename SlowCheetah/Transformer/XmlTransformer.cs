// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System.Diagnostics.Contracts;
using System.IO;
using Microsoft.Web.XmlTransform;
using SlowCheetah.Exceptions;


namespace SlowCheetah
{
    /// <summary>
    /// Transforms XML files utilizing Microsoft Web XmlTransform library
    /// </summary>
    public class XmlTransformer : ITransformer
    {
        public void Transform(string source, string transform, string destination)
        {
            //Parameter validation
            Contract.Requires(!string.IsNullOrWhiteSpace(source));
            Contract.Requires(!string.IsNullOrWhiteSpace(transform));
            Contract.Requires(!string.IsNullOrWhiteSpace(destination));

            //TO DO: Logging
            //File validation
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("File to transform not found", source);
            }
            if (!File.Exists(transform))
            {
                throw new FileNotFoundException("Transform file not found", transform);
            }

            using (XmlTransformableDocument document = new XmlTransformableDocument())
            using (XmlTransformation transformation = new XmlTransformation(transform))
            {
                document.PreserveWhitespace = true;
                document.Load(source);

                var success = transformation.Apply(document);
                if (!success)
                {
                    string message = string.Format(
                        "There was an unknown error trying while trying to apply the transform. Source file='{0}',Transform='{1}', Destination='{2}'",
                        source, transform, destination);
                    throw new TransformFailedException(message);
                }

                document.Save(destination);
            }
        }
    }
}
