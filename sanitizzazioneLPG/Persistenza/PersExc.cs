using System;

namespace sanitizzazioneLPG.Persistenza;

/// <summary>
/// Classe usata per eccezioni generiche della persistenza
/// </summary>
public class PersExc : Exception
{
    public PersExc()
    {
    }

    public PersExc(string message) : base("Eccezione persistenza: " +  message)
    {
    }

    public PersExc(string message, Exception inner) : base("Eccezione persistenza: " +  message, inner)
    {
    }
}
