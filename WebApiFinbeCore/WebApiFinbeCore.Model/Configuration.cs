using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    public class ConfiguracionConexion
    {
        [JsonProperty("servidor")]
        public string servidor { get; set; }
        [JsonProperty("bd")]
        public string bd { get; set; }
        [JsonProperty("usuario")]
        public string usuario { get; set; }
        [JsonProperty("contraseña")]
        public string contraseña { get; set; }
    }

    public class Validacion
    {
        public bool isValid { get; set; }
        public string mensaje { get; set; }
    }
}
