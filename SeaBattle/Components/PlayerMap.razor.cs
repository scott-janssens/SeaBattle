using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SeaBattleEngine;
using System.Collections.Immutable;

namespace SeaBattle.Components;

public partial class PlayerMap
{
    [Parameter]
    public IMap Map { get; set; } = default!;

#pragma warning disable IDE1006 // Naming Styles
    [Parameter]
    public EventCallback<MouseEventArgs> onmouseup { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    private Ship carrier = default!;
    private Ship battleship = default!;
    private Ship cruiser = default!;
    private Ship submarine = default!;
    private Ship destroyer = default!;
    private Ship? setupShip = null;

    public void PlaceShip(SeaBattleEngine.Ship ship)
    {
        setupShip = ship switch
        {
            Carrier _ => carrier,
            Battleship _ => battleship,
            Cruiser => cruiser,
            Submarine => submarine,
            Destroyer => destroyer,
            _ => throw new NotImplementedException()
        };

        setupShip.IsSelected = true;
        setupShip.IsVisible = true;
    }

    public void LockShip()
    {
        if (setupShip != null)
        {
            setupShip.IsSelected = false;
        }
    }

    public void ShowAll(ShipInfo[]? ships = null)
    {
        if (ships != null)
        {
            foreach (var shipInfo in ships!)
            {
                var ship = Map.GetShipByName(shipInfo.Name);
                ship.SetLocation(shipInfo.Row, shipInfo.Column, shipInfo.Orientation, false);
                ship.SetHits(shipInfo.ZonesHit);
            }
        }

        carrier.IsVisible = true;
        battleship.IsVisible = true;
        cruiser.IsVisible = true;
        submarine.IsVisible = true;
        destroyer.IsVisible = true;
    }

    public void Reset()
    {
        setupShip = null;
        carrier.Reset();
        battleship.Reset();
        cruiser.Reset();
        submarine.Reset();
        destroyer.Reset();
    }

    private async Task MouseUpHandler(MouseEventArgs e)
    {
        await onmouseup.InvokeAsync(e);
    }
}
