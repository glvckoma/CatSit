using System.Windows.Threading;

namespace CatSit.Interop;

public sealed class HookThread
{
    private readonly ManualResetEventSlim _ready = new(false);
    private Dispatcher? _dispatcher;

    public Dispatcher Dispatcher => _dispatcher ?? throw new InvalidOperationException("Hook thread not started");

    public void Start()
    {
        var thread = new Thread(() =>
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _ready.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "CatSit.Hooks"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        _ready.Wait();
    }

    public void Stop() => _dispatcher?.InvokeShutdown();
}
