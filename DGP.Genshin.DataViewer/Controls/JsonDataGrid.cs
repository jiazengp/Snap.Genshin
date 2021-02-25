using DGP.Snap.Framework.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DGP.Genshin.DataViewer.Controls
{
    public class JsonDataGrid : DataGrid
    {
        static JsonDataGrid() => DefaultStyleKeyProperty.OverrideMetadata(typeof(JsonDataGrid), new FrameworkPropertyMetadata(typeof(JsonDataGrid)));

    }
}
