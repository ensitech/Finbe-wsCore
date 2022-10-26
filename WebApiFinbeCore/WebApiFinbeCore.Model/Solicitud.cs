using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiFinbeCore.Model
{
    /// <summary>
    /// Solicitud de iMX
    /// </summary>
    public class Solicitud
    {

        /// <summary>
        /// Folio iMX
        /// </summary>
        [Required(AllowEmptyStrings =false)]
        public string Folio { get; set; } //folio imx
        /// <summary>
        /// Tipo de Credito
        /// </summary>
        [Required]
        public int TipoCredito { get; set; } // camiones o automovil
        /// <summary>
        /// Promotor asignado a la solicitud
        /// </summary>
        public string Promotor { get; set; }
        /// <summary>
        /// Persona solicitante puede ser fisica o moral
        /// </summary>
        [Required]
        public PersonaEvaluada PersonaEvaluada {get;set;} //solicitante
        /// <summary>
        /// Detalle de la financiacion
        /// </summary>
        [Required]
        public DetalleFinanciacion DetalleFinanciacion { get; set; } //detalle financiacion
    }

    /// <summary>
    /// Persona fisica o Moral
    /// </summary>
    public class PersonaEvaluada : DatosPersonaFisica
    {
        #region PERSONA MORAL

        /// <summary>
        /// Tipo de Persona
        /// </summary>
        [Required]
        public string TipoPersona { get; set; }
        /// <summary>
        /// Nombre de la Persona Moral
        /// </summary>
        public string Nombre { get; set; }
        /// <summary>
        /// Razon Social
        /// </summary>
        public string RazonSocial { get; set; }

        #endregion

        #region Datos Generales 
        /// <summary>
        /// RFC Persona Fisica o Moral
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string RFC { get; set; } //valido el tamaño 12 PM , 13 PF
        /// <summary>
        /// Email PFE
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string EmailPFE { get; set; }
        /// <summary>
        /// Contacto de Cobranza
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ContactoCobranza { get; set; }
        /// <summary>
        /// Telefono Móvil
        /// </summary>
        public string TelefonoMovil { get; set; }
        /// <summary>
        /// Telefono de Oficina
        /// </summary>
        public string TelefonoOficina { get; set; }
        /// <summary>
        /// Email de Facturacion
        /// </summary>
        public string EmailFacturacion { get; set; }
        /// <summary>
        /// Telefono de Cobranza (1)
        /// </summary>
        public string TelefonoCobranza1 { get; set; }
        /// Telefono de Cobranza (2)
        public string TelefonoCobranza2 { get; set; }
        /// <summary>
        /// Giro o Actividad
        /// </summary>
        public string GiroOActividad { get; set; }
        [JsonIgnore]
        public Guid ActividadId { get; set; }
        /// <summary>
        /// Domicilio de Facturación
        /// </summary>
        [Required]
        public Domicilio DomicilioFacturacion { get; set; }
        /// Domicilio Operativo
        public Domicilio DomicilioOperativo { get; set; }
        /// <summary>
        /// Solidarios Obligados
        /// </summary>
        public List<SolidarioObligado> SolidariosObligados { get; set; }
        #endregion

        /// <summary>
        /// Impuestos a aplicar
        /// </summary>
        public int Impuestos { get; set; }
        /// <summary>
        /// Forma de Pago
        /// </summary>
        public string FormaPago { get; set; }
        /// <summary>
        /// Uso de comprobante (CFDI)
        /// </summary>
        public string UsoComprobante { get; set; }
        /// <summary>
        /// Regimen Fiscal
        /// </summary>
        public string RegimenFiscal { get; set; }
        /// <summary>
        /// Razón Social CFDI 4.0
        /// </summary>
        public string RazonSocialV4 { get; set; }
        /// <summary>
        /// Código Postal CFDI 4.0
        /// </summary>
        public string CodigoPostalV4 { get; set; }
    }

    /// <summary>
    /// Solidario obligado (Persona fisica)
    /// </summary>
    public class SolidarioObligado : DatosPersonaFisica
    {

        /// <summary>
        /// Tipo de Persona
        /// </summary>
        [Required]
        public string TipoPersona { get; set; }
        /// <summary>
        /// RFC
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string RFC { get; set; }

        #region PERSONA MORAL
        /// <summary>
        /// Nombre de la Persona Moral
        /// </summary>
        public string Nombre { get; set; }
        /// <summary>
        /// Razon Social
        /// </summary>
        public string RazonSocial { get; set; }

        #endregion

        #region Datos Generales 
        /// <summary>
        /// Email PFE
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string EmailPFE { get; set; }
        /// <summary>
        /// Contacto de Cobranza
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ContactoCobranza { get; set; }
        /// <summary>
        /// Telefono Móvil
        /// </summary>
        public string TelefonoMovil { get; set; }
        /// <summary>
        /// Telefono de Oficina
        /// </summary>
        public string TelefonoOficina { get; set; }
        /// <summary>
        /// Email de Facturacion
        /// </summary>
        public string EmailFacturacion { get; set; }
        /// <summary>
        /// Telefono de Cobranza (1)
        /// </summary>
        public string TelefonoCobranza1 { get; set; }
        /// Telefono de Cobranza (2)
        public string TelefonoCobranza2 { get; set; }
        /// <summary>
        /// Giro o Actividad
        /// </summary>
        public string GiroOActividad { get; set; }
        [JsonIgnore]
        public Guid ActividadId { get; set; }
        /// <summary>
        /// Domicilio de Facturación
        /// </summary>
        [Required]
        public Domicilio DomicilioFacturacion { get; set; }
        /// <summary>
        /// Domicilio Operativo
        /// </summary>
        public Domicilio DomicilioOperativo { get; set; }
        #endregion

        /// <summary>
        /// Impuestos a aplicar
        /// </summary>
        public int Impuestos { get; set; }
        /// <summary>
        /// Forma de Pago
        /// </summary>
        public string FormaPago { get; set; }
        /// <summary>
        /// Uso de comprobante (CFDI)
        /// </summary>
        public string UsoComprobante { get; set; }
        /// <summary>
        /// Regimen Fiscal
        /// </summary>
        public string RegimenFiscal { get; set; }
        /// <summary>
        /// Razón Social CFDI 4.0
        /// </summary>
        public string RazonSocialV4 { get; set; }
        /// <summary>
        /// Código Postal CFDI 4.0
        /// </summary>
        public string CodigoPostalV4 { get; set; }
    }

    /// <summary>
    /// Domicilio
    /// </summary>
    public class Domicilio
    {
        /// <summary>
        /// Calle
        /// </summary>
        //[Required(AllowEmptyStrings = false)]
        public string Calle { get; set; }
        /// <summary>
        /// Numero Interior
        /// </summary>
        public string NumeroInterior { get; set; }
        /// <summary>
        /// Numero Exterior
        /// </summary>
        //[Required(AllowEmptyStrings = false)]
        public string NumeroExterior { get; set; }
        /// <summary>
        /// Codigo Postal
        /// </summary>
        //[Required(AllowEmptyStrings = false)]
        public string CodigoPostal { get; set; }
        /// <summary>
        /// Colonia
        /// </summary>
        //[Required(AllowEmptyStrings = false)]
        public string Colonia { get; set; }
        /// <summary>
        /// Guid de la Colonia
        /// </summary>
        [JsonIgnore]
        public Guid ColoniaId { get; set; }
        /// <summary>
        /// Municipio
        /// </summary>
        /*[Required(AllowEmptyStrings = false)]
        public string Municipio { get; set; }
        /// <summary>
        /// Estado
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Estado { get; set; }
        /// <summary>
        /// Pais
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Pais { get; set; }*/
        [JsonIgnore]
        public DomicilioCodigoPostal DomicilioCodigoPostal { get; set; }
        
    }

    /// <summary>
    /// Datos de persona fisica
    /// </summary>
    public class DatosPersonaFisica
    {
        #region PERSONA FISICA
        /// <summary>
        /// Genero
        /// </summary>
        public int Genero { get; set; }
        /// <summary>
        /// Nombre(s)
        /// </summary>
        public string Nombres { get; set; }
        /// <summary>
        /// Apellido Paterno
        /// </summary>
        public string ApellidoPaterno { get; set; }
        /// <summary>
        /// Apellido Materno
        /// </summary>
        public string ApellidoMaterno { get; set; }
        /// <summary>
        /// Fecha de Nacimiento (Formato = UNIX)
        /// </summary>
        public string FechaNacimiento { get; set; }
        /// <summary>
        /// Lugar de Nacimiento
        /// </summary>
        public string LugarNacimiento { get; set; }
        [JsonIgnore]
        public Guid PaisNacimientoId { get; set; }
        [JsonIgnore]
        public Guid EstadoNacimientoId { get; set; }
        /// <summary>
        /// Nacionalidad
        /// </summary>
        public int? Nacionalidad { get; set; }
        /// <summary>
        /// Profesion
        /// </summary>
        public int? Profesion { get; set; }
        /// <summary>
        /// Estado Civil
        /// </summary>
        public int? EstadoCivil { get; set; }
        /// <summary>
        /// Documento de Identificacion
        /// </summary>
        public int? DocumentoIdentificacion { get; set; }
        /// <summary>
        /// Numero de Identificacion
        /// </summary>
        public string NumeroIdentificacion { get; set; }
        /// <summary>
        /// Nombre largo
        /// </summary>
        public string NombreLargo { get; set; }

        #endregion
    }

    /// <summary>
    /// Detalle de Financiacion
    /// </summary>
    public class DetalleFinanciacion
    {
        /// <summary>
        /// Importe
        /// </summary>
        [Required]
        public decimal Importe { get; set; }
        /// <summary>
        /// Deposito de Garantia
        /// </summary>
        [Required]
        public decimal DepositoGarantia { get; set; }
        /// <summary>
        /// Porcentaje de Deposito de Garantia
        /// </summary>
        public decimal PorcentajeDepositoGarantia { get; set; }
        /// <summary>
        /// Anticipo por comision
        /// </summary>
        [Required]
        public decimal AnticipoComision { get; set; }
        /// <summary>
        /// Porcentaje por anticipo por comision
        /// </summary>
        public decimal PorcentajeAnticipoComision { get; set; }
        /// <summary>
        /// Anticipo de Renta extraordinaria
        /// </summary>
        [Required]
        public decimal AnticipoRentaExtraordinaria { get; set; }
        /// <summary>
        /// Porcentaje Renta Extraordinaria
        /// </summary>
        public decimal PorcentajeRentaExtraordinaria { get; set; }
        /// <summary>
        /// Valor Residual
        /// </summary>
        public decimal ValorResidual { get; set; }
        /// <summary>
        /// Porcentaje de Valor Residual
        /// </summary>
        public decimal PorcentajeValorResidual { get; set; }
        /// <summary>
        /// Renta sin Iva
        /// </summary>
        public decimal RentaSinIva { get; set; }
        /// <summary>
        /// Renta con Iva
        /// </summary>
        [Required]
        public decimal RentaConIva { get; set; }
        /// <summary>
        /// Plazo
        /// </summary>
        [Required]
        public int Plazo { get; set; }
        /// <summary>
        /// Fecha de Inicio
        /// </summary>
        public string FechaInicio { get; set; }
    }

    public class ValorLista
    {
        public string Nombre { get; set; }
        public int? Valor { get; set; }
    }

    public class SolicitudResponse
    {
        public string FolioIMX { get; set; }
        public string Peticion { get; set; }
        public string Respuesta { get; set; }
        public string FolioSolicitud { get; set; }
        public bool Success { get; set; }
    }

    public class Persona
    {
        public Guid Id { get; set; }
        public bool Existe { get; set; }
        public bool Solicitante { get; set; }
        public bool Aval { get; set; }
        public bool PersonaFisica { get; set; }
        public bool PersonaMoral { get; set; }
        public List<Guid> TelefonosContacto { get; set; }
        public Guid RelacionAval { get; set; } = Guid.Empty;
        public Persona()
        {
            TelefonosContacto = new List<Guid>();
        }
    }

    public class DomicilioCodigoPostal
    {
        public Guid PaisId { get; set; }
        public string Pais { get; set; }
        public Guid EstadoId { get; set; }
        public string Estado { get; set; }
        public Guid MunicipioId { get; set; }
        public string Municipio { get; set; }
        public Guid CiudadId { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public List<DomicilioColonia> Colonias { get; set; }

    }

    public class DomicilioColonia
    {
        public Guid ColoniaId { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Solicitud para asignar documento a una solicitud de crédito
    /// </summary>
    public class DocumentoSolicitudRequest
    {
        /// <summary>
        /// Folio iMX de la solicitud de crédito
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Folio { get; set; }

        /// <summary>
        /// Objeto de documentos a integrar a la solicitud
        /// </summary>
        [Required]
        public DocumentoSolicitud Documento { get; set; }

        /// <summary>
        /// Indica si es el ultimo documento enviado
        /// </summary>
        public bool UltimoDocumento { get; set; }
    }

    /// <summary>
    /// Objeto de documentos a integrar a la solicitud
    /// </summary>
    public class DocumentoSolicitud
    {
        /// <summary>
        /// Nombre del archivo
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Nombre { get; set; }

        /// <summary>
        /// Archivo en base64
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Archivo { get; set; }
    }

    /// <summary>
    /// Respuesta a la petición de asignar documento a solicitud de crédito
    /// </summary>
    public class DocumentoSolicitudResponse
    {
        /// <summary>
        /// Indica si fue o no exitosa la carga y asignación del documento a la solicitud
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Url de SharePoint donde fue cargado el documento
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// En caso de un error el detalle se agrega a esta variable
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Petición para actualizar solicitud de iMX
    /// </summary>
    public class SolicitudActualizacion
    {
        /// <summary>
        /// Folio iMX
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Folio { get; set; } //folio imx

        /// <summary>
        /// Solidarios Obligados
        /// </summary>
        public List<SolidarioObligado> SolidariosObligados { get; set; }

        /// <summary>
        /// Detalle de la financiacion
        /// </summary>
        [Required]
        public DetalleFinanciacion DetalleFinanciacion { get; set; }
    }
}
