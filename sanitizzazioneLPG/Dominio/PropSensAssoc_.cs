namespace sanitizzazioneLPG.Dominio;

public class PropSensAssoc_
{
    private string[] _propAssoc;
    private bool _sanitizzareProp;

    public PropSensAssoc_(string[] propAssoc, bool sanitizzareProp)
    {
        _propAssoc = propAssoc;
        _sanitizzareProp = sanitizzareProp;
    }

    public string[] PropAssoc { get => _propAssoc; set => _propAssoc = value; }
    public bool SanitizzareProp { get => _sanitizzareProp; set => _sanitizzareProp = value; }  

}
