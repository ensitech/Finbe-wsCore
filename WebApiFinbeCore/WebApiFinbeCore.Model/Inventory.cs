using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    public class EstatusCredito
    {
        /// <summary>
        /// Número de Crédito
        /// </summary>
        public string noCredito { get; set; }
        /// <summary>
        /// Número de Persona Fisica o Persona Moral
        /// </summary>
        public string noCliente { get; set; }
        /// <summary>
        /// Nombre de Cliente
        /// </summary>
        public string nombreCliente { get; set; }
        /// <summary>
        /// Linea de Credito
        /// </summary>
        public string lineaCredito { get; set; }
        /// <summary>
        /// Tipo de Linea de Credito
        /// </summary>
        public string tipoDispLinea { get; set; }
        /// <summary>
        /// Marca del Vehiculo
        /// </summary>
        public string marca { get; set; }
        /// <summary>
        /// Modelo del Vehiculo
        /// </summary>
        public string modelo { get; set; }
        /// <summary>
        /// Tipo de Unidad
        /// </summary>
        public string tipoUnidad { get; set; }
        /// <summary>
        /// Descripcion de la unidad
        /// </summary>
        public string descripcion { get; set; }
        /// <summary>
        /// VIN
        /// </summary>
        public string vin { get; set; }
        /// <summary>
        /// Codigo de Moneda
        /// </summary>
        public string codigoMoneda { get; set; }
        /// <summary>
        /// Tasa
        /// </summary>
        public string tasa { get; set; }
        /// <summary>
        /// Capital
        /// </summary>
        public decimal? monto { get; set; }
        /// <summary>
        /// Plazo
        /// </summary>
        public string plazo { get; set; }
        /// <summary>
        /// Proximo Pago
        /// </summary>
        public string proximoPago { get; set; }
        /// <summary>
        /// Inicio de Crédito
        /// </summary>
        public string inicioCredito { get; set; }
        /// <summary>
        /// Fin de Crédito
        /// </summary>
        public string finCredito { get; set; }
    }

    public class InformacionVin
    {
        [JsonProperty("conteo")]
        public int conteo { get; set; }
        [JsonProperty("detalle")]
        public List<DetalleInformacionVin> detalle { get; set; }
    }

    public class DetalleInformacionVin
    {
        [JsonProperty("credito")]
        public string credito { get; set; }
        [JsonProperty("estatus")]
        public string estatus { get; set; }
    }

}
