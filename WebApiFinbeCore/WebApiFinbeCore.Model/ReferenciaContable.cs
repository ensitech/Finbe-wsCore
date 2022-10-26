using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    /// <summary>
    /// Referencia Contable
    /// </summary>
    public class ReferenciaContable
    {
        /// <summary>
        /// Data Area
        /// </summary>
        public string dataareaid { get; set; }
        /// <summary>
        /// Mayor
        /// </summary>
        public string mayor { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public string cuenta { get; set; }
        /// <summary>
        /// SubCuenta
        /// </summary>
        public string subcuenta { get; set; }
        /// <summary>
        /// Ledger Account
        /// </summary>
        public string ledgeraccount { get; set; }
        /// <summary>
        /// Estructura Dim
        /// </summary>
        public string estructuradim { get; set; }
        /// <summary>
        /// Tipo Linea
        /// </summary>
        public string tipolinea { get; set; }
    }
}
