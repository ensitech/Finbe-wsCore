using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    public class PolizaResponse
    {
        /// <summary>
        /// Variable que indica si en alguna de las polizas tiene error
        /// </summary>
        public bool TieneError { get; set; }
        /// <summary>
        /// Lista de polizas encontradas a partir de los datos de entrada
        /// </summary>
        public List<Poliza> Polizas { get; set; }
    }
    public class Poliza
    {
        /// <summary>
        /// AccountType
        /// </summary>
        public string AccountType { get; set; }
        /// <summary>
        /// DefaultDimensionDisplayValue
        /// </summary>
        public string DefaultDimensionDisplayValue { get; set; }
        /// <summary>
        /// AccountDisplayValue
        /// </summary>
        public string AccountDisplayValue { get; set; }
        /// <summary>
        /// TransDate
        /// </summary>
        public DateTime TransDate { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// CreditAmount
        /// </summary>
        public decimal CreditAmount { get; set; }
        /// <summary>
        /// Mes
        /// </summary>
        public decimal DebitAmount { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public string Document { get; set; }
        /// <summary>
        /// Sub Cuenta
        /// </summary>
        public string CurrencyCode { get; set; }
        /// <summary>
        /// Cuenta Area
        /// </summary>
        public int LineNumber { get; set; }
        /// <summary>
        /// Variable que indica si la poliza tiene error
        /// </summary>
        public string CenterCost { get; set; }
        /// <summary>
        /// Variable que indica si la poliza tiene error
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// Variable que indica si la poliza tiene error
        /// </summary>
        public string Branch { get; set; }
        /// <summary>
        /// Variable que indica si la poliza tiene error
        /// </summary>
        public string Transaction { get; set; }
        /// <summary>
        /// Variable que indica si la poliza tiene error
        /// </summary>
        public string IDGTO { get; set; }
        /// <summary>
        /// Variable que indica si la poliza tiene error
        /// </summary>
        public bool TieneError { get; set; }
        /// <summary>
        /// Mensaje de error
        /// </summary>
        public string Error { get; set; }
    }

    public class OpePoliza
    {
        /// <summary>
        /// Compañía
        /// </summary>
        public int cia { get; set; }
        /// <summary>
        /// Sucursal
        /// </summary>
        public int suc { get; set; }
        /// <summary>
        /// Formato
        /// </summary>
        public int formato { get; set; }
        /// <summary>
        /// Poliza
        /// </summary>
        public int poliza { get; set; }
        /// <summary>
        /// Año
        /// </summary>
        public int anio { get; set; }
        /// <summary>
        /// Mes
        /// </summary>
        public int mes { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public int cuentaDepto { get; set; }
        /// <summary>
        /// Sub Cuenta
        /// </summary>
        public int subCuentaDepto { get; set; }
        /// <summary>
        /// Cuenta Area
        /// </summary>
        public int cuentaArea { get; set; }
        /// <summary>
        /// Sub Cuenta Area
        /// </summary>
        public int subCuentaArea { get; set; }
        /// <summary>
        /// Mayor
        /// </summary>
        public int mayor { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public int cuenta { get; set; }

        /// <summary>
        /// Sub Cuenta
        /// </summary>
        public int subCuenta { get; set; }

        /// <summary>
        /// Fecha
        /// </summary>
        public DateTime fecha { get; set; }
        /// <summary>
        /// Descripción
        /// </summary>
        public string descripcion { get; set; }
        /// <summary>
        /// Centro de costo
        /// </summary>
        public int cuentaCentro { get; set; }
        /// <summary>
        /// Centro de costo
        /// </summary>
        public int subCuentaCentro { get; set; }
        /// <summary>
        /// Cargo
        /// </summary>
        public decimal cargo { get; set; }
        /// <summary>
        /// Abono
        /// </summary>
        public decimal abono { get; set; }
        /// <summary>
        /// Docto
        /// </summary>
        public string documento { get; set; }
    }
}
