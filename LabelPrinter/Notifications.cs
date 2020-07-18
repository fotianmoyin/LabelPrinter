using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinter
{
    class Notifications
    {
        /// <summary>
        /// 输入框显示
        /// </summary>
        public readonly static Guid InputShow = Guid.NewGuid();
        /// <summary>
        /// 报表预览
        /// </summary>
        public readonly static Guid ReportPreview = Guid.NewGuid();
        /// <summary>
        /// 报表保存
        /// </summary>
        public readonly static Guid ReportSave = Guid.NewGuid();
        /// <summary>
        /// 报表另存为
        /// </summary>
        public readonly static Guid ReportSaveAs = Guid.NewGuid();
    }
}
