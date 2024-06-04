namespace testPerf;

class Program
{
    static void Main(string[] args)
    {
        Generatore g = new Generatore();

        g.GeneraFileUnNodo();
        g.GeneraFileUnaRel();
    }
}
