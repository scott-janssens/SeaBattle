using Microsoft.AspNetCore.Components;
using SeaBattleEngine;

namespace SeaBattle.Components
{
    public partial class Ship
    {
        private int RectWidth => 10 + 50 * (ShipModel.Size - 1);
        private int Xoffset => 50 * (ShipModel.Column + 1) + 2;
        private int Yoffset => 50 * (ShipModel.Row + 1) + 2;
        private int Rotation => ShipModel.Orientation == Orientation.Horizontal ? 0 : 90;
        private string FillColor => ShipModel.IsValid && ShipModel.IsSeaworthy ? "lightgray" : "red";
        private string StrokeColor => IsSelected ? "darkviolet" : "black";

        [Parameter]
        public SeaBattleEngine.Ship ShipModel { get; set; } = default!;

        private bool _isVisible = false;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                InvokeAsync(StateHasChanged);
            }
        }

        public bool IsSelected { get; set; }

        public void Reset()
        {
            IsSelected = false;
            IsVisible = false;
        }
    }
}
