using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Environment;

namespace LabelPrinter
{
    class Funs
    {
        public static Window Window;

        private static string _storeFolder;
        public static string StoreFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_storeFolder))
                    _storeFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.MyDocuments), "LabelPrinter");
                return _storeFolder;
            }
        }

        private static string _reportsFolder;
        public static string ReportsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_reportsFolder))
                    _reportsFolder = Path.Combine(StoreFolder, "Reports");
                return _reportsFolder;
            }
        }
    }
}
