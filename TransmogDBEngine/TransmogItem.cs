using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowDotNetAPI.Models;

namespace TransmogDBEngine
{
    class TransmogItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        // public string Source { get; set; }

        public TransmogItem(Item _item)
        {
            ID = _item.Id;
            Name = _item.Name;

           // ItemSourceInfo mySource = _item.ItemSource;
           // Source = mySource..ToString();

        }

    }

}
