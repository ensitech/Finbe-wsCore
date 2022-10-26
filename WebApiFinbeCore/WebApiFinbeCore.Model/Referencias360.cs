using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Model
{
    public class Referencias360
    {
        public List<CentroCosto> centroCostos { get; set; }
        public List<RefDepto> refDeptos { get; set; }
        public List<ReferenciaContable> referenciaContables { get; set; }
        public List<RefSuc> refSucs { get; set; }
        public ConfiguracionCompania configuracionCompania { get; set; }
    }
}
