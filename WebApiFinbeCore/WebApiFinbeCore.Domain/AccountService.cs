using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Domain
{
    public class AccountService
    {
        public static List<LineaCredito> ObtenerLineasDeCredito(string numeroCliente, bool tipoLinea)
        {
            List<LineaCredito> lineas = new List<LineaCredito>();
            var service = CRMService.createService();

            String fetchXmlPf = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_configclientesdisplinea\">" +
                                    "<attribute name=\"fib_cliente\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_tipo\" operator=\"eq\" value=\"{1}\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"contact\" from=\"contactid\" to=\"fib_clientepfid\" link-type=\"inner\" alias=\"account\">" +
                                        "<attribute name=\"fib_numpersonafisica\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"fib_numpersonafisica\" operator=\"eq\" value=\"{0}\" />" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"fib_lineadecredito\" from=\"fib_customerpfid\" to=\"contactid\" link-type=\"inner\" alias=\"linea\">" +
                                        "<attribute name=\"fib_name\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                           
                                        "</link-entity>" +
                                    "</link-entity>" +
                                    
                                "</entity>" +
                            "</fetch>";
            String fetchXmlPm = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_configclientesdisplinea\">" +
                                    "<attribute name=\"fib_cliente\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_tipo\" operator=\"eq\" value=\"{1}\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"account\" from=\"accountid\" to=\"fib_clientepmid\" link-type=\"inner\" alias=\"account\">" +
                                        "<attribute name=\"accountnumber\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"accountnumber\" operator=\"eq\" value=\"{0}\" />" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"fib_lineadecredito\" from=\"fib_customerpmid\" to=\"accountid\" link-type=\"inner\" alias=\"linea\">" +
                                        "<attribute name=\"fib_name\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                         
                                    "</link-entity>" +
                                    "</link-entity>" +
                                    
                                "</entity>" +
                            "</fetch>";
            string fetchXml;
            int int_tipoLinea = tipoLinea ? 1 : 0;
            if (numeroCliente.Trim().StartsWith(Constantes.PERSONA_MORAL))
            {
                fetchXml = string.Format(fetchXmlPm,numeroCliente,int_tipoLinea);
            }
            else
            {
                fetchXml = string.Format(fetchXmlPf, numeroCliente, int_tipoLinea);
            }
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Any())
            {
                foreach (var result in results.Entities)
                {
                    lineas.Add(new LineaCredito
                    {
                        numeroLinea = (string)((AliasedValue)result.Attributes["linea.fib_name"]).Value,
                        tipoLinea = null//((AliasedValue)result.Attributes["fib_configclientesdisplinea.fib_tipo"]).Value.ToString()
                    });
                }
            }
            return lineas;
        }

        public static InformacionLineaCredito ObtenerInformacionLineaDeCredito(string numeroDeLineaCredito)
        {
            InformacionLineaCredito informacion = new InformacionLineaCredito();
            var service = CRMService.createService();

            String fetchXml = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_lineadecredito\">" +
                                    "<attribute name=\"fib_lineadecreditoid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<attribute name=\"fib_tipodelineadecredito\" />" +
                                    "<attribute name=\"fib_estatus\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<attribute name=\"fib_fechavencimiento\" />" +
                                    "<attribute name=\"fib_importedisponible\" />" +
                                    "<attribute name=\"fib_saldodisponible\" />" +
                                    "<attribute name=\"fib_fechadisponibildad\" />" +
                                    
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_disponibleporproducto\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"inner\" alias=\"producto\">" +
                                            "<attribute name=\"fib_codigo\" />" +
                                            "<attribute name=\"fib_importedisponible\" />" +
                                            "<attribute name=\"fib_importedispuesto\" />" +
                                            "<attribute name=\"fib_importerevolvente\" />" +
                                            "<attribute name=\"fib_numerodedisposicionesremanentes\" />" +
                                            "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "</filter>" +
                                            "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            var results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXml,numeroDeLineaCredito)));
            if (results.Entities.Any())
            {
                var result = results.Entities[0];
                informacion = new InformacionLineaCredito
                {
                    numeroLinea = result.Attributes["fib_name"].ToString(),
                    tipoLinea = result.AtributoColleccion("fib_tipodelineadecredito").ToStringNull(),
                    estatus = result.AtributoColleccion("fib_estatus", TipoAtributos.OPCION).ToStringNull(),
                    fechaInicio = result.AtributoColleccion("fib_fechadisponibildad").ToStringNull(),
                    fechaRecalificacion = null,
                    fechaVencimiento = result.AtributoColleccion("fib_fechavencimiento").ToStringNull(),
                };
                foreach(var producto in results.Entities)
                {
                    informacion.productos.Add(new Producto
                    {
                        descripcion = producto.AtributoColleccion("producto.fib_codigo").ToStringNull(),
                        disposicionesRestantes = (int)producto.AtributoColleccion("producto.fib_numerodedisposicionesremanentes").ToInt32(),
                        importeDisponible = (decimal)producto.AtributoColleccion("producto.fib_importedisponible", TipoAtributos.MONEY).ToDecimal(),
                        importeDispuesto = (decimal)producto.AtributoColleccion("producto.fib_importedispuesto", TipoAtributos.MONEY).ToDecimalZero(),
                        importeRevolvente = (decimal)producto.AtributoColleccion("producto.fib_importerevolvente", TipoAtributos.MONEY).ToDecimalZero(),
                    });
                }
            }
            return informacion;
        }
        
        public static EstadoCuentaCredito EstadosDeCuenta(string idCredito,string mesAnioInicial,string mesAnioFinal)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { idCredito,mesAnioInicial,mesAnioFinal };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getAccountStatements", paramlist);
            //result = result.Replace("\"Disposición Única de la CONDUSEF aplicable a las Entidades Financieras\"", "\\\"Disposición Única de la CONDUSEF aplicable a las Entidades Financieras\\\"");
            conectorAx.Logoff();
            var estadoCuentaCreditos = Newtonsoft.Json.JsonConvert.DeserializeObject<EstadoCuentaCredito>(result);
            return estadoCuentaCreditos;
        }

        public static FacturaCredito Facturas(string idCredito, string fechaInicial, string fechaFinal)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { idCredito, fechaInicial, fechaFinal };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getInvoiceandComplement", paramlist);
            //result = "{\"credito\":\"CR0000024824\",\"documentos\":[{\"tipo\":\"factura\",\"folio\":\"FV000951763\",\"fecha\":\"07\\/01\\/2020\",\"descripcion\":\"BC INTERES EXIGIBLE CR0000024824 35\\/60\",\"importe\":\"506.77\",\"xml\":\"<?xml version=\\\"1.0\\\" encoding=\\\"UTF-8\\\"?><cfdi:Comprobante xmlns:cfdi=\\\"http:\\/\\/www.sat.gob.mx\\/cfd\\/3\\\" xsi:schemaLocation=\\\"http:\\/\\/www.sat.gob.mx\\/cfd\\/3 http:\\/\\/www.sat.gob.mx\\/sitio_internet\\/cfd\\/3\\/cfdv33.xsd\\\" Version=\\\"3.3\\\" Serie=\\\"FV\\\" Folio=\\\"951763\\\" Fecha=\\\"2020-01-07T20:44:46\\\" FormaPago=\\\"99\\\" NoCertificado=\\\"00001000000406862811\\\" Certificado=\\\"MIIGRTCCBC2gAwIBAgIUMDAwMDEwMDAwMDA0MDY4NjI4MTEwDQYJKoZIhvcNAQELBQAwggGyMTgwNgYDVQQDDC9BLkMuIGRlbCBTZXJ2aWNpbyBkZSBBZG1pbmlzdHJhY2nDs24gVHJpYnV0YXJpYTEvMC0GA1UECgwmU2VydmljaW8gZGUgQWRtaW5pc3RyYWNpw7NuIFRyaWJ1dGFyaWExODA2BgNVBAsML0FkbWluaXN0cmFjacOzbiBkZSBTZWd1cmlkYWQgZGUgbGEgSW5mb3JtYWNpw7NuMR8wHQYJKoZIhvcNAQkBFhBhY29kc0BzYXQuZ29iLm14MSYwJAYDVQQJDB1Bdi4gSGlkYWxnbyA3NywgQ29sLiBHdWVycmVybzEOMAwGA1UEEQwFMDYzMDAxCzAJBgNVBAYTAk1YMRkwFwYDVQQIDBBEaXN0cml0byBGZWRlcmFsMRQwEgYDVQQHDAtDdWF1aHTDqW1vYzEVMBMGA1UELRMMU0FUOTcwNzAxTk4zMV0wWwYJKoZIhvcNAQkCDE5SZXNwb25zYWJsZTogQWRtaW5pc3RyYWNpw7NuIENlbnRyYWwgZGUgU2VydmljaW9zIFRyaWJ1dGFyaW9zIGFsIENvbnRyaWJ1eWVudGUwHhcNMTcwNzExMTcwMTEyWhcNMjEwNzExMTcwMTEyWjCB5TEtMCsGA1UEAxMkRklOQU5DSUVSQSBCRVBFTlNBIFNBIERFIENWIFNPRk9NIEVSMS0wKwYDVQQpEyRGSU5BTkNJRVJBIEJFUEVOU0EgU0EgREUgQ1YgU09GT00gRVIxLTArBgNVBAoTJEZJTkFOQ0lFUkEgQkVQRU5TQSBTQSBERSBDViBTT0ZPTSBFUjElMCMGA1UELRMcRkJFOTMwMjAyUUZBIC8gVkFCSjY3MTAxOExLNzEeMBwGA1UEBRMVIC8gVkFCSjY3MTAxOEhERlpTTjAyMQ8wDQYDVQQLEwZ1bmlkYWQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCM+ttusfpJE2niSBXns3dfYPc5Xv8ix2+d+8CcJkSC0Jbtmoxm8yN0irV1eqdK+qdm8vyVNvM9FFj6frF9rlb2hMbhnm5Yq9DUOx4DOTx9hPjfbytS6ndOnjQyHVmFg5iqov+QLExZJPzwjcDHOfOwR0+Ua32hbMpMF7BGlN\\/EH5dhdSJxQG\\/ghkvwcAD7BOeb+PWjaKQnYRgWDrTUuyM7AhzPyfwopXwkJStl\\/otWcYW8fycoUaCzXkdDyccxYswYw6276ClIY9E1G8K6fI9uK1cCaook7F6HN2+hUoPk9TOt74oeQpcFYtRv5pc7qDwYgfR6gj9W5OCmVATQLGiRAgMBAAGjHTAbMAwGA1UdEwEB\\/wQCMAAwCwYDVR0PBAQDAgbAMA0GCSqGSIb3DQEBCwUAA4ICAQBPbIY\\/3fhCzhPTfhGqxrUhdXjxIGCuLwm29t5itd+f0HbJzSnZv1l0n1NpRog\\/XWJtCq2ARtxrQcQ4c+\\/JHNJjVlzdR0mNCcEoyhR3rb90kKYIuhILNAAL2Php2ch50hHU0eopVE31YC+2yOhEkxs6gbF8uoufCGA+KWAdzSx7LsBjJhssjtnNvKsnL\\/BSNZD86UayA+sTa04AfmNxzlneMQpwSnXp0hboSULkbIeGSVVJbMOoEAGBQPQkQKD4nVMMmHREtanzO3WxGBchIjOlqOUR9wsnlGo0qZwmBwci+dnO56QOF7FUM55XyLa7LTDx17L9D6dvycs4d9tRMcEURvTPXKk3tW\\/Dd83EZusaucCglxF92KX29nezKvs7Z6HXvQgStjbg1FimXkqkihBdm9Y18Vr9JRZxIs7yrLh5sOK0QWjeD2FtKt0oQZOAd5Z7gd08jtWw8qrukQcWTzQMThL0WVeAQXAeOZDfPXBtAInXgFPcdivBCxz7wo42+Ji4MeqzeqzCQojz2WikIzVKduSDgiam7S2DlxD5v8GybkhyJ0aZ8WuUvAFOkkon7uLsv05c9pWjSp\\/\\/p91L7cdKxpTj95CXHncyYgufqmpE7ILPROcI\\/T6Ec0JFE1i+e\\/kaipa8qSgV6B9AppnwQr7X36ooyPGd9ZpXzjrd+yOqDQ==\\\" SubTotal=\\\"475.19\\\" Moneda=\\\"MXN\\\" Total=\\\"506.77\\\" TipoDeComprobante=\\\"I\\\" MetodoPago=\\\"PPD\\\" LugarExpedicion=\\\"97100\\\" Sello=\\\"HYiIVO3wMp9t8e+nMyWnCR+s4uTNTlaHRaICezjDap114UqrZYjfCzYx1Cp1k8B81wqCfOdScqdpUt9lv88S1BFoO1VwcPL1fXrzxtPW2h1MB1rzmHlip9cvHoRbqLlCVHdIBdey4m+ZDgNqvLPjQg7dlIgj4rkL+nHS6zUjgvOyi7eBSpL\\/\\/YUUrCT\\/NmvznRHBgYhANlkGGb9GAYNFFc1d8kwsnkUIaI4uWNve26wqmjtjSFtCyON3dvfPdZU7KFu8eBw98Pw6JSwCoBzqeRC12opeejGZsKQj6og0i3G\\/X4NMTcQgVVicQHU8VT6C9LIVg6vga6tZZ4kwh7pGtg==\\\" xmlns:xsi=\\\"http:\\/\\/www.w3.org\\/2001\\/XMLSchema-instance\\\"><cfdi:Emisor Rfc=\\\"FBE930202QFA\\\" Nombre=\\\"Financiera Bepensa S.A. de C.V. SOFOM ER\\\" RegimenFiscal=\\\"601\\\" \\/><cfdi:Receptor Rfc=\\\"FACK8703207E6\\\" Nombre=\\\"FARFAN CASTILLO KELLY YESENIA\\\" UsoCFDI=\\\"G03\\\" \\/><cfdi:Conceptos><cfdi:Concepto ClaveProdServ=\\\"84101700\\\" Cantidad=\\\"1.00\\\" ClaveUnidad=\\\"E48\\\" Unidad=\\\"NO APLICA\\\" Descripcion=\\\"BC INTERES EXIGIBLE CR0000024824 35\\/60\\\" ValorUnitario=\\\"475.19\\\" Importe=\\\"475.19\\\"><cfdi:Impuestos><cfdi:Traslados><cfdi:Traslado Base=\\\"197.38\\\" Impuesto=\\\"002\\\" TipoFactor=\\\"Tasa\\\" TasaOCuota=\\\"0.160000\\\" Importe=\\\"31.58\\\" \\/><\\/cfdi:Traslados><\\/cfdi:Impuestos><\\/cfdi:Concepto><\\/cfdi:Conceptos><cfdi:Impuestos TotalImpuestosTrasladados=\\\"31.58\\\"><cfdi:Traslados><cfdi:Traslado Impuesto=\\\"002\\\" TipoFactor=\\\"Tasa\\\" TasaOCuota=\\\"0.160000\\\" Importe=\\\"31.58\\\" \\/><\\/cfdi:Traslados><\\/cfdi:Impuestos><cfdi:Complemento><tfd:TimbreFiscalDigital xmlns:tfd=\\\"http:\\/\\/www.sat.gob.mx\\/TimbreFiscalDigital\\\" xmlns:xsi=\\\"http:\\/\\/www.w3.org\\/2001\\/XMLSchema-instance\\\" xsi:schemaLocation=\\\"http:\\/\\/www.sat.gob.mx\\/TimbreFiscalDigital http:\\/\\/www.sat.gob.mx\\/sitio_internet\\/cfd\\/TimbreFiscalDigital\\/TimbreFiscalDigitalv11.xsd\\\" FechaTimbrado=\\\"2020-01-07T21:14:41\\\" RfcProvCertif=\\\"EDI101020E99\\\" UUID=\\\"7bcbb418-fa4a-4233-8c1f-4cf50d822884\\\" NoCertificadoSAT=\\\"00001000000405428713\\\" SelloCFD=\\\"HYiIVO3wMp9t8e+nMyWnCR+s4uTNTlaHRaICezjDap114UqrZYjfCzYx1Cp1k8B81wqCfOdScqdpUt9lv88S1BFoO1VwcPL1fXrzxtPW2h1MB1rzmHlip9cvHoRbqLlCVHdIBdey4m+ZDgNqvLPjQg7dlIgj4rkL+nHS6zUjgvOyi7eBSpL\\/\\/YUUrCT\\/NmvznRHBgYhANlkGGb9GAYNFFc1d8kwsnkUIaI4uWNve26wqmjtjSFtCyON3dvfPdZU7KFu8eBw98Pw6JSwCoBzqeRC12opeejGZsKQj6og0i3G\\/X4NMTcQgVVicQHU8VT6C9LIVg6vga6tZZ4kwh7pGtg==\\\" SelloSAT=\\\"cZr9jvQ2GmDgD4eHV8o6vv+iKm8tfYHd6wSmGvXLnVZoO6e2h0QAqiSIYQJvD3Nxs40ivp1Ev74K5OvMLrYPtz4h0mGD3a\\/ToNVzs8VdPY9fOeWbk1UD\\/BsK4\\/2pyOs9qs0GTCCI\\/MDTZ2BHyVF1VNt0TtETV0ucINGfi2c2NU3NuTpGHyTRIZs6gWu8iVhs+jAE3d9HNsZ0DYR497UJr1aoU6lQ3uLluZuewvlg2MPf4udlw+XV22XnGTnO\\/aXhrKtM7QkGd5QvQf2FAR79AcAarxxJHRB2lYUyu0eih3HT3LnLpFiIZpJo9mZ6mKLctG9AywGc\\/jI5HEet9pjyYQ==\\\" Version=\\\"1.1\\\"><\\/tfd:TimbreFiscalDigital><\\/cfdi:Complemento><\\/cfdi:Comprobante>\"},{\"tipo\":\"complemento\",\"folio\":\"CO01234728\",\"fecha\":\"08\\/01\\/2020\",\"descripcion\":\"Complemento de Pago\",\"importe\":\"-2662.36\",\"xml\":\"<?xml version=\\\"1.0\\\" encoding=\\\"UTF-8\\\"?><cfdi:Comprobante xmlns:cfdi=\\\"http:\\/\\/www.sat.gob.mx\\/cfd\\/3\\\" xsi:schemaLocation=\\\"http:\\/\\/www.sat.gob.mx\\/cfd\\/3 http:\\/\\/www.sat.gob.mx\\/sitio_internet\\/cfd\\/3\\/cfdv33.xsd http:\\/\\/www.sat.gob.mx\\/Pagos http:\\/\\/www.sat.gob.mx\\/sitio_internet\\/cfd\\/Pagos\\/Pagos10.xsd\\\" Version=\\\"3.3\\\" Serie=\\\"CP\\\" Folio=\\\"142234\\\" Fecha=\\\"2020-01-09T13:57:55\\\" LugarExpedicion=\\\"97100\\\" Moneda=\\\"XXX\\\" TipoDeComprobante=\\\"P\\\" SubTotal=\\\"0\\\" Total=\\\"0\\\" xmlns:pago10=\\\"http:\\/\\/www.sat.gob.mx\\/Pagos\\\" NoCertificado=\\\"00001000000406862811\\\" Certificado=\\\"MIIGRTCCBC2gAwIBAgIUMDAwMDEwMDAwMDA0MDY4NjI4MTEwDQYJKoZIhvcNAQELBQAwggGyMTgwNgYDVQQDDC9BLkMuIGRlbCBTZXJ2aWNpbyBkZSBBZG1pbmlzdHJhY2nDs24gVHJpYnV0YXJpYTEvMC0GA1UECgwmU2VydmljaW8gZGUgQWRtaW5pc3RyYWNpw7NuIFRyaWJ1dGFyaWExODA2BgNVBAsML0FkbWluaXN0cmFjacOzbiBkZSBTZWd1cmlkYWQgZGUgbGEgSW5mb3JtYWNpw7NuMR8wHQYJKoZIhvcNAQkBFhBhY29kc0BzYXQuZ29iLm14MSYwJAYDVQQJDB1Bdi4gSGlkYWxnbyA3NywgQ29sLiBHdWVycmVybzEOMAwGA1UEEQwFMDYzMDAxCzAJBgNVBAYTAk1YMRkwFwYDVQQIDBBEaXN0cml0byBGZWRlcmFsMRQwEgYDVQQHDAtDdWF1aHTDqW1vYzEVMBMGA1UELRMMU0FUOTcwNzAxTk4zMV0wWwYJKoZIhvcNAQkCDE5SZXNwb25zYWJsZTogQWRtaW5pc3RyYWNpw7NuIENlbnRyYWwgZGUgU2VydmljaW9zIFRyaWJ1dGFyaW9zIGFsIENvbnRyaWJ1eWVudGUwHhcNMTcwNzExMTcwMTEyWhcNMjEwNzExMTcwMTEyWjCB5TEtMCsGA1UEAxMkRklOQU5DSUVSQSBCRVBFTlNBIFNBIERFIENWIFNPRk9NIEVSMS0wKwYDVQQpEyRGSU5BTkNJRVJBIEJFUEVOU0EgU0EgREUgQ1YgU09GT00gRVIxLTArBgNVBAoTJEZJTkFOQ0lFUkEgQkVQRU5TQSBTQSBERSBDViBTT0ZPTSBFUjElMCMGA1UELRMcRkJFOTMwMjAyUUZBIC8gVkFCSjY3MTAxOExLNzEeMBwGA1UEBRMVIC8gVkFCSjY3MTAxOEhERlpTTjAyMQ8wDQYDVQQLEwZ1bmlkYWQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCM+ttusfpJE2niSBXns3dfYPc5Xv8ix2+d+8CcJkSC0Jbtmoxm8yN0irV1eqdK+qdm8vyVNvM9FFj6frF9rlb2hMbhnm5Yq9DUOx4DOTx9hPjfbytS6ndOnjQyHVmFg5iqov+QLExZJPzwjcDHOfOwR0+Ua32hbMpMF7BGlN\\/EH5dhdSJxQG\\/ghkvwcAD7BOeb+PWjaKQnYRgWDrTUuyM7AhzPyfwopXwkJStl\\/otWcYW8fycoUaCzXkdDyccxYswYw6276ClIY9E1G8K6fI9uK1cCaook7F6HN2+hUoPk9TOt74oeQpcFYtRv5pc7qDwYgfR6gj9W5OCmVATQLGiRAgMBAAGjHTAbMAwGA1UdEwEB\\/wQCMAAwCwYDVR0PBAQDAgbAMA0GCSqGSIb3DQEBCwUAA4ICAQBPbIY\\/3fhCzhPTfhGqxrUhdXjxIGCuLwm29t5itd+f0HbJzSnZv1l0n1NpRog\\/XWJtCq2ARtxrQcQ4c+\\/JHNJjVlzdR0mNCcEoyhR3rb90kKYIuhILNAAL2Php2ch50hHU0eopVE31YC+2yOhEkxs6gbF8uoufCGA+KWAdzSx7LsBjJhssjtnNvKsnL\\/BSNZD86UayA+sTa04AfmNxzlneMQpwSnXp0hboSULkbIeGSVVJbMOoEAGBQPQkQKD4nVMMmHREtanzO3WxGBchIjOlqOUR9wsnlGo0qZwmBwci+dnO56QOF7FUM55XyLa7LTDx17L9D6dvycs4d9tRMcEURvTPXKk3tW\\/Dd83EZusaucCglxF92KX29nezKvs7Z6HXvQgStjbg1FimXkqkihBdm9Y18Vr9JRZxIs7yrLh5sOK0QWjeD2FtKt0oQZOAd5Z7gd08jtWw8qrukQcWTzQMThL0WVeAQXAeOZDfPXBtAInXgFPcdivBCxz7wo42+Ji4MeqzeqzCQojz2WikIzVKduSDgiam7S2DlxD5v8GybkhyJ0aZ8WuUvAFOkkon7uLsv05c9pWjSp\\/\\/p91L7cdKxpTj95CXHncyYgufqmpE7ILPROcI\\/T6Ec0JFE1i+e\\/kaipa8qSgV6B9AppnwQr7X36ooyPGd9ZpXzjrd+yOqDQ==\\\" Sello=\\\"Gesh0boMfhrb0UFvaKD1HC6Rw200piL9LS3PqN4wkIshv59Wn5LdmNii9Kqb5naGs0J5WPHDUKi7WvaEfzzGs6Qqyu5cVgiy33+kGXykjlLjNGF5Bvf\\/W27EfQIMIO4OT+Ow5yI2vV5Rfa1EJsgZXIZ3zu6GKPOpXTjO8MZqwOo0ieivlj2mpAsawuvJJrVIoWiJn\\/xZiNHeffE3a2pnAVQznjzdfbViKec4dJ8Af6L6JPSoki+6S+xCOyzYUy2DF+2vIYND70IxkRPQCwgJ8jg\\/iR9Q22PrurV7lYGpaitO6wEhjVGFZVhHkphDqsxzrv454n8d5UDNawREP5o2FA==\\\" xmlns:xsi=\\\"http:\\/\\/www.w3.org\\/2001\\/XMLSchema-instance\\\"><cfdi:Emisor Rfc=\\\"FBE930202QFA\\\" Nombre=\\\"Financiera Bepensa S.A. de C.V. SOFOM ER\\\" RegimenFiscal=\\\"601\\\" \\/><cfdi:Receptor Rfc=\\\"FACK8703207E6\\\" Nombre=\\\"FARFAN CASTILLO KELLY YESENIA\\\" UsoCFDI=\\\"P01\\\" \\/><cfdi:Conceptos><cfdi:Concepto ClaveProdServ=\\\"84111506\\\" ClaveUnidad=\\\"ACT\\\" Descripcion=\\\"Pago\\\" ValorUnitario=\\\"0\\\" Importe=\\\"0\\\" Cantidad=\\\"1\\\" \\/><\\/cfdi:Conceptos><cfdi:Complemento><pago10:Pagos xmlns:pago10=\\\"http:\\/\\/www.sat.gob.mx\\/Pagos\\\" Version=\\\"1.0\\\"><pago10:Pago FechaPago=\\\"2020-01-08T12:00:00\\\" FormaDePagoP=\\\"03\\\" MonedaP=\\\"MXN\\\" NumOperacion=\\\"5062370000296245\\\" Monto=\\\"506.77\\\"><pago10:DoctoRelacionado IdDocumento=\\\"7bcbb418-fa4a-4233-8c1f-4cf50d822884\\\" Serie=\\\"FV\\\" Folio=\\\"951763\\\" MonedaDR=\\\"MXN\\\" MetodoDePagoDR=\\\"PPD\\\" NumParcialidad=\\\"1\\\" ImpSaldoAnt=\\\"506.77\\\" ImpPagado=\\\"506.77\\\" ImpSaldoInsoluto=\\\"0.00\\\" \\/><\\/pago10:Pago><\\/pago10:Pagos><tfd:TimbreFiscalDigital xmlns:tfd=\\\"http:\\/\\/www.sat.gob.mx\\/TimbreFiscalDigital\\\" xmlns:xsi=\\\"http:\\/\\/www.w3.org\\/2001\\/XMLSchema-instance\\\" xsi:schemaLocation=\\\"http:\\/\\/www.sat.gob.mx\\/TimbreFiscalDigital http:\\/\\/www.sat.gob.mx\\/sitio_internet\\/cfd\\/TimbreFiscalDigital\\/TimbreFiscalDigitalv11.xsd\\\" FechaTimbrado=\\\"2020-01-09T14:07:20\\\" RfcProvCertif=\\\"EDI101020E99\\\" UUID=\\\"b8a92dff-1a0a-4a46-94f5-bbc8ec7ed88c\\\" NoCertificadoSAT=\\\"00001000000405428713\\\" SelloCFD=\\\"Gesh0boMfhrb0UFvaKD1HC6Rw200piL9LS3PqN4wkIshv59Wn5LdmNii9Kqb5naGs0J5WPHDUKi7WvaEfzzGs6Qqyu5cVgiy33+kGXykjlLjNGF5Bvf\\/W27EfQIMIO4OT+Ow5yI2vV5Rfa1EJsgZXIZ3zu6GKPOpXTjO8MZqwOo0ieivlj2mpAsawuvJJrVIoWiJn\\/xZiNHeffE3a2pnAVQznjzdfbViKec4dJ8Af6L6JPSoki+6S+xCOyzYUy2DF+2vIYND70IxkRPQCwgJ8jg\\/iR9Q22PrurV7lYGpaitO6wEhjVGFZVhHkphDqsxzrv454n8d5UDNawREP5o2FA==\\\" SelloSAT=\\\"Y+2BJFwjdfqJ6tIpqykHLRktaT3y6YO5aRzdGlCmK0bgRCJuwuBHOKnJ3rhpXtGe826mBo3N6v+4ch3cBJb3AAM0H1zrTGiWaoAMUnK\\/dozUUXO00CAvgBSaVaXOv61Assl6894kYyFFfYhQwC1dRA0ut7AGz0vfPlK93EFqYtkFcffh8USO0GXnHHv5VRnWFxaz8jIZAOoddJj8R\\/zdUJkEKB\\/qd0l4eArDoYA6SnZFYau6E5afVMoLQVc304\\/GVGnuif+n8tDGqtGC5\\/nV4Uex+zKa4OzI78S4dRXEZ3OhaE6+LGeBmn9s40AT9VzEwr2pGqJkYCoQ5S+7JBBoVQ==\\\" Version=\\\"1.1\\\"><\\/tfd:TimbreFiscalDigital><\\/cfdi:Complemento><\\/cfdi:Comprobante>\"}]}";
            conectorAx.Logoff();
            var facturasCreditos = Newtonsoft.Json.JsonConvert.DeserializeObject<FacturaCredito>(result);
            return facturasCreditos;
        }

        public static CuotaCapital GetCapitalPaymentTermReduccion(string credito,decimal monto)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { credito, monto };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getCapitalPaymentTermReduc", paramlist);
            conectorAx.Logoff();
            //result = "{\"numerocuotas\":29,\"mensualidad\":5946.64,\"saldo\":120174.30,\"cuotas\":[{\"cuota\":12,\"fecha\":\"12\\/10\\/2020\",\"capital\":3417.16,\"interes\":2085.59,\"pago\":5946.64,\"saldofinal\":116757.14},{\"cuota\":13,\"fecha\":\"11\\/11\\/2020\",\"capital\":3467.40,\"interes\":2042.28,\"pago\":5946.64,\"saldofinal\":113289.74},{\"cuota\":14,\"fecha\":\"11\\/12\\/2020\",\"capital\":3518.34,\"interes\":1998.36,\"pago\":5946.64,\"saldofinal\":109771.40},{\"cuota\":15,\"fecha\":\"11\\/01\\/2021\",\"capital\":3576.54,\"interes\":1948.19,\"pago\":5946.64,\"saldofinal\":106194.86},{\"cuota\":16,\"fecha\":\"11\\/02\\/2021\",\"capital\":3629.11,\"interes\":1902.87,\"pago\":5946.64,\"saldofinal\":102565.75},{\"cuota\":17,\"fecha\":\"11\\/03\\/2021\",\"capital\":3682.39,\"interes\":1856.94,\"pago\":5946.64,\"saldofinal\":98883.36},{\"cuota\":18,\"fecha\":\"11\\/04\\/2021\",\"capital\":3736.38,\"interes\":1810.40,\"pago\":5946.64,\"saldofinal\":95146.98},{\"cuota\":19,\"fecha\":\"11\\/05\\/2021\",\"capital\":3791.42,\"interes\":1762.95,\"pago\":5946.64,\"saldofinal\":91355.56},{\"cuota\":20,\"fecha\":\"11\\/06\\/2021\",\"capital\":3847.17,\"interes\":1714.89,\"pago\":5946.64,\"saldofinal\":87508.39},{\"cuota\":21,\"fecha\":\"11\\/07\\/2021\",\"capital\":3903.61,\"interes\":1666.23,\"pago\":5946.64,\"saldofinal\":83604.78},{\"cuota\":22,\"fecha\":\"11\\/08\\/2021\",\"capital\":3960.78,\"interes\":1616.95,\"pago\":5946.64,\"saldofinal\":79644.00},{\"cuota\":23,\"fecha\":\"11\\/09\\/2021\",\"capital\":4019.00,\"interes\":1566.76,\"pago\":5946.64,\"saldofinal\":75625.00},{\"cuota\":24,\"fecha\":\"11\\/10\\/2021\",\"capital\":4078.27,\"interes\":1515.66,\"pago\":5946.64,\"saldofinal\":71546.73},{\"cuota\":25,\"fecha\":\"11\\/11\\/2021\",\"capital\":4138.25,\"interes\":1463.96,\"pago\":5946.64,\"saldofinal\":67408.48},{\"cuota\":26,\"fecha\":\"11\\/12\\/2021\",\"capital\":4198.94,\"interes\":1411.64,\"pago\":5946.64,\"saldofinal\":63209.54},{\"cuota\":27,\"fecha\":\"11\\/01\\/2022\",\"capital\":4260.68,\"interes\":1358.41,\"pago\":5946.64,\"saldofinal\":58948.86},{\"cuota\":28,\"fecha\":\"11\\/02\\/2022\",\"capital\":4323.14,\"interes\":1304.57,\"pago\":5946.64,\"saldofinal\":54625.72},{\"cuota\":29,\"fecha\":\"11\\/03\\/2022\",\"capital\":4386.65,\"interes\":1249.82,\"pago\":5946.64,\"saldofinal\":50239.07},{\"cuota\":30,\"fecha\":\"11\\/04\\/2022\",\"capital\":4451.21,\"interes\":1194.16,\"pago\":5946.64,\"saldofinal\":45787.86},{\"cuota\":31,\"fecha\":\"11\\/05\\/2022\",\"capital\":4516.49,\"interes\":1137.89,\"pago\":5946.64,\"saldofinal\":41271.37},{\"cuota\":32,\"fecha\":\"11\\/06\\/2022\",\"capital\":4582.82,\"interes\":1080.71,\"pago\":5946.64,\"saldofinal\":36688.55},{\"cuota\":33,\"fecha\":\"11\\/07\\/2022\",\"capital\":4650.21,\"interes\":1022.61,\"pago\":5946.64,\"saldofinal\":32038.34},{\"cuota\":34,\"fecha\":\"11\\/08\\/2022\",\"capital\":4718.66,\"interes\":963.60,\"pago\":5946.64,\"saldofinal\":27319.68},{\"cuota\":35,\"fecha\":\"11\\/09\\/2022\",\"capital\":4787.82,\"interes\":903.98,\"pago\":5946.64,\"saldofinal\":22531.86},{\"cuota\":36,\"fecha\":\"11\\/10\\/2022\",\"capital\":4858.03,\"interes\":843.46,\"pago\":5946.64,\"saldofinal\":17673.83},{\"cuota\":37,\"fecha\":\"11\\/11\\/2022\",\"capital\":4929.66,\"interes\":781.71,\"pago\":5946.64,\"saldofinal\":12744.17},{\"cuota\":38,\"fecha\":\"11\\/12\\/2022\",\"capital\":5001.99,\"interes\":719.35,\"pago\":5946.64,\"saldofinal\":7742.18},{\"cuota\":39,\"fecha\":\"11\\/01\\/2023\",\"capital\":5075.38,\"interes\":656.09,\"pago\":5946.64,\"saldofinal\":2666.80},{\"cuota\":40,\"fecha\":\"11\\/02\\/2023\",\"capital\":2666.80,\"interes\":306.33,\"pago\":3079.21,\"saldofinal\":0.00}]}";
            var cuotaCapital = Newtonsoft.Json.JsonConvert.DeserializeObject<CuotaCapital>(result);
            return cuotaCapital;
        }

        public static SaldosCredito ObtenerBalanceCreditoPagosMinimos(string numeroCliente,int opcion)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { numeroCliente, opcion };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getCreditBalanceMinPaym", paramlist);
            conectorAx.Logoff();
            var saldos = Newtonsoft.Json.JsonConvert.DeserializeObject<SaldosCredito>(result);
            return saldos;
        }

        public static PagoCapitalRecalculo ObtenerPagoCapitalRecalculo(string idCredito, decimal importe)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { idCredito, importe };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getCapitalPaymentRecalc", paramlist);
            conectorAx.Logoff();
            var recalculo = Newtonsoft.Json.JsonConvert.DeserializeObject<PagoCapitalRecalculo>(result);
            return recalculo;
        }

        public static Liquidacion ProcesoLiquidacion(string idCredito, string fecha)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { idCredito, fecha };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getCreditPendingBalance", paramlist);
            conectorAx.Logoff();
            var iquidacion = Newtonsoft.Json.JsonConvert.DeserializeObject<Liquidacion>(result);
            return iquidacion;
        }

        public static ConfigPago GetConfigVars(string credito)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { credito };
            string result = (string)ax.CallStaticClassMethod("FibInfoPagos", "getConfigPagosMC", paramlist);
            conectorAx.Logoff();
            string[] array2 = result.Split(new char[]{'|'});
            ConfigPago configPago = new ConfigPago();
            if (result.Trim() != "" && !result.Contains("Error"))
            {
                configPago.semilla = ((array2[0] != null) ? array2[0] : "");
                configPago.xmlm = ((array2[1] != null) ? array2[1] : "");
                configPago.urlwebpay = ((array2[2] != null) ? array2[2] : "");
                configPago.urlresponse = ((array2[3] != null) ? array2[3] : "");
                configPago.idcompany = ((array2[4] != null) ? array2[4] : "");
            }
            else if (result.Contains("Error"))
            {
                conectorAx.Logoff();
                throw new Exception(result);
            }
            return configPago;            
        }

        public static InformacionCreditos getInfoFromCreditList(string[] listaCreditos)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            string format = "{\"creditos\":\"" + string.Join(",",listaCreditos) + "\"}";
            object[] paramlist = { format };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getInfoFromCreditList", paramlist);
            conectorAx.Logoff();
            var informacionCreditos = Newtonsoft.Json.JsonConvert.DeserializeObject<InformacionCreditos>(result);
            return informacionCreditos;
        }

        public static InformacionContrato ObtenerInformacionContrato(string numeroDeLineaCredito)
        {
            InformacionContrato informacion = new InformacionContrato();
            var service = CRMService.createService();

            String fetchXml = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_lineadecredito\">" +
                                    "<attribute name=\"fib_lineadecreditoid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<attribute name=\"fib_tipodelineadecredito\" />" +
                                    "<attribute name=\"fib_estatus\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<attribute name=\"fib_fechavencimiento\" />" +
                                    "<attribute name=\"fib_importedisponible\" />" +
                                    "<attribute name=\"fib_saldodisponible\" />" +
                                    "<attribute name=\"fib_fechadisponibildad\" />" +

                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"opportunity\" from=\"opportunityid\" to=\"fib_solicituddecreditoid\" link-type=\"inner\" alias=\"opportunity\">" +
                                            "<attribute name=\"fib_fechafirmadecontrato\" />" +
                                            "<attribute name=\"fib_ciudadfirma\" />" +
                                            "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                                "</condition>" +
                                            "</filter>" +
                                            "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            var results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXml, numeroDeLineaCredito)));
            if (results.Entities.Any())
            {
                var result = results.Entities[0];
                informacion = new InformacionContrato
                {
                    FechaFirmaContrato = result.AtributoColleccion("opportunity.fib_fechafirmadecontrato",TipoAtributos.FECHA).ToStringNull(),
                    CiudadFirma = result.AtributoColleccion("opportunity.fib_ciudadfirma").ToStringNull()
                };
            }
            return informacion;
        }

        public static List<PagoPendienteCr> GetPagosPendientes(string credito)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { credito };
            string result = (string)ax.CallStaticClassMethod("ANDFibPaymentsBC", "andPendingPaymentsFIB",paramlist);
            string json = result.Replace("<data>", "").Replace("</data>", "");
            
            conectorAx.Logoff();
            var listapendientes = Newtonsoft.Json.JsonConvert.DeserializeObject<PagosPendientesList>(json);
            return listapendientes.pagosPendientes;
        }
    }
}
