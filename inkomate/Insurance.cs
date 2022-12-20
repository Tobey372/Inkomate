using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inkomate
{
    internal class Insurance
    {
        protected int id;
        protected string name;
        protected int interval;
        private bool selected = false;
        private bool primary = false;

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                if (value >= 0)
                {
                    id = value;
                }
                else
                {
                    throw new Exception("Id can not be less than 0");
                }

            }
        }

        public string Name { get { return name; } set { name = value; } }

        public int Interval
        {
            get
            {
                return interval;
            }
            set
            {
                if (value > 0 && value <= 12)
                {
                    interval = value;
                }
                else
                {
                    throw new Exception("Interval (in months) must not be smaller than 1 or greater than 12");
                }

            }
        }

        public bool Selected { get { return selected; } set { selected = value; } }

        public bool Primary { get { return primary; } set { primary = value; } }

        public int DateBasePrimary 
        {
            get 
            { 
                if (primary)
                {
                    return 1;
                }
                return 0;
            } 
        }

        public Insurance(int id, string name, int interval)
        {
            ID = id;
            Name = name;
            Interval = interval;
        }

        public Insurance(string name, int interval)
        {
            ID = id;
            Name = name;
            Interval = interval;
        }

        public Insurance(Int64 id, String name, Int64 interval)
        {
            ID = (int)id;
            Name = (string)name;
            Interval = (int)interval;
        }

        public override bool Equals(object obj)
        {
            Insurance insurance = (Insurance)obj;

            if (insurance.Name == this.Name || insurance.ID == this.ID)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
