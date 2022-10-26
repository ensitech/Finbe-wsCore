using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace WebApiFinbeCore.Model
{
    public class Pago
    {
        /// <summary>
        /// Referencia del pago
        /// </summary>
        public string Referencia { get; set; }
        /// <summary>
        /// Estatus del pago
        /// </summary>
        public string Estatus { get; set; }
        /// <summary>
        /// Fecha del pago yyyy-MM-ddTHH:mm:ss
        /// </summary>
        public DateTime Fecha { get; set; }
        /// <summary>
        /// Numero de Autorizacion
        /// </summary>
        public string NoAuth { get; set; }
        /// <summary>
        /// Monto Global
        /// </summary>
        public decimal MontoGlobal { get; set; }
        /// <summary>
        /// Numero de Cliente
        /// </summary>
        public string NoCliente { get; set; }
        /// <summary>
        /// Lista de Detalles
        /// </summary>
        public List<DetallePago> Detalle { get; set; }

        /// <summary>
        /// Validar Modelo
        /// </summary>
        /// <returns></returns>
        public Validacion ValidarModelo()
        {
            bool _isValid = true;
            string _mensaje = "";
            if(Referencia == null || Referencia.Length > 30)
            {
                _isValid = false;
                _mensaje = "Referencia requerida o Formato Incorrecto";
            }else if(Fecha == null)
            {
                _isValid = false;
                _mensaje = "Fecha Requerida";
            }else if(MontoGlobal < 0)
            {
                _isValid = false;
                _mensaje = "Monto Global invalido";
            }else if(NoCliente == null || NoCliente.Length > 8)
            {
                _isValid = false;
                _mensaje = "Numero de Cliente Requerido";
            }else if(Detalle != null)
            {
                foreach(var det in Detalle)
                {
                    var objDet = det.ValidarModelo();
                    if (!objDet.isValid)
                    {
                        _isValid = objDet.isValid;
                        _mensaje = objDet.mensaje;
                        break;
                    }
                }
            }
            return new Validacion { isValid = _isValid, mensaje= _mensaje};
        }

    }

    public class DetallePago
    {
        /// <summary>
        /// Referencia del Pago
        /// </summary>
        public string Referencia { get; set; }
        /// <summary>
        /// Credito
        /// </summary>
        public string Credito { get; set; }
        /// <summary>
        /// Monto del detalle
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Validar Modelo
        /// </summary>
        /// <returns></returns>
        public Validacion ValidarModelo()
        {
            bool _isValid = true;
            string _mensaje = "";
            if(Referencia == null || Referencia.Length > 30)
            {
                _isValid = false;
                _mensaje = "Referencia requerida en el detalle";
            }else if(Credito == null || Credito.Length > 12)
            {
                _isValid = false;
                _mensaje = "Credito requerido";
            }else if(Monto < 0)
            {
                _isValid = false;
                _mensaje = "Monto Requerido";
            }
            return new Validacion { isValid = _isValid , mensaje = _mensaje};
        }
    }

    public class ActualizaPago
    {
        /// <summary>
        /// Referencia del Pago
        /// </summary>
        public string Referencia { get; set; }
        /// <summary>
        /// Estatus del pago
        /// </summary>
        public string Estatus { get; set; }
        /// <summary>
        /// Numero de autorizacion
        /// </summary>
        public string NoAuth { get; set; }
        /// <summary>
        /// Numero de Cliente
        /// </summary>
        public string NoCliente { get; set; }

        public Validacion ValidarModelo()
        {
            bool _isValid = true;
            string _mensaje = "";
            if(Referencia == null || Referencia.Trim().Length == 0)
            {
                _isValid = false;
                _mensaje = "Referencia requerida";
            }
            else if (Estatus == null || Estatus.Trim().Length == 0)
            {
                _isValid = false;
                _mensaje = "Estatus Requerido";
            }
            else if (NoAuth == null || NoAuth.Trim().Length == 0)
            {
                _isValid = false;
                _mensaje = "Numero de Autorizacion Requerido";
            }
            else if (NoCliente == null || NoCliente.Trim().Length == 0)
            {
                _isValid = false;
                _mensaje = "Numero de Cliente Requerido";
            }
            return new Validacion { isValid = _isValid, mensaje = _mensaje };
        }

    }

    public class PagoCapitalRecalculo
    {
        /// <summary>
        /// Credito
        /// </summary>
        [JsonProperty("crédito")]
        public string Credito { get; set; }
        /// <summary>
        /// Tipo De Calculo
        /// </summary>
        [JsonProperty("tipocalculo")]
        public string TipoCalculo { get; set; }
        /// <summary>
        /// Algoritmo
        /// </summary>
        [JsonProperty("algoritmo")]
        public string Algoritmo { get; set; }
        /// <summary>
        /// Impuesto
        /// </summary>
        [JsonProperty("impuesto")]
        public string Impuesto { get; set; }
        /// <summary>
        /// Tasa Fija
        /// </summary>
        [JsonProperty("tasafija")]
        public decimal TasaFija { get; set; }
        /// <summary>
        /// Nombre Tasa
        /// </summary>
        [JsonProperty("nombretasa")]
        public string NombreTasa { get; set; }
        /// <summary>
        /// Tasa Variable
        /// </summary>
        [JsonProperty("tasavariable")]
        public decimal TasaVariable { get; set; }
        /// <summary>
        /// Operador TV
        /// </summary>
        [JsonProperty("operadorTV")]
        public string OperadorTv { get; set; }
        /// <summary>
        /// Puntos TV
        /// </summary>
        [JsonProperty("puntosTV")]
        public decimal PuntosTv { get; set; }
        /// <summary>
        /// Moneda
        /// </summary>
        [JsonProperty("moneda")]
        public string Moneda { get; set; }
        /// <summary>
        /// Cuotas
        /// </summary>
        [JsonProperty("cuotas")]
        public int Cuotas { get; set; }
        /// <summary>
        /// Saldo
        /// </summary>
        [JsonProperty("saldo")]
        public decimal Saldo { get; set; }
        /// <summary>
        /// FEcha Inicial
        /// </summary>
        [JsonProperty("fechainicial")]
        public string FechaInicial { get; set; }
        /// <summary>
        /// FEcha Inicial
        /// </summary>
        [JsonProperty("gtoadmon")]
        public string GtoAdmon { get; set; }
        /// <summary>
        /// FEcha Inicial
        /// </summary>
        [JsonProperty("tasavalorizada")]
        public string TasaValorizada { get; set; }
        /// <summary>
        /// FEcha Inicial
        /// </summary>
        [JsonProperty("impuestoval")]
        public string ImpuestoVal { get; set; }
        

    }

    public class Liquidacion
    {
        /// <summary>
        /// Credito
        /// </summary>
        [JsonProperty("credito")]
        public string Credito { get; set; }
        /// <summary>
        /// Saldo
        /// </summary>
        [JsonProperty("saldo")]
        public decimal Saldo { get; set; }
    }

    public class PagoPendiente
    {
        /// <summary>
        /// Fecha del Pago
        /// </summary>
        public string fecha { get; set; }
        /// <summary>
        /// Referencia del Pago
        /// </summary>
        public string referencia { get; set; }
        /// <summary>
        /// Credito
        /// </summary>
        public string credito { get; set; }
        /// <summary>
        /// Importe
        /// </summary>
        public decimal? importe { get; set; }
        /// <summary>
        /// Estatus
        /// </summary>
        public string estatus { get; set; }
    }

}
