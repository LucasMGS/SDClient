using System;
using System.Collections.Generic;
using System.Text;

namespace CompGrafica
{
    [Serializable]
    public class DtoGrafo
    {
        public int Operador { get; set; }
        public int NoPartida { get; set; }
        public int NoDestino { get; set; }
    }
}
