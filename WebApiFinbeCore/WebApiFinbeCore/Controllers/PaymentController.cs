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
    public class PaymentController : BaseController
    {
        /// <summary>
        /// Registrar Pago
        /// </summary>
        /// <param name="pago">Modelo de pago</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve true si se pudo registrar el pago y false si no se pudo registrar</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpPost]
        [Route("RegistroPago")]
        [ResponseType(typeof(bool))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage RegistroPago([FromBody] Pago pago)
        {
            try
            {
                var model = pago.ValidarModelo();
                if (model.isValid)
                {
                    var response = PaymentService.RegistroPago(pago);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,model.mensaje);
                }
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Actualizar Pago
        /// </summary>
        /// <param name="pago"></param>
        /// <returns></returns>
        /// <response code="200">Ok. True si se actualizo el pago y false si no se pudo actualizar</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpPut]
        [Route("ActualizarPago")]
        [ResponseType(typeof(bool))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage ActualizarPago([FromBody] ActualizaPago pago)
        {
            try
            {
                var model = pago.ValidarModelo();
                if (model.isValid)
                {
                    var valid = PaymentService.ExistePago(pago.Referencia,pago.NoCliente);
                    if (valid)
                    {
                        var response = PaymentService.ActualizarPago(pago);
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest,"La Referencia no existe");
                    }
                    
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, model.mensaje);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtener Pagos Pendientes del Cliente
        /// </summary>
        /// <param name="ClienteId">Numero del Cliente PF o PM</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve los pagos pendientes</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("PagosPendientes")]
        [ResponseType(typeof(List<PagoPendiente>))]
        public HttpResponseMessage PagosPendientes([FromUri]string ClienteId)
        {
            try
            {
                var result = PaymentService.ObtenerPagosPendientes(ClienteId);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


    }
}
