using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetSummonerNames.Model
{
    public class Aliado
    {
        public string Name { get; set; }
    }

    public class Aliados
    {
        public List<Aliado> Participants { get; set; }
    }
}
