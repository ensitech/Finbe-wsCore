using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    /// <summary>
    /// Disponible por producto
    /// </summary>
    public class DisponiblePorProducto
    {
        /// <summary>
        /// Importe Disponible
        /// </summary>
        public decimal importeDisponible { get; set; }
        /// <summary>
        /// Importe Dispuesto
        /// </summary>
        public decimal importeDispuesto { get; set; }
        /// <summary>
        /// Importe Revolvente
        /// </summary>
        public decimal importeRevolvente { get; set; }
    }

    /// <summary>
    /// Disposicion
    /// </summary>
    public class DisposicionResponse
    {
        /// <summary>
        /// Numero de Disposicion
        /// </summary>
        public string numeroDisposicion { get; set; }
        /// <summary>
        /// Result
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// Comision e Iva
        /// </summary>
        public string comisionEIva { get; set; }
        /// <summary>
        /// Error
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// ID de Garantia
        /// </summary>
        public string numeroGarantia { get; set; }
    }
}
