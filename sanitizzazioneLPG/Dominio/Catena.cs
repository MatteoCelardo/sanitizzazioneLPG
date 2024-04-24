using System.Collections.Generic;

namespace sanitizzazioneLPG.Dominio;

public class Catena : IDom
{
    private List<IDom> _els;

    public Catena()
    {
        _els = new List<IDom>();
    }

    public Catena(List<IDom> els)
    {
        _els = new List<IDom>(els);
    }

    public List<IDom> Els {get => _els; set => _els = value; }
}
