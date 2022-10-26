using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Domain
{
    public class ConectorCRMException : Exception
    {
        public ConectorCRMException()
        {
        }

        public ConectorCRMException(string message)
            : base(message)
        {
        }

        public ConectorCRMException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
