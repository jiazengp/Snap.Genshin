using DGP.Snap.Framework.Extensions.System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DGP.Snap.Framework.Extensions.System.Windows.Controls
{
    public static class DataGridExtensions
    {
        /// <summary> 
        /// 获取DataGrid控件单元格 
        /// </summary> 
        /// <param name="dataGrid">DataGrid控件</param> 
        /// <param name="rowIndex">单元格所在的行号</param> 
        /// <param name="columnIndex">单元格所在的列号</param> 
        /// <returns>指定的单元格</returns> 
        public static DataGridCell GetCell(this DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            DataGridRow rowContainer = dataGrid.GetRow(rowIndex);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = rowContainer.FirstVisualChild<DataGridCellsPresenter>();
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[columnIndex]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
                return cell;
            }
            return null;
        }

        /// <summary> 
        /// 获取DataGrid的行 
        /// </summary> 
        /// <param name="dataGrid">DataGrid控件</param> 
        /// <param name="rowIndex">DataGrid行号</param> 
        /// <returns>指定的行号</returns> 
        public static DataGridRow GetRow(this DataGrid dataGrid, int rowIndex)
        {
            DataGridRow rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (rowContainer == null)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
                rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            }
            return rowContainer;
        }
    }
}
