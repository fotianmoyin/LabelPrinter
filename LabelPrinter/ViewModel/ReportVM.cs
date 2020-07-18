using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinter.ViewModel
{
    public class ReportVM : ViewModelBase
    {
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                Set(ref _id, value);
            }
        }

        private string _reportName;
        public string ReportName
        {
            get
            {
                return _reportName;
            }
            set
            {
                Set(ref _reportName, value);
            }
        }

        private string _reportPath;
        public string ReportPath
        {
            get
            {
                return _reportPath;
            }
            set
            {
                Set(ref _reportPath, value);
            }
        }

        private string _dataPath;

        public string DataPath
        {
            get
            {
                return _dataPath;
            }
            set
            {
                Set(ref _dataPath, value);
            }
        }

        public Action<ReportVM> CopyImp;
        public Action<ReportVM> RemoveImp;
        public Action<ReportVM> ExportImp;

        #region 复制

        private RelayCommand _copyCmd;
        public RelayCommand CopyCmd
        {
            get
            {
                if (_copyCmd == null)
                    _copyCmd = new RelayCommand(Copy);

                return _copyCmd;
            }
        }

        private void Copy()
        {
            CopyImp?.Invoke(this);
        }

        #endregion

        #region 删除

        private RelayCommand _removeCmd;
        public RelayCommand RemoveCmd
        {
            get
            {
                if (_removeCmd == null)
                    _removeCmd = new RelayCommand(Remove);
                return _removeCmd;
            }
        }

        private void Remove()
        {
            RemoveImp?.Invoke(this);
        }

        #endregion

        #region 导出

        private RelayCommand _exportCmd;
        public RelayCommand ExportCmd
        {
            get
            {
                if (_exportCmd == null)
                    _exportCmd = new RelayCommand(Export);
                return _exportCmd;
            }
        }

        private void Export()
        {
            ExportImp?.Invoke(this);
        }

        #endregion
    }
}
