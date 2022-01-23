using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;

namespace Ultimate_Triclustering.Parsers
{
    public class Lexeme
    {
        public int type;
        public int index;
        public string buf;
        public double value;

	    public Lexeme(int type, int index, string buf, double value)
        {
            this.type = type;
            this.index = index;
            this.buf = buf;
            this.value = value;
        }
    }
}