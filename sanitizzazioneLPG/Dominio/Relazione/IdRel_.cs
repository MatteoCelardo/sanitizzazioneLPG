using System.Collections.Generic;

namespace sanitizzazioneLPG.Dominio;

public class IdRel_
{
    private string _etichetta; 
    private IDictionary<string, string> _propStr;
    private IDictionary<string, double> _propNum;

    public IdRel_ (string etichetta, IDictionary<string, string> propStr, IDictionary<string, double> propNum) 
    {
        _etichetta = etichetta;
        _propNum = propNum;
        _propStr = propStr;
    }

    public IdRel_ ()
    {
        _propStr = new Dictionary<string,string>();
        _propNum = new Dictionary<string,double>();
    }

    public string Etichetta { get => _etichetta; set => _etichetta = value; }
    public IDictionary<string, string> PropStr {get => _propStr; set => _propStr = value;}
    public IDictionary<string, double> PropNum {get => _propNum; set => _propNum = value;}
}
