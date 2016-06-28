using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowDotNetAPI.Models;

namespace TransmogDBEngine
{
    public enum SlotType { BACK, CHEST, FEET, HEAD, HANDS, LEGS, MAINHAND, OFFHAND, RANGED, SHIRT, SHOULDER, WAIST, WRIST };

    class TransmogItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Transmogrified { get; set; }
        public SlotType Slot { get; set; }

        // public string Source { get; set; }

        public TransmogItem(Item _item, bool _transmogrified, SlotType _slot)
        {
            ID = _item.Id;
            Name = _item.Name;
            Transmogrified = _transmogrified;
            Slot = _slot;

           // ItemSourceInfo mySource = _item.ItemSource;
           // Source = mySource..ToString();

        }

    }

}
