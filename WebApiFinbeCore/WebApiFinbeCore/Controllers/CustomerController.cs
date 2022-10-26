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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CustomerController : BaseController
    {
        /// <summary>
        /// Obtiene los clientes de plan piso
        /// </summary>
        /// <param name="tipoDispLinea">Tipo de Disposicion Linea</param>
        /// <returns></returns>
        /// <response code="200">OK. Devuelve la lista de clientes configurados en CRM que pueden hacer uso del portal plan piso.</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción </response>
        [HttpGet]
        [Route("ClientesDispLinea")]
        [ResponseType(typeof(List<ClientesPlanPiso>))]
        public HttpResponseMessage ClientesPlanPiso([FromUri]string tipoDispLinea = "")
        {
            try
            {
                //ValidarApiKey();
                var clientesPiso = CustomerService.ObtenerClientesPlanPiso(tipoDispLinea);
                return Request.CreateResponse(HttpStatusCode.OK, clientesPiso);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los representantes legales
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la lista de los representantes legales</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>

        [HttpGet]
        [Route("RepresentantesLegales")]
        [ResponseType(typeof(List<RepresentanteLegal>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage RepresentantesLegales([FromUri]string numeroCliente)
        {
            try
            {
                var representantesLegales = CustomerService.ObtenerRepresentantesLegales(numeroCliente);
                return Request.CreateResponse(HttpStatusCode.OK, representantesLegales);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtener referencia bancaria del cliente
        /// </summary>
        /// <param name="idCliente">Id del Cliente</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve una referencia bancaria</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("ObtenerReferenciaBancaria")]
        [ResponseType(typeof(string))]
        public HttpResponseMessage ObtenerReferenciaBancaria([FromUri]string idCliente)
        {
            try
            {
                var result = CustomerService.obtenerReferenciaBancaria(idCliente);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Obtener Configuracion cliente Plan Piso
        /// </summary>
        /// <param name="numeroCliente">Numero de Cliente</param>
        /// <param name="tipoDispLinea">Tipo de Disposicion Linea</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la configuracion de clientes</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("ConfiguracionClientesDispLinea")]
        [ResponseType(typeof(List<Configuracion>))]
        public HttpResponseMessage ConfiguracionClientesPlanPiso([FromUri]string numeroCliente,[FromUri]string tipoDispLinea = "")
        {
            try
            {
                var result = CustomerService.getConfiguracion(numeroCliente,tipoDispLinea);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Validar Nip del Cliente
        /// </summary>
        /// <param name="numeroCliente">Numero de Cliente</param>
        /// <param name="clave">Clave del Cliente</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la validacion del login</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("Autenticar")]
        [ResponseType(typeof(Login))]
        public HttpResponseMessage Autenticar([FromUri]string numeroCliente, [FromUri]string clave)
        {
            try
            {
                var result = CustomerService.Autenticar(numeroCliente, clave);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Obtener las Bitacora de Disposicion del Cliente
        /// </summary>
        /// <param name="numeroCliente">Numero de cliente</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve el listado de bitacoras de disposicion</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("BitacorasDisposicion")]
        [ResponseType(typeof(List<BitacoraDisposicion>))]
        public HttpResponseMessage BitacorasDisposicion([FromUri]string numeroCliente)
        {
            try
            {
                var result = CustomerService.getBitacorasDisposicion(numeroCliente);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Crear Disposicion Con Garantia
        /// </summary>
        /// <param name="numeroCliente">Numero de Cliente</param>
        /// <param name="montoDispuesto">Monto Dispuesto</param>
        /// <param name="notificarUsuario">Notificar Usuario</param>
        /// <param name="plazo">Plazo</param>
        /// <param name="fechaDisposicion">Fecha de Disposicion</param>
        /// <param name="tipoCalculo">Tipo de Calculo</param>
        /// <param name="periodosGracia">Periodos Gracia</param>
        /// <param name="marca">Marca</param>
        /// <param name="modelo">Modelo</param>
        /// <param name="tipoAutomovil">Tipo Automovil</param>
        /// <param name="numeroSerie">Numero de Serie</param>
        /// <param name="color">Color</param>
        /// <param name="lineaDeCredito">Linea de Credito</param>
        /// <param name="noMotor">Numero de Motor</param>
        /// <param name="version">Version</param>
        /// <param name="descripcion">Descripción</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la datos de la disposicion con garantia creada</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpPost]
        [Route("CrearDisposicionConGarantia")]
        [ResponseType(typeof(DisposicionResponse))]
        public HttpResponseMessage CrearDisposicionConGarantia([FromUri] string numeroCliente, [FromUri] string lineaDeCredito,[FromUri] decimal montoDispuesto, [FromUri]bool notificarUsuario, [FromUri] int plazo,[FromUri] DateTime fechaDisposicion ,[FromUri]string tipoCalculo,[FromUri]int periodosGracia, [FromUri] string descripcion,[FromUri]string numeroSerie, [FromUri]string modelo, [FromUri]string marca, [FromUri]string tipoAutomovil = null, [FromUri]string color = null, [FromUri]string version = null, [FromUri]string noMotor = null)
        {
            try
            {
                var result = CustomerService.CrearBitacoraDisposicion(numeroCliente,lineaDeCredito, montoDispuesto, notificarUsuario, plazo, fechaDisposicion, tipoCalculo, periodosGracia,numeroSerie,modelo,marca,tipoAutomovil,color,version,noMotor,descripcion);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Información de los créditos del cliente
        /// </summary>
        /// <param name="numeroCliente">Numero de Cliente</param>
        /// <param name="credito">Credito</param>
        /// <param name="linea">Linea</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve el listado de creditos</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("DetalleCreditos")]
        [ResponseType(typeof(List<Credito>))]
        public HttpResponseMessage DetalleCreditos([FromUri]string numeroCliente,[FromUri] string linea = "",[FromUri]string credito = "")
        {
            try
            {
                var result = CustomerService.ObtenerDetalleCreditos(numeroCliente,linea,credito);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Avales
        /// </summary>
        /// <param name="lineaDeCredito">Linea de Credito</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la lista de representantes legales</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("Avales")]
        [ResponseType(typeof(List<Aval>))]
        public HttpResponseMessage Avales([FromUri]string lineaDeCredito)
        {
            try
            {
                var result = CustomerService.ObtenerAvalesPorLineaDeCredito(lineaDeCredito);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("Clientes")]
        [ResponseType(typeof(List<Cliente>))]
        public HttpResponseMessage Clientes([FromUri]string filtro = null)
        {
            try
            {
                var result = CustomerService.getClientes(filtro);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Obtener las cuentas del cliente dado un correo
        /// </summary>
        /// <param name="correo">Correo</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCuentas")]
        [ResponseType(typeof(List<CuentaCorreo>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage GetCuentaCorreo([FromUri] string correo)
        {
            try
            {
                var clientes = CustomerService.getClientesCorreo(correo);
                return Request.CreateResponse(HttpStatusCode.OK, clientes);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
