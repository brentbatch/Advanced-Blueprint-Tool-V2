using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Extensions
{
    public static class IntExtensions
    {
        public static int mod(this int i, int j)
        {
            return (i + j) % j;
        }

    }
}
