namespace sanitizzazioneLPG;

public class PersEcc : Exception
{
    public PersEcc()
    {
    }

    public PersEcc(string message) : base("Eccezione persistenza: " + message)
    {
    }

    public PersEcc(string message, Exception inner) : base("Eccezione persistenza: " +  message, inner)
    {
    }
}
