using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cards;

namespace u
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Card card = new Card(123, 12, 2030, 4111111111111111);

            Task task = card.getEncryptKey();

        }
    }
}
