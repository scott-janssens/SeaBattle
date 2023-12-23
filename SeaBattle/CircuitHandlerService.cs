using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Collections.Concurrent;

namespace SeaBattle;

public class CircuitHandlerService : CircuitHandler
{
    private readonly static CircuitHandlerService _instance = new();
    private readonly ConcurrentQueue<ICircuitBroker> _circuitIdQueue = new();

    public ConcurrentDictionary<string, Circuit> Circuits { get; }
    public event EventHandler<CircuitEventArgs> CircuitOpened = default!;
    public event EventHandler<CircuitEventArgs> CircuitClosed = default!;
    public event EventHandler<CircuitEventArgs> ConnectionDown = default!;

    private CircuitHandlerService()
    {
        Circuits = new ConcurrentDictionary<string, Circuit>();
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_circuitIdQueue.TryDequeue(out var circuitId))
        {
            circuitId.CurrentCircuit = circuit;
        }

        Circuits[circuit.Id] = circuit;
        CircuitOpened?.Invoke(this, new() { CircuitId = circuit.Id });
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Circuits.TryRemove(circuit.Id, out _);
        CircuitClosed?.Invoke(this, new() { CircuitId = circuit.Id });
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        ConnectionDown?.Invoke(this, new() { CircuitId = circuit.Id });
        return base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        return base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public static CircuitHandlerService GetCircuitHandlerService(ICircuitBroker circuitId)
    {
        _instance._circuitIdQueue.Enqueue(circuitId);
        return _instance;
    }
}

public class CircuitEventArgs : EventArgs
{
    public required string CircuitId { get; init; }
}

public interface ICircuitBroker
{
    Circuit? CurrentCircuit { get; set; }
}

public class CircuitBroker : ICircuitBroker
{
    public Circuit? CurrentCircuit { get; set; }
}