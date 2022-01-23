using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultimate_Triclustering
{
    class ConstraintClass // для хранения ограничений для DataPeeler'а
    {
        public ConstraintClass() { }
        public ConstraintClass(string con)
        {
            constraint = con;
        }
        private string constraint;

        public string Constraint
        {
            get { return constraint; }
            set { constraint = value; }
        }

        public override string ToString()
        {
            return constraint;
        }
    }
}
