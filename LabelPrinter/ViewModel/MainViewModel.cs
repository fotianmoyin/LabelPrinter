using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using LabelPrinter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LabelPrinter.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // code runs in blend --> create design time data.
                ReportList.Add(new ReportVM { ReportName = "顺丰快递" });
                ReportList.Add(new ReportVM { ReportName = "中通快递" });
                ReportList.Add(new ReportVM { ReportName = "圆通快递" });
                ReportList.Add(new ReportVM { ReportName = "汇通快递" });
            }
            else
            {
                // code runs "for real"
                if (!Directory.Exists(Funs.StoreFolder))
                    Directory.CreateDirectory(Funs.StoreFolder);

                if (!File.Exists(Path.Combine(Funs.StoreFolder, "Reports.json")))
                {
                    Encoding gbk = Encoding.GetEncoding("gbk");
                    ZipStrings.CodePage = gbk.CodePage;
                    ZipFile zipFile = new ZipFile("Reports.zip");
                    for (int i = 0; i < zipFile.Count; i++)
                    {
                        var zipEntry = zipFile[i];
                        if (zipEntry.IsDirectory)
                        {
                            var folderPath = $"{Funs.StoreFolder}/{zipEntry.Name}";
                            if (!Directory.Exists(folderPath))
                                Directory.CreateDirectory(folderPath);
                            continue;
                        }

                        var filePath = $"{Funs.StoreFolder}/{zipEntry.Name}";
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            using (Stream input = zipFile.GetInputStream(zipEntry))
                            {
                                byte[] buffer = new byte[10 * 1024];
                                int length = 0;
                                while ((length = input.Read(buffer, 0, 10 * 1024)) > 0)
                                    fs.Write(buffer, 0, length);
                            }
                        }
                    }
                }
            }
        }

        private ObservableCollection<ReportVM> _reportList = new ObservableCollection<ReportVM>();

        public ObservableCollection<ReportVM> ReportList
        {
            get
            {
                return _reportList;
            }
            set
            {
                Set(ref _reportList, value);
            }
        }

        private ReportVM _selectedReport;
        public ReportVM SelectedReport
        {
            get
            {
                return _selectedReport;
            }
            set
            {
                Set(ref _selectedReport, value,true);
            }
        }

        #region 加载

        private RelayCommand _loadCmd;
        public RelayCommand LoadCmd
        {
            get
            {
                if (_loadCmd == null)
                    _loadCmd = new RelayCommand(Load);
                return _loadCmd;
            }
        }

        private void Load()
        {
            LoadReportList();
        }

        #endregion

        #region 报表添加

        private RelayCommand _reportAddCmd;
        public RelayCommand ReportAddCmd
        {
            get
            {
                if (_reportAddCmd == null)
                    _reportAddCmd = new RelayCommand(ReportAdd);
                return _reportAddCmd;
            }
        }

        private void ReportAdd()
        {
            ReportNew(new ReportVM
            {
                ReportPath = $"{Funs.ReportsFolder}/Default.frx",
                DataPath = $"{Funs.ReportsFolder}/Default.xlsx"
            });
        }

        #endregion

        #region 报表导入

        private RelayCommand _reportImportCmd;
        public RelayCommand ReportImportCmd
        {
            get
            {
                if (_reportImportCmd == null)
                    _reportImportCmd = new RelayCommand(ReportImport);
                return _reportImportCmd;
            }
        }

        private void ReportImport()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "标签模板|*.lbpt";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            Encoding gbk = Encoding.GetEncoding("gbk");
            ZipStrings.CodePage = gbk.CodePage;
            ZipFile zipFile = new ZipFile(openFileDialog.FileName);
            var json = zipFile.ZipFileComment;
            Report report = JsonConvert.DeserializeObject<Report>(json);
            if (ReportList.Count(r => r.Id == report.Id) > 0)
            {
                if (System.Windows.Forms.MessageBox.Show("已包含该模板，确定覆盖吗", "询问",
                    System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            for (int i = 0; i < zipFile.Count; i++)
            {
                var zipEntry = zipFile[i];
                if (zipEntry.IsDirectory)
                {
                    var folderPath = $"{Funs.ReportsFolder}/{zipEntry.Name}";
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    continue;
                }

                var filePath = $"{Funs.ReportsFolder}/{zipEntry.Name}";
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (Stream input = zipFile.GetInputStream(zipEntry))
                    {
                        byte[] buffer = new byte[10 * 1024];
                        int length = 0;
                        while ((length = input.Read(buffer, 0, 10 * 1024)) > 0)
                            fs.Write(buffer, 0, length);
                    }
                }
            }

            if (ReportList.Count(r => r.Id == report.Id) <= 0)
                ReportAdd(report.Id, report.ReportName);

            SelectedReport = ReportList.First(r => r.Id == report.Id);
            System.Windows.Forms.MessageBox.Show("导入完成", "提示");
        }

        #endregion

        private void ReportCopy(ReportVM rvm)
        {
            ReportNew(rvm);
        }

        private void ReportExport(ReportVM rvm)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.FileName = rvm.ReportName;
            saveFileDialog.Filter = "标签模板|*.lbpt";
            if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            Encoding gbk = Encoding.GetEncoding("gbk");
            ZipStrings.CodePage = gbk.CodePage;
            ZipFile zipFile = ZipFile.Create(saveFileDialog.FileName);
            zipFile.BeginUpdate();
            zipFile.Add(rvm.ReportPath, $"{rvm.Id}.frx");
            zipFile.Add(rvm.DataPath, $"{rvm.Id}.xlsx");
            zipFile.SetComment(JsonConvert.SerializeObject(new Report { Id = rvm.Id, ReportName = rvm.ReportName }));
            zipFile.CommitUpdate();
            zipFile.Close();

            System.Windows.Forms.MessageBox.Show("导出完成", "提示");
        }

        private void ReportRemove(ReportVM rvm)
        {
            if (System.Windows.Forms.MessageBox.Show("确定要删除该模板吗", "询问", 
                System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                return;

            if (!ReportList.Contains(rvm))
                return;

            ReportList.Remove(rvm);
            File.Delete(rvm.ReportPath);
            File.Delete(rvm.DataPath);
            SaveReportList();

            if (ReportList.Count > 0)
                SelectedReport = ReportList[0];
        }

        private void ReportNew(ReportVM sourceReport)
        {
            MessengerInstance.Send(
                new CallbackMessage<InputVM>(
                    new InputVM { Title = "请输入模板名称" },
                    (msg) => {
                        Console.WriteLine("您输入了：" + msg.Text);
                        var id = Guid.NewGuid().ToString();
                        File.Copy(sourceReport.ReportPath, $"{Funs.ReportsFolder}/{id}.frx");
                        File.Copy(sourceReport.DataPath, $"{Funs.ReportsFolder}/{id}.xlsx");
                        ReportAdd(id, msg.Text);
                    }),
                Notifications.InputShow);
        }

        private ReportVM ReportInit(string id, string name)
        {
            return new ReportVM
            {
                Id = id,
                ReportName = name,
                ReportPath = $"{Funs.ReportsFolder}/{id}.frx",
                DataPath = $"{Funs.ReportsFolder}/{id}.xlsx",
                CopyImp = ReportCopy,
                ExportImp = ReportExport,
                RemoveImp = ReportRemove
            };
        }

        private void ReportAdd(string id, string name)
        {
            var report = ReportInit(id, name);
            ReportList.Add(report);
            SaveReportList();
            SelectedReport = report;
        }

        #region 报表预览

        private RelayCommand _reportPreviewCmd;
        public RelayCommand ReportPreviewCmd
        {
            get
            {
                if (_reportPreviewCmd == null)
                    _reportPreviewCmd = new RelayCommand(ReportPreview);
                return _reportPreviewCmd;
            }
        }

        private void ReportPreview()
        {
            if (SelectedReport == null)
                return;

            MessengerInstance.Send<ReportVM>(SelectedReport, Notifications.ReportPreview);
        }

        #endregion

        #region 报表保存

        private RelayCommand _reportSaveCmd;
        public RelayCommand ReportSaveCmd
        {
            get
            {
                if (_reportSaveCmd == null)
                    _reportSaveCmd = new RelayCommand(ReportSave);
                return _reportSaveCmd;
            }
        }

        private void ReportSave()
        {
            MessengerInstance.Send<ReportVM>(SelectedReport, Notifications.ReportSave);
        }

        #endregion

        #region 报表另存

        private RelayCommand _reportSaveAsCmd;
        public RelayCommand ReportSaveAsCmd
        {
            get
            {
                if (_reportSaveAsCmd == null)
                    _reportSaveAsCmd = new RelayCommand(ReportSaveAs);
                return _reportSaveAsCmd;
            }
        }

        private void ReportSaveAs()
        {
            var sourceReport = SelectedReport;
            MessengerInstance.Send(
                new CallbackMessage<ReportVM>(
                    SelectedReport,
                    (report) => {
                        ReportAdd(report.Id, report.ReportName);
                    }),
                Notifications.ReportSaveAs);
        }

        #endregion

        private void LoadReportList()
        {
            var filePath = $"{Funs.StoreFolder}/Reports.json";
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var reports = JsonConvert.DeserializeObject<List<Report>>(json);
                foreach (var report in reports)
                {
                    ReportList.Add(ReportInit(report.Id, report.ReportName));
                }
                if (ReportList.Count > 0)
                    SelectedReport = ReportList[0];
            }
        }

        private void SaveReportList()
        {
            var filePath = $"{Funs.StoreFolder}/Reports.json";
            var reports = new List<Report>();
            foreach (var rvm in ReportList)
            {
                reports.Add(new Report
                {
                    Id = rvm.Id,
                    ReportName = rvm.ReportName
                });
            }
            var json = JsonConvert.SerializeObject(reports);
            File.WriteAllText(filePath, json);
        }
    }
}