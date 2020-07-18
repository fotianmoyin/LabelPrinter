using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinter.ViewModel
{
    public class InputVM : ViewModelBase
    {
        public InputVM()
        {
            if (IsInDesignMode)
            {
                Title = "请输入内容";
                Text = "内容样例";
            }
        }
        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                Set(ref _title, value);
            }
        }

        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                Set(ref _text, value);
            }
        }
    }
}
