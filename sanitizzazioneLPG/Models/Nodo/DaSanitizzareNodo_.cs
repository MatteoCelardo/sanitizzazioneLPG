using System.Collections.Generic;

namespace sanitizzazioneLPG.Models;

public class DaSanitizzareNodo_
{
    private string[] _etichetteSens;
    private string[] _propSempreSens;
    private IDictionary<string, PropSensAssoc_> _propSensAssoc;

    public DaSanitizzareNodo_(string[] etichetteSens, string[] propSempreSens, IDictionary<string, PropSensAssoc_> propSensAssoc)
    {
        _etichetteSens = etichetteSens;
        _propSempreSens = propSempreSens;
        _propSensAssoc = propSensAssoc;
    }


    public string[] EtichetteSens { get => _etichetteSens; set => _etichetteSens = value; }
    public string[] PropSempreSens { get => _propSempreSens; set => _propSempreSens = value;}
    public IDictionary<string, PropSensAssoc_> PropSensAssoc { get => _propSensAssoc; set => _propSensAssoc = value; }

}
