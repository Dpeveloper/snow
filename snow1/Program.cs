using snow1;

class Program
{
    static async Task Main(string[] args)
    {
        var sim = new ClosedCycleSimulation();
        await sim.RunAsync();
    }
}
