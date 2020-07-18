using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelPrinter.Model
{
    public class CallbackMessage<TMessage>
    {
        public TMessage Message { set; get; }
        public Action<TMessage> Callback { get; set; }
        public CallbackMessage(TMessage message, Action<TMessage> callback)
        {
            Message = message;
            Callback = callback;
        }
    }
}
