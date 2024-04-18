namespace sanitizzazioneLPG;

public class Catena :IDom
{
    private string[] _cat;

    public Catena(string[] cat)
    {
        _cat = cat;
    }

    public string[] Cat {get => _cat; set => _cat = value; }
}
