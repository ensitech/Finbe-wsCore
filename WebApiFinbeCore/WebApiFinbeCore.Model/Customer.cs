using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace WebApiFinbeCore.Model
{
    public class ClientesPlanPiso
    {
        /// <summary>
        /// Numero de cliente
        /// </summary>
        public string numeroCliente { get; set; }
        ///// <summary>
        ///// Nombre de cliente
        ///// </summary>
        ////public string nombreCliente { get; set; }
        /// <summary>
        /// Nombre de la empresa
        /// </summary>
        public string nombreEmpresa { get; set; }
        /// <summary>
        /// Razón social
        /// </summary>
        public string razonSocial { get; set; }
        /// <summary>
        /// Direccion fiscal
        /// </summary>
        public string direccionFiscal { get; set; }
        /// <summary>
        /// Tipo Disposicion en Linea
        /// </summary>
        public string tipoDisLinea { get; set; }
    }

    public class Aval
    {
        /// <summary>
        /// Numero de Cliente
        /// </summary>
        public string numeroCliente { get; set; }
        /// <summary>
        /// Nombre del Representante Legal (PM seria el nombre del apoderado) (PF es la misma persona)
        /// </summary>
        public string nombres { get; set; }
        /// <summary>
        /// Apellidos del Representante 
        /// </summary>
        public string apellidos { get; set; }

        /// <summary>
        /// RFC 
        /// </summary>
        public string rfc { get; set; }

        /// <summary>
        /// Dirección Fiscal
        /// </summary>
        public string direccionFiscal { get; set; }

        /// <summary>
        /// Correo
        /// </summary>
        public string correo { get; set; }
        /// <summary>
        /// Telefono
        /// </summary>
        public string telefono { get; set; }

    }

    public class RepresentanteLegal
    {
        /// <summary>
        /// Numero de Cliente
        /// </summary>
        public string numeroCliente { get; set; }
        /// <summary>
        /// Nombre del Representante Legal (PM seria el nombre del apoderado) (PF es la misma persona)
        /// </summary>
        public string nombres { get; set; }
        /// <summary>
        /// Apellidos del Representante 
        /// </summary>
        public string apellidos { get; set; }

        /// <summary>
        /// RFC 
        /// </summary>
        public string rfc { get; set; }

        /// <summary>
        /// Dirección Fiscal
        /// </summary>
        public string direccionFiscal { get; set; }

        /// <summary>
        /// Correo
        /// </summary>
        public string correo { get; set; }
        /// <summary>
        /// Telefono
        /// </summary>
        public string telefono { get; set; }
        /// <summary>
        /// Cargo
        /// </summary>
        public string cargo { get; set; }

    }
    /// <summary>
    /// Configurador
    /// </summary>
    public class Configurador
    {
        /// <summary>
        /// configDispLineaId
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Id del banco
        /// </summary>
        public string bancoId { get; set; }
        /// <summary>
        /// Nombre del banco
        /// </summary>
        public string bancoName { get; set; }
        /// <summary>
        /// Comision
        /// </summary>
        public string comision { get; set; }
        /// <summary>
        /// CLABE
        /// </summary>
        public string clabe { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public string cuenta { get; set; }
        /// <summary>
        /// Id persona moral
        /// </summary>
        public string personaMoralId { get; set; }
        /// <summary>
        /// Nombre persona moral
        /// </summary>
        public string personaMoralName { get; set; }
        /// <summary>
        /// Id persona fisica
        /// </summary>
        public string personaFisicaId { get; set; }
        /// <summary>
        /// Nombre persona fisica
        /// </summary>
        public string personaFisicaName { get; set; }
        /// <summary>
        /// Impuesto
        /// </summary>
        public int? impuesto { get; set; }
        /// <summary>
        /// Plazos de Pago Nombres
        /// </summary>
        public string plazosDePagoNombres { get; set; }
        /// <summary>
        /// Tasa de interes
        /// </summary>
        public string tasaInteres { get; set; }
        /// <summary>
        /// Dia de corte
        /// </summary>
        public int? diaDeCorte { get; set; }
        /// <summary>
        /// Linea de Credito Id
        /// </summary>
        public string lineaDeCreditoId { get; set; }
        /// <summary>
        /// Credito Empresarial
        /// </summary>
        public bool creditoEmpresarial { get; set; }
        /// <summary>
        /// Fecha de Vencimiento
        /// </summary>
        public DateTime fechaVencimiento { get; set; }
        /// <summary>
        /// Importe Disponible
        /// </summary>
        public decimal? importeDisponible { get; set; }
        /// <summary>
        /// Transaccioncurrency (Moneda) Id
        /// </summary>
        public string transaccionCurrencyId { get; set; }
        /// <summary>
        /// Transaccioncurrency (Moneda) Name
        /// </summary>
        public string transaccionCurrencyName { get; set; }
        /// <summary>
        /// Estatus
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// Tipo de Linea
        /// </summary>
        public string tipoDispLinea { get; set; }
        /// <summary>
        /// Linea de Credito
        /// </summary>
        public string lineaCredito { get; set; }
        /// <summary>
        /// Tasa Fija Variable
        /// </summary>
        public string tasaFijaVariable { get; set; }
        /// <summary>
        /// Valor Tasa Variable
        /// </summary>
        public decimal? valorTasaVariable { get; set; }
        /// <summary>
        /// Tasa Mora
        /// </summary>
        public decimal? tasaMora { get; set; }
        /// <summary>
        /// Día para cambio de mes
        /// </summary>
        public int? diaCambioMes { get; set; }
    }

    /// <summary>
    /// Configuracion
    /// </summary>
    public class Configuracion
    {
        /// <summary>
        /// Tipo de Linea
        /// </summary>
        public string tipoDispLinea { get; set; }
        /// <summary>
        /// Linea de Credito
        /// </summary>
        public string lineaCredito { get; set; }
        /// <summary>
        /// Monto MAximo
        /// </summary>
        public decimal montoMaximo { get; set; }
        /// <summary>
        /// Disponible
        /// </summary>
        public decimal disponible { get; set; }
        /// <summary>
        /// Monto Restante
        /// </summary>
        public decimal montoRestante { get; set; }
        /// <summary>
        /// Dia de Corte
        /// </summary>
        public int? diaDeCorte { get; set; }
        /// <summary>
        /// IVa General
        /// </summary>
        public decimal? ivaGeneral { get; set; }
        /// <summary>
        /// Impuesto
        /// </summary>
        public decimal? impuesto { get; set; }
        /// <summary>
        /// Comision
        /// </summary>
        public string comision { get; set; }
        /// <summary>
        /// Plazos
        /// </summary>
        public string plazos { get; set; }
        /// <summary>
        /// Error
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// Error Exigibles
        /// </summary>
        public string errorExigibles { get; set; }
        /// <summary>
        /// Credito Empresarial
        /// </summary>
        public bool creditoEmpresarial { get; set; }
        /// <summary>
        /// Banco
        /// </summary>
        public string banco { get; set; }
        /// <summary>
        /// Cuenta
        /// </summary>
        public string cuenta { get; set; }
        /// <summary>
        /// CLABE
        /// </summary>
        public string clabe { get; set; }
        /// <summary>
        /// Producto
        /// </summary>
        public string producto { get; set; }
        /// <summary>
        /// Tasa Moratorios
        /// </summary>
        public decimal? tasaMora { get; set; }
        /// <summary>
        /// Intereses
        /// </summary>
        public decimal intereses { get; set; }
        /// <summary>
        /// Tiene Saldos Pendientes
        /// </summary>
        public bool tienesSaldosPendientes { get; set; }
        /// <summary>
        /// Tasa Fija Variable
        /// </summary>
        public string tasaFijaVariable { get; set; }
        /// <summary>
        /// Valor Tasa Variable
        /// </summary>
        public decimal? valorTasaVariable { get; set; }
        /// <summary>
        /// Tasa de interes
        /// </summary>
        public string tasaInteres { get; set; }
        /// <summary>
        /// Puede Disponer
        /// </summary>
        public bool puedeDisponer { get; set; }
    }
    /// <summary>
    /// Login
    /// </summary>
    public class Login
    {
        /// <summary>
        /// Autenticacion
        /// </summary>
        public int autenticacion { get; set; }
        /// <summary>
        /// MAX Intentos
        /// </summary>
        public int maxIntentos { get; set; }
    }

    /// <summary>
    /// Bitacora Disposicion
    /// </summary>
    public class BitacoraDisposicion
    {
        /// <summary>
        /// Cantidad Dispuesta
        /// </summary>
        public decimal? cantidadDispuesta { get; set; }
        /// <summary>
        /// FEcha Disposicion
        /// </summary>
        public string fechaDisposicion { get; set; }
        /// <summary>
        /// Plazo
        /// </summary>
        public int plazo { get; set; }
        /// <summary>
        /// Tipo de CAlculo
        /// </summary>
        public string tipoCalculo { get; set; }
        /// <summary>
        /// En Tramite
        /// </summary>
        public bool enTramite { get; set; }
        /// <summary>
        /// En calculo
        /// </summary>
        public bool enCalculo { get; set; }
        /// <summary>
        /// Notifico usuario
        /// </summary>
        public bool notificoUsuario { get; set; }
        /// <summary>
        /// Credito Empresarial
        /// </summary>
        public bool creditoEmpresarial { get; set; }
        /// <summary>
        /// Periodos Gracia
        /// </summary>
        public int periodosGracia { get; set; }
        /// <summary>
        /// Numero Disposicion
        /// </summary>
        public string numeroDisposicion { get; set; }
    }

    /// <summary>
    /// Bitacora
    /// </summary>
    public class Bitacora
    {
        /// <summary>
        /// Credito
        /// </summary>
        public string credito { get; set; }
        /// <summary>
        /// Disposicion
        /// </summary>
        public string disposicion { get; set; }
        /// <summary>
        /// Nombre
        /// </summary>
        public string name { get; set; }
    }

    public class Credito
    {
        [JsonProperty("credito")]
        public string credito { get; set; }
        [JsonProperty("tipocredito")]
        public string TipoCredito { get; set; }
        [JsonProperty("producto")]
        public string Producto { get; set; }
        [JsonProperty("tipoproducto")]
        public string TipoProducto { get; set; }
        [JsonProperty("subtipoproducto")]
        public string SubtipoProducto { get; set; }
        [JsonProperty("moneda")]
        public string Moneda { get; set; }
        [JsonProperty("montocredito")]
        public decimal MontoCredito { get; set; }
        [JsonProperty("diasvencido")]
        public int DiasVencido { get; set; }
        [JsonProperty("cuotaspagadas")]
        public int CuotasPagadas { get; set; }
        [JsonProperty("estado")]
        public string Estado { get; set; }
        [JsonProperty("capitalvigente")]
        public decimal CapitalVigente { get; set; }
        [JsonProperty("capitalexigible")]
        public decimal CapitalExigible { get; set; }
        [JsonProperty("interesdevengado")]
        public decimal InteresDevengado { get; set; }
        [JsonProperty("interesexigible")]
        public decimal InteresExigible { get; set; }
        [JsonProperty("interesmoratorio")]
        public decimal InteresMoratorio { get; set; }
        [JsonProperty("otros")]
        public decimal Otros { get; set; }
        [JsonProperty("saldo")]
        public decimal Saldo { get; set; }
        [JsonProperty("plazo")]
        public int Plazo { get; set; }
        [JsonProperty("tasafija")]
        public decimal TasaFija { get; set; }
        [JsonProperty("tasavariable")]
        public string TasaVariable { get; set; }
        [JsonProperty("puntostasa")]
        public decimal PuntosTasa { get; set; }
        [JsonProperty("factortasa")]
        public string FactorTasa { get; set; }
        [JsonProperty("lineacredito")]
        public string LineaCredito { get; set; }
        [JsonProperty("disponiblelinea")]
        public decimal DisponibleLinea { get; set; }
        [JsonProperty("importeultimopago")]
        public decimal ImporteUltimoPago { get; set; }
        [JsonProperty("fechainicial")]
        public string FechaInicial { get; set; }
        [JsonProperty("fechafinal")]
        public string FechaFinal { get; set; }
        [JsonProperty("fechaultimopago")]
        public string FechaUltimoPago { get; set; }
        [JsonProperty("valortasavariable")]
        public decimal ValorTasaVariable { get; set; }
        [JsonProperty("cuotafija")]
        public decimal CuotaFija { get; set; }
        [JsonProperty("grupoimpuestointeres")]
        public string GrupoImpuestoInteres { get; set; }
        [JsonProperty("valorgrupoimpuestointeres")]
        public decimal ValorGrupoImpuestoInteres { get; set; }
        [JsonProperty("fechamasvencida")]
        public string FechaMasVencida { get; set; }
        [JsonProperty("importesigpago")]
        public decimal ImporteSigPago { get; set; }
        [JsonProperty("fechasigpago")]
        public string FechaSigPago { get; set; }

    }

    public class CreditoList
    {
        [JsonProperty("creditos")]
        public List<Credito> creditos { get; set; }
    }

    public class CreditoRolesList
    {
        [JsonProperty("creditos")]
        public List<CreditoRol> creditos { get; set; }
        public CreditoRolesList()
        {
            creditos = new List<CreditoRol>();
        }
    }

    public class CreditoRol
    {
        
        public string Credito { get; set; }
        public string Rol { get; set; }
        public string TipoDeContrato { get; set; }
        public decimal MontoInversion { get; set; }
    }

    public class Cliente
    {
        public string codigoCliente { get; set; }
        public string nombre { get; set; }
        public string rfc { get; set; }
    }

    public class CuentaCorreo
    {
        public string NumeroCuenta { get; set; }
        public string NombreORazonSocial { get; set; }
        [JsonIgnore]
        public Guid ClienteId { get; set; }
        [JsonIgnore]
        public bool PersonaFisica { get; set; }
        [JsonIgnore]
        public bool Creditos { get; set; }
    }
}
    
