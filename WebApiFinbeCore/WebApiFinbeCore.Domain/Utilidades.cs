using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
namespace WebApiFinbeCore.Domain
{
    public static class Utilidades
    {
        public static object AtributoColleccion(this Entity entity,string field,TipoAtributos tipo = TipoAtributos.STRING) 
        {
            object result = null;
            bool alias = field.Contains(".");
            if (entity.Contains(field))
            {
                object aliasValue = null;
                if (alias)
                {
                    aliasValue = ((AliasedValue)entity.Attributes[field]).Value;
                }
                else
                {
                    aliasValue = entity.Attributes[field];
                }
                switch (tipo)
                {
                    case TipoAtributos.OPCION:
                        result = ((OptionSetValue)aliasValue).Value;
                        break;
                    case TipoAtributos.OPCION_TEXTO:
                        result = entity.FormattedValues[field];
                        break;
                    case TipoAtributos.MONEY:
                        result = ((Money)aliasValue).Value;
                        break;
                    case TipoAtributos.ENTITY_REFERENCE_ID:
                        result = ((EntityReference)aliasValue).Id;
                        break;
                    case TipoAtributos.ENTITY_REFERENCE_NAME:
                        result = ((EntityReference)aliasValue).Name;
                        break;
                    case TipoAtributos.STRING:
                        result = aliasValue;
                        break;
                    case TipoAtributos.FECHA:
                        try
                        {
                            DateTime valueFecha = (DateTime)aliasValue;
                            if(valueFecha != null)
                            {
                                result = valueFecha.ToString("dd/MM/yyyy");
                            }
                            else
                            {
                                result = null;
                            }
                        }
                        catch(Exception)
                        {
                            result = aliasValue;
                        }
                        break;
                }

            }
            return result;
        }

        public static String ToStringNull(this object objeto)
        {
            if (objeto != null)
            {
                return objeto.ToString();
            }
            return null;
        }

        public static Decimal? ToDecimal(this object objeto)
        {
            if (objeto != null)
            {
                return Decimal.Parse(objeto.ToString());
            }
            return null;
        }

        public static Decimal? ToDecimalZero(this object objeto)
        {
            if (objeto != null)
            {
                return Decimal.Parse(objeto.ToString());
            }
            return Decimal.Zero;
        }

        public static Double ToDouble(this object objeto)
        {
            if (objeto != null)
            {
                return Double.Parse(objeto.ToString());
            }
            return 0;
        }

        public static Decimal ToDecimal2(this object objeto,decimal valueDefault = 0)
        {
            if (objeto != null)
            {
                return Decimal.Parse(objeto.ToString());
            }
            return valueDefault;
        }

        public static Int32? ToInt32(this object objeto)
        {
            if (objeto != null)
            {
                return Int32.Parse(objeto.ToString());
            }
            return null;
        }

        public static Boolean ToBoolean(this object objeto)
        {
            if (objeto != null)
            {
                return Boolean.Parse(objeto.ToString());
            }
            return false;
        }

        public static DateTime CalcularFechaPerGracia(DateTime fechadisposicion, int diaCorte)
        {
            DateTime fechapergracia;
            int diaInicio = fechadisposicion.Day;

            int meses = (diaInicio <= diaCorte) ? 1 : 2;
            fechapergracia = fechadisposicion.AddMonths(meses);
            int daysInMonth = DateTime.DaysInMonth(fechapergracia.Year, fechapergracia.Month);

            if (diaCorte > daysInMonth) diaCorte = daysInMonth;

            fechapergracia = new DateTime(fechapergracia.Year, fechapergracia.Month, diaCorte);

            return fechapergracia;
        }

    }

   

    public enum TipoAtributos
    {
        STRING,
        MONEY,
        OPCION,
        OPCION_TEXTO,
        INT,
        ENTITY_REFERENCE_ID,
        ENTITY_REFERENCE_NAME,
        FECHA
    } 

    
}
