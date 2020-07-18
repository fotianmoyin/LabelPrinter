using FastReport;
using FastReport.Design.StandardDesigner;
using FastReport.Preview;
using GalaSoft.MvvmLight.Messaging;
using SpreadsheetLight;
using LabelPrinter.Model;
using LabelPrinter.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using unvell.ReoGrid;

namespace LabelPrinter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        EnvironmentSettings _settings = new EnvironmentSettings();
        DesignerControl _designer;
        FastReport.Report _report = new FastReport.Report();
        ReportVM _selectedReport;

        public MainWindow()
        {
            InitializeComponent();
            gridEdit.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Funs.Window = this;
            Messenger.Default.Register<CallbackMessage<InputVM>>(this, Notifications.InputShow, ShowInput);
            Messenger.Default.Register<PropertyChangedMessage<ReportVM>>(this, ReportChanged);
            Messenger.Default.Register<ReportVM>(this, Notifications.ReportPreview, ReportPreview);
            Messenger.Default.Register<ReportVM>(this, Notifications.ReportSave, ReportSave);
            Messenger.Default.Register<CallbackMessage<ReportVM>>(this, Notifications.ReportSaveAs, ReportSaveAs);

            FastReport.Utils.Res.LocaleFolder = "Localization";
            var file = FastReport.Utils.Res.LocaleFolder + @"Chinese (Simplified).frl";
            FastReport.Utils.Res.LoadLocale(file);

            _settings.PreviewSettings.Buttons =
                PreviewButtons.Print |
                //PreviewButtons.Email |
                PreviewButtons.Find |
                PreviewButtons.Zoom |
                PreviewButtons.Outline |
                PreviewButtons.PageSetup |
                //PreviewButtons.Edit |
                PreviewButtons.Watermark |
                PreviewButtons.Navigator |
                PreviewButtons.Close;
            _settings.PreviewSettings.Icon = new System.Drawing.Icon("LabelPrinter.ico");
            _settings.DesignerSettings.DefaultFont = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            _settings.UIStyle = FastReport.Utils.UIStyle.Office2007Blue;

            _designer = new DesignerControl();
            //_designer.IsPreviewPageDesigner = false;
            //_designer.ShowMainMenu = false;
            _designer.MainMenu.miFile.Visible = false;
            _designer.MainMenu.miData.Visible = false;
            _designer.MainMenu.miHelp.Visible = false;
            _designer.MainMenu.miReport.Visible = false;
            _designer.MainMenu.miFileExit.Visible = false;
            _designer.MainMenu.miViewOptions.Visible = false;

            _designer.Restrictions.DontEditCode = true;
            _designer.Restrictions.DontDeletePage = true;
            _designer.Restrictions.DontInsertBand = true;
            //_designer.Restrictions.DontInsertObject = true;
            //_designer.Restrictions.DontChangeReportOptions = true;
            _designer.Restrictions.DontCreateData = true;
            _designer.Restrictions.DontEditData = true;
            //_designer.Restrictions.DontChangePageOptions = true;
            _designer.Restrictions.DontShowRecentFiles = true;
            _designer.Restrictions.DontPreviewReport = true;
            _designer.Restrictions.DontCreateReport = true;
            _designer.Restrictions.DontSaveReport = true;
            _designer.Restrictions.DontLoadReport = true;
            _designer.Restrictions.DontCreatePage = true;
            //_designer.UIStyle = FastReport.Utils.UIStyle.Office2007Black;
            _designer.RestoreConfig();
            _designer.Report = _report;
            _designer.RefreshLayout();
            wfhDesign.Child = _designer;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Messenger.Default.Unregister<CallbackMessage<InputVM>>(this, Notifications.InputShow, ShowInput);
            Messenger.Default.Unregister<PropertyChangedMessage<ReportVM>>(this, ReportChanged);
            Messenger.Default.Unregister<ReportVM>(this, Notifications.ReportPreview, ReportPreview);
            Messenger.Default.Unregister<ReportVM>(this, Notifications.ReportSave, ReportSave);
            Messenger.Default.Unregister<CallbackMessage<ReportVM>>(this, Notifications.ReportSaveAs, ReportSaveAs);
            _designer.SaveConfig();
        }

        private void ReportChanged(PropertyChangedMessage<ReportVM> message)
        {
            if (message.Sender is MainViewModel && message.PropertyName == "SelectedReport")
            {
                _selectedReport = message.NewValue;
                if (_selectedReport != null)
                {
                    LoadReport(_selectedReport);
                    gridEdit.Visibility = Visibility.Visible;
                }
                else
                {
                    _designer.Report = null;
                    _designer.InitReport();
                    gridEdit.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ReportPreview(ReportVM report)
        {
            if (_selectedReport == null)
                return;

            reoGrid.Save($"{Funs.StoreFolder}/temp.xlsx");
            var dataRows = LoadDataRow($"{Funs.StoreFolder}/temp.xlsx", reoGrid.CurrentWorksheet.Name);
            _report.RegisterData(dataRows, "DataRows");
            _designer.InitReport();
            _designer.cmdPreview.Invoke();
        }

        private void ReportSave(ReportVM report)
        {
            if (_selectedReport == null)
                return;

            if (_designer.Modified)
                _designer.cmdSave.Invoke();
            reoGrid.Save(report.DataPath);
            System.Windows.Forms.MessageBox.Show("保存完成", "提示");
            _designer.Modified = false;
        }

        private void ReportSaveAs(CallbackMessage<ReportVM> msg)
        {
            InputVM inputVM = new InputVM { Title = "请输入模板名称" };
            WinInput win = new WinInput();
            win.Owner = this;
            win.SetValue(DataContextProperty, inputVM);
            var result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var id = Guid.NewGuid().ToString();
                _designer.Report.Save($"{Funs.ReportsFolder}/{id}.frx");
                reoGrid.Save($"{Funs.ReportsFolder}/{id}.xlsx");
                msg.Callback(new ReportVM { Id = id, ReportName = inputVM.Text });
            }
        }

        private void ShowInput(CallbackMessage<InputVM> msg)
        {
            WinInput win = new WinInput();
            win.Owner = this;
            win.SetValue(DataContextProperty, msg.Message);
            var result = win.ShowDialog();
            
            if (result.HasValue && result.Value)
            {
                msg.Callback(msg.Message);
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReport == null)
                return;

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Excel 2007|*.xlsx|Excel 2003|*.xls";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            LoadData(openFileDialog.FileName);
        }

        private void LoadReport(ReportVM report)
        {
            _report.Load(report.ReportPath);
            LoadData(report.DataPath);
            _designer.Modified = false;
        }

        private void LoadData(string fileName)
        {
            reoGrid.Load(fileName);
            var dataRows = LoadDataRow(fileName, reoGrid.CurrentWorksheet.Name);
            _report.RegisterData(dataRows, "DataRows");
            _designer.InitReport();
        }

        private List<DataRow> LoadDataRow(string fileName, string sheetName)
        {
            List<DataRow> dataRows = new List<DataRow>();
            SLDocument sld = new SLDocument(fileName);
            sld.SelectWorksheet(sheetName);
            var slws = sld.GetWorksheetStatistics();
            for (int rowIndex = slws.StartRowIndex; rowIndex <= slws.EndRowIndex; rowIndex++)
            {
                DataRow dataRow = new DataRow();
                for (int columnIndex = slws.StartColumnIndex; columnIndex <= slws.EndColumnIndex; columnIndex++)
                {
                    if (columnIndex == 27)
                        break;

                    switch (columnIndex)
                    {
                        case 1:
                            dataRow.A = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 2:
                            dataRow.B = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 3:
                            dataRow.C = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 4:
                            dataRow.D = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 5:
                            dataRow.E = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 6:
                            dataRow.F = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 7:
                            dataRow.G = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 8:
                            dataRow.H = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 9:
                            dataRow.I = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 10:
                            dataRow.J = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 11:
                            dataRow.K = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 12:
                            dataRow.L = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 13:
                            dataRow.M = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 14:
                            dataRow.N = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 15:
                            dataRow.O = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 16:
                            dataRow.P = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 17:
                            dataRow.Q = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 18:
                            dataRow.R = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 19:
                            dataRow.S = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 20:
                            dataRow.T = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 21:
                            dataRow.U = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 22:
                            dataRow.V = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 23:
                            dataRow.W = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 24:
                            dataRow.X = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 25:
                            dataRow.Y = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                        case 26:
                            dataRow.Z = sld.GetCellValueAsString(rowIndex, columnIndex);
                            break;
                    }
                }
                dataRows.Add(dataRow);
            }

            return dataRows;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedReport == null)
                return;

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Excel 2007|*.xlsx";
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            reoGrid.Save(saveFileDialog.FileName);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_selectedReport == null)
                return;

            if (!_designer.Modified)
                return;

            var result = System.Windows.Forms.MessageBox.Show("是否保存对模板的修改", "询问", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == System.Windows.Forms.DialogResult.No)
                return;

            _designer.cmdSave.Invoke();
            reoGrid.Save(_selectedReport.DataPath);
        }

        private void btnPageSetup_Click(object sender, RoutedEventArgs e)
        {
            _designer.cmdPageSetup.Invoke();
        }

        private void btnPrinterSetup_Click(object sender, RoutedEventArgs e)
        {
            _designer.cmdPrinterSetup.Invoke();
        }
    }
}
