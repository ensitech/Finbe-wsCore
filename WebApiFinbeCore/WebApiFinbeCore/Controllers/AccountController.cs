using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using WebApiFinbeCore.Attributes;
using WebApiFinbeCore.Domain;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Controllers
{
    public class AccountController : BaseController
    {
        /// <summary>
        /// Obtener Lineas de credito del cliente
        /// </summary>
        /// <param name="numeroCliente">Numero de Cliente</param>
        /// <param name="tipoLinea">Tipo de Linea</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve un listado de lineas de credito</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("LineasCredito")]
        [ResponseType(typeof(List<LineaCredito>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage LineasCredito([FromUri] string numeroCliente, [FromUri] bool tipoLinea)
        {
            try
            {
                var lineasCredito = AccountService.ObtenerLineasDeCredito(numeroCliente, tipoLinea);
                return Request.CreateResponse(HttpStatusCode.OK, lineasCredito);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Obtener la información de la linea de crédito
        /// </summary>
        /// <param name="numeroDeLineaCredito">Número De Línea de Crédito</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la informacion de una linea de credito</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("InformacionLineaCredito")]
        [ResponseType(typeof(InformacionLineaCredito))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage InformacionLineaCredito([FromUri] string numeroDeLineaCredito)
        {
            try
            {
                var informacionLineaCredito = AccountService.ObtenerInformacionLineaDeCredito(numeroDeLineaCredito);
                return Request.CreateResponse(HttpStatusCode.OK, informacionLineaCredito);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Información de los Estados de cuenta de un crédito en un periodo determinado
        /// </summary>
        /// <param name="idCredito">Id del Credito</param>
        /// <param name="mesAnioInicial">Mes Anio Inicial</param>
        /// <param name="mesAnioFinal">Mes Anio Final</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve un listado de estados de cuenta</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("EstadosDeCuenta")]
        [ResponseType(typeof(EstadoCuentaCredito))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage EstadosDeCuenta([FromUri] string idCredito,[FromUri]string mesAnioInicial,[FromUri]string mesAnioFinal)
        {
            try
            {
                var creditosEstadoDeCuenta = AccountService.EstadosDeCuenta(idCredito, mesAnioInicial, mesAnioFinal);
                return Request.CreateResponse(HttpStatusCode.OK, creditosEstadoDeCuenta);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Información de las facturas y complementos de pago de un crédito en un periodo determinado
        /// </summary>
        /// <param name="idCredito">Id de Credito</param>
        /// <param name="fechaInicial">Fecha Inicial</param>
        /// <param name="fechaFinal">Fecha Final</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve un listado de documentos</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("Facturas")]
        [ResponseType(typeof(FacturaCredito))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Facturas([FromUri] string idCredito, [FromUri]string fechaInicial, [FromUri]string fechaFinal)
        {
            try
            {
                var creditosFacturas = AccountService.Facturas(idCredito, fechaInicial, fechaFinal);
                return Request.CreateResponse(HttpStatusCode.OK, creditosFacturas);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Proceso para calcular el nuevo capital y tabla de amortización para la reducción de plazo
        /// </summary>
        /// <param name="credito">Credito</param>
        /// <param name="monto">Monto</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve el nuevo capital</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("PagoCapitalReduccionPlazo")]
        [ResponseType(typeof(CuotaCapital))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage PagoCapitalReduccionPlazo([FromUri] string credito, [FromUri]decimal monto)
        {
            try
            {
                var capital = AccountService.GetCapitalPaymentTermReduccion(credito, monto);
                return Request.CreateResponse(HttpStatusCode.OK, capital);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Información de Saldos y pagos mínimos
        /// </summary>
        /// <param name="numeroCliente">Numero de Cliente</param>
        /// <param name="opcion">Opcion</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve los saldos</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("SaldosPagosMinimos")]
        [ResponseType(typeof(SaldosCredito))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage SaldosPagosMinimos([FromUri] string numeroCliente, [FromUri]int opcion)
        {
            try
            {
                var saldos = AccountService.ObtenerBalanceCreditoPagosMinimos(numeroCliente, opcion);
                return Request.CreateResponse(HttpStatusCode.OK, saldos);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Proceso para calcular el nuevo capital y tabla de amortización para el recalculo
        /// </summary>
        /// <param name="idCredito">Id Credito</param>
        /// <param name="importe">Importe</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve el pago capital recalculo</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("PagoCapitalRecalculo")]
        [ResponseType(typeof(PagoCapitalRecalculo))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage PagoCapitalRecalculo([FromUri] string idCredito, [FromUri]decimal importe)
        {
            try
            {
                var recalculo = AccountService.ObtenerPagoCapitalRecalculo(idCredito,importe);
                return Request.CreateResponse(HttpStatusCode.OK, recalculo);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Proceso para la liquidación del crédito
        /// </summary>
        /// <param name="idCredito">Credito Id</param>
        /// <param name="fecha">Fecha dd/mm/yyyy</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la liquidacion</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("Liquidacion")]
        [ResponseType(typeof(Liquidacion))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Liquidacion([FromUri] string idCredito, [FromUri]string fecha)
        {
            try
            {
                var liquidacion = AccountService.ProcesoLiquidacion(idCredito, fecha);
                return Request.CreateResponse(HttpStatusCode.OK, liquidacion);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get Config Vars
        /// </summary>
        /// <param name="credito"></param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la configuracion del pago</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("GetConfigVars")]
        [ResponseType(typeof(ConfigPago))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetConfigVars([FromUri] string credito)
        {
            try
            {
                var configPago = AccountService.GetConfigVars(credito);
                return Request.CreateResponse(HttpStatusCode.OK, configPago);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="creditos"></param>
        /// <returns></returns>
        /// /// <response code="200">Ok. Devuelve un listado de creditos</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("GetInfoFromCreditList")]
        [ResponseType(typeof(InformacionCreditos))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage getInfoFromCreditList([FromUri] string[] creditos)
        {
            try
            {
                var informacionCredito = AccountService.getInfoFromCreditList(creditos);
                return Request.CreateResponse(HttpStatusCode.OK, informacionCredito);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtener informacion del contrato de la solicitud de credito por linea de credito
        /// </summary>
        /// <param name="lineaCredito"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetInfoContrato")]
        [ResponseType(typeof(InformacionContrato))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetInfoContrato([FromUri] string lineaCredito)
        {
            try
            {
                   var informacionContrato = AccountService.ObtenerInformacionContrato(lineaCredito);
                return Request.CreateResponse(HttpStatusCode.OK, informacionContrato);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtener Pagos pendientes
        /// </summary>
        /// <param name="credito">Número de Crédito</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPagosPendientesCR")]
        [ResponseType(typeof(List<PagoPendienteCr>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetPagosPendientesCr([FromUri] string credito)
        {
            try
            {
                var informacionContrato = AccountService.GetPagosPendientes(credito);
                return Request.CreateResponse(HttpStatusCode.OK, informacionContrato);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
