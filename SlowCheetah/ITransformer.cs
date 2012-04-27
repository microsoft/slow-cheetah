namespace SlowCheetah.VisualStudio {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface ITransformer {
        void Transform(string source, string transform, string destination);
    }
}
