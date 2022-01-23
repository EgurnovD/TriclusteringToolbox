using System;
using System.Net;
using System.Windows;

namespace Ultimate_Triclustering.Parsers
{
    public class ParseException : Exception
    {
        public string comment;

        public ParseException(string comment)
        {
            this.comment = comment;
        }
    }
}