using Microsoft.AspNetCore.Components;

namespace SeaBattle.Pages;

public partial class Index
{
    private ElementReference nameInput;

    [Parameter]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public string? GameId { get; set; }

    //protected override async Task OnInitializedAsync()
    //{
    //    //(circuitHandler as CircuitHandlerService).OnCircuitOpenedAsync
    //    base.OnInitialized();
    //}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (circuitBroker.CurrentCircuit?.Id != null)
        {
            await ProtectedLocalStorage.SetAsync("circuitId", circuitBroker.CurrentCircuit.Id);
        }

        if (firstRender)
        {
            await nameInput.FocusAsync();
        }
    }

    private void NameSet()
    {
        if (GameId != null)
        {
            Navigation.NavigateTo($"game/{Name}/{GameId}/join");
        }
    }

    private void SetUp2Player()
    {
        GameId = Guid.NewGuid().ToString();
        Navigation.NavigateTo($"game/{Name}/{GameId}");
    }
}
