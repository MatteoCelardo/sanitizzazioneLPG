namespace sanitizzazioneLPG;

public class IdNodo_
{
    private string[] _etichette;
    private IDictionary<string, string> _propStr;
    private IDictionary<string, double> _propNum;

    public IdNodo_(string[] etichette, IDictionary<string, string> propStr, IDictionary<string, double> propNum)
    {
        _etichette = etichette;
        _propStr = propStr;
        _propNum = propNum;
    }

    public string[] Etichette { get => _etichette; set => _etichette = value; }
    public IDictionary<string, string> PropStr {get => _propStr; set => _propStr = value;}
    public IDictionary<string, double> PropNum {get => _propNum; set => _propNum = value;}

}
