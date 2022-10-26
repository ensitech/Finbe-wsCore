using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    /// <summary>
    /// Linea de Credito
    /// </summary>
    public class LineaCredito
    {
        /// <summary>
        /// Número de Linea
        /// </summary>
        public string numeroLinea { get; set; }
        /// <summary>
        /// Tipo de Linea
        /// </summary>
        public string tipoLinea { get; set; }
    }
    /// <summary>
    /// Informacion de una linea de credito
    /// </summary>
    public class InformacionLineaCredito
    {
        /// <summary>
        /// Número de Linea
        /// </summary>
        public string numeroLinea { get; set; }
        /// <summary>
        /// Productos
        /// </summary>
        public List<Producto> productos { get; set; }
        /// <summary>
        /// Tipo de Linea
        /// </summary>
        public string tipoLinea { get; set; }
        /// <summary>
        /// Estatus
        /// </summary>
        public string estatus { get; set; }
        /// <summary>
        /// Fecha de Inicio
        /// </summary>
        public string fechaInicio { get; set; }
        /// <summary>
        /// Fecha de Vencimiento
        /// </summary>
        public string fechaVencimiento { get; set; }
        /// <summary>
        /// Fecha de Recalificacion
        /// </summary>
        public string fechaRecalificacion { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        public InformacionLineaCredito()
        {
            productos = new List<Producto>();
        }
    }

    public class Producto
    {
        public string descripcion { get; set; }
        public decimal saldoDisponible { get { return importeDisponible - importeDispuesto + importeRevolvente; }}
        public decimal importeDisponible { get; set; }
        public decimal importeDispuesto { get; set; }
        public decimal importeRevolvente { get; set; }
        public int disposicionesRestantes { get; set; }
    }
    /// <summary>
    /// Estado de cuenta
    /// </summary>
    public class EstadoCuentaCredito
    {
        /// <summary>
        /// Numero del Credito
        /// </summary>
        [JsonProperty("credito")]
        public string credito { get; set; }
        /// <summary>
        /// Listado de estados de cuenta
        /// </summary>
        [JsonProperty("EstadoCuenta")]
        public List<InformacionEstadoCuenta> estadoCuenta { get; set; }
    }
    /// <summary>
    /// Informacion de un estado de cuenta
    /// </summary>
    public class InformacionEstadoCuenta
    {
        /// <summary>
        /// Fecha de Inicio
        /// </summary>
        [JsonProperty("fechainicial")]
        public string fechaInicial { get; set; }
        /// <summary>
        /// Fecha Final
        /// </summary>
        [JsonProperty("fechafinal")]
        public string fechaFinal { get; set; }
        /// <summary>
        /// Estado de cuenta
        /// </summary>
        [JsonProperty("estadocuenta")]
        public string estadocuenta { get; set; }
        /// <summary>
        /// Movimientos
        /// </summary>
        [JsonProperty("movimientos")]
        public string movimientos { get; set; }
    }
    /// <summary>
    /// Factura
    /// </summary>
    public class FacturaCredito
    {
        /// <summary>
        /// Numero del Credito
        /// </summary>
        [JsonProperty("credito")]
        public string credito { get; set; }
        /// <summary>
        /// Listado de estados de cuenta
        /// </summary>
        [JsonProperty("documentos")]
        public List<InformacionDocumento> documentos { get; set; }
    }
    /// <summary>
    /// Documento
    /// </summary>
    public class InformacionDocumento
    {
        /// <summary>
        /// Tipo
        /// </summary>
        [JsonProperty("tipo")]
        public string tipo { get; set; }
        /// <summary>
        /// Folio
        /// </summary>
        [JsonProperty("folio")]
        public string folio { get; set; }
        /// <summary>
        /// Fecha
        /// </summary>
        [JsonProperty("fecha")]
        public string fecha { get; set; }
        /// <summary>
        /// Descripcion
        /// </summary>
        [JsonProperty("descripcion")]
        public string descripcion { get; set; }
        /// <summary>
        /// Importe
        /// </summary>
        [JsonProperty("importe")]
        public string importe { get; set; }
        /// <summary>
        /// Contenido XML
        /// </summary>
        [JsonProperty("xml")]
        public string xml { get; set; }
    }

    public class CuotaCapital
    {
        [JsonProperty("numcuotas")]
        public int numcuotas { get; set; }
        [JsonProperty("mensualidad")]
        public decimal mensualidad { get; set; }
        [JsonProperty("saldo")]
        public decimal saldo { get; set; }
        [JsonProperty("cuotas")]
        public List<Cuota> cuotas { get; set; }
    }

    public class Cuota
    {
        [JsonProperty("cuota")]
        public int numero { get; set; }
        [JsonProperty("fecha")]
        public string fecha { get; set; }
        [JsonProperty("capital")]
        public decimal capital { get; set; }
        [JsonProperty("interes")]
        public decimal interes { get; set; }
        [JsonProperty("pago")]
        public decimal pago { get; set; }
        [JsonProperty("saldofinal")]
        public decimal saldofinal { get; set; }
    }

    public class SaldosCredito
    {
        [JsonProperty("creditos")]
        public List<SaldoCredito> creditos { get; set; }
    }

    public class SaldoCredito
    {
        [JsonProperty("credito")]
        public string credito { get; set; }
        [JsonProperty("saldoactual")]
        public decimal saldoactual { get; set; }
        [JsonProperty("saldocorte")]
        public decimal saldocorte { get; set; }
        [JsonProperty("pagominimo")]
        public decimal pagominimo { get; set; }
        [JsonProperty("tipo")]
        public string tipo { get; set; }
        [JsonProperty("fecha")]
        public string fecha { get; set; }
        [JsonProperty("cia")]
        public string compania { get; set; }

    }

    public class Moneda
    {
        /// <summary>
        /// ID de la moneda
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Codigo de Moenda
        /// </summary>
        public string Codigo { get; set; }
    }

    public class Modelo
    {
        /// <summary>
        /// ID de la marca
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Codigo de Modelo
        /// </summary>
        public string Name { get; set; }
    }

    public class Marca
    {
        /// <summary>
        /// ID de la Marca
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Codigo de Marca
        /// </summary>
        public string Name { get; set; }
    }

    public class TipoAutomovil
    {
        /// <summary>
        /// ID de la tipo
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Codigo de Tipo Automovil
        /// </summary>
        public string Name { get; set; }
    }

    public class Concesionaria
    {
        /// <summary>
        /// ID de la tipo
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Codigo de Tipo Automovil
        /// </summary>
        public string Name { get; set; }
    }

    public class Disposicion
    {
        public string Id { get; set; }
        public string LineaCreditoId { get; set; }
        public string MonedaId { get; set; }
        public double Monto { get; set; }
    }

    public class ConfigPago
    {
        public string semilla { get; set; }
        public string xmlm { get; set; }
        public string urlwebpay { get; set; }
        public string urlresponse { get; set; }
        public string idcompany { get; set; }
    }

    public class InformacionCreditos
    {
        [JsonProperty("data")]
        public List<InformacionCredito> informacionCreditos { get; set; }
    }

    public class InformacionCredito
    {
        [JsonProperty("crédito")]
        public string Credito { get; set; }
        [JsonProperty("producto")]
        public string Producto { get; set; }
        [JsonProperty("capital_inicial")]
        public decimal CapitalInicial { get; set; }
        [JsonProperty("saldo_capital")]
        public decimal SaldoCapital { get; set; }
        [JsonProperty("saldo_vencido")]
        public decimal SaldoVencido { get; set; }
        [JsonProperty("otros_vencidos")]
        public decimal OtrosVencidos { get; set; }
        [JsonProperty("interes_vencido")]
        public decimal InteresesVencido { get; set; }
        [JsonProperty("intereses_moratorio")]
        public decimal InteresesMoratorio { get; set; }
        [JsonProperty("fechainicio")]
        public string FechaInicio { get; set; }
        [JsonProperty("coutas_vencidas")]
        public int CuotasVencidas { get; set; }
        [JsonProperty("coutas_pendientes")]
        public int CuotasPendientes { get; set; }
        [JsonProperty("plazo")]
        public int Plazo { get; set; }
        [JsonProperty("prox_vencimiento")]
        public string ProximoVencimiento { get; set; }
    }

    public class InformacionContrato
    {
        public string FechaFirmaContrato { get; set; }
        public string CiudadFirma { get; set; }
    }

    public class PagosPendientesList
    {
        [JsonProperty("pagospendientes")]
        public List<PagoPendienteCr> pagosPendientes { get; set; }
    }

    public class PagoPendienteCr
    {
        public int periodo { get; set; }
        public string fechadepago { get; set; }
        public decimal pagopendiente { get; set; }
        public decimal capitalvigente { get; set; }
        public decimal capitalvencido { get; set; }
        public decimal interesdevengado { get; set; }
        public decimal interesexigible { get; set; }
        public decimal impuesto {get;set;}
        public decimal gastocobranza { get; set; }
        public decimal otros { get; set; }
        public decimal tasamoratoria { get; set; }
        public decimal capitalprogramado { get; set; }
    }
}
