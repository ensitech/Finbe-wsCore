using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    /// <summary>
    /// Centro Costo
    /// </summary>
    public class CentroCosto
    { 
        /// <summary>
        /// Data Area
        /// </summary>
        public string dataareaid { get; set; }
        /// <summary>
        /// Mayor
        /// </summary>
        public string ctacentro { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public string sctacentro { get; set; }
        /// <summary>
        /// SubCuenta
        /// </summary>
        public string dimcc { get; set; }
    }
}
