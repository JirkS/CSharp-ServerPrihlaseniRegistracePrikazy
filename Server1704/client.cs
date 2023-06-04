using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server1704
{
    public class client
    {

        private string jmeno;
        private string heslo;

        public client(string jmeno, string heslo)
        {
            this.jmeno = jmeno;
            this.heslo = heslo;
        }

        public string Jmeno
        {
            get { return jmeno; }
            set { jmeno = value; }
        }

        public string Heslo
        {
            get { return heslo; }
            set { heslo = value; }
        }

        public override string ToString()
        {
            return "Uzivatel: " + jmeno + " " + heslo;
        }
    }
}
