using Microsoft.AspNetCore.Components;
using SeaBattle.Components;

namespace SeaBattle.Pages;

public partial class CpuGame
{
    private GameHelper _gameHelper = default!;
    private ElementReference gameContainer;
    private PlayerMap playerMapComponent = default!;
    private PlayerMap opponentMapComponent = default!;

    private bool isInitialized = false;

    private int intializers = 0;

    [Parameter]
    public string Name { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        _gameHelper = new(1, Name, Guid.NewGuid().ToString())
        {
            Navigation = Navigation,
            StateHasChangedAsync = async () => await InvokeAsync(StateHasChanged)
        };

        intializers++;

        await StartSetup();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _gameHelper.GameContainer = gameContainer;
            await gameContainer.FocusAsync();

            intializers++;

            await StartSetup();
        }
    }

    private async Task StartSetup()
    {
        if (intializers >= 2)
        {
            isInitialized = true;

            // needed to populate playerMapComponent field
            await InvokeAsync(StateHasChanged);

            _gameHelper.PlayerMapComponent = playerMapComponent;
            _gameHelper.OpponentMapComponent = opponentMapComponent;

            await _gameHelper.SetupNextShipAsync();
            await InvokeAsync(StateHasChanged);
        }
    }
}
