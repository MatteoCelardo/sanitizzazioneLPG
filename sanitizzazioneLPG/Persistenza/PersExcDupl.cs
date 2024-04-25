using System;

namespace sanitizzazioneLPG.Persistenza;

public class PersExcDupl : PersExc
{
    public PersExcDupl()
    {
    }

    public PersExcDupl(string message) : base(message)
    {
    }

    public PersExcDupl(string message, Exception inner) : base(message, inner)
    {
    }
}
