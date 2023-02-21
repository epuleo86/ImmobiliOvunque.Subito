using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImmobiliOvunque.Subito
{
    class PrefixedWriter : TextWriter
    {
        private TextWriter originalOut;

        public PrefixedWriter()
        {
            originalOut = Console.Out;
        }

        public override Encoding Encoding
        {
            get { return new ASCIIEncoding(); }
        }
        public override void WriteLine(string message)
        {
            originalOut.WriteLine(string.Format("{0} {1}", DateTime.Now, message));
        }
        public override void Write(string message)
        {
            originalOut.Write(string.Format("{0} {1}", DateTime.Now, message));
        }
    }
}
