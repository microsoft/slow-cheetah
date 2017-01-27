// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;
using Microsoft.Build.Framework;

namespace SlowCheetah
{
    public class TransformXml : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string Source { get; set; }

        [Required]
        public String Transform { get; set; }

        [Required]
        public String Destination { get; set; }

        public override bool Execute()
        {

            ITransformer transformer = new XmlTransformer();

            try
            {
                Log.LogMessage("Beginning transformation.");
                transformer.Transform(Source, Transform, Destination);
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
            }
            finally
            {
                Log.LogMessage(Log.HasLoggedErrors ?
                    "Transformation failed." :
                    "Transformation succeeded");
            }

            //TO DO: Transforms for different file types
            //TO DO: Logging level: catch exceptions or log inside transformer?

            return !Log.HasLoggedErrors;
        }
    }
}
