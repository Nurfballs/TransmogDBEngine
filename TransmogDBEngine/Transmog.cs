using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowDotNetAPI;
using WowDotNetAPI.Models;

namespace TransmogDBEngine
{
    class Transmog
    {
        public string Realm { get; set; }
        public string Name { get; set; }
        // public string Spec { get; set; }
        public string Class { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public string Image { get; set; }
        public List<TransmogItem> Items { get; set; }

        public Transmog(Character _character, List<Item> _items)
        {
            // WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{Program.apikey}");

            // Realm
            Realm = _character.Realm;

            // Character Name
            Name = _character.Name;
            
            //Spec
            // TO DO

            // Class
            CharacterClass myClass = _character.Class;
            Class = myClass.ToString();

            // Race
            CharacterRace myRace = _character.Race;
            Race = myRace.ToString();

            // Gender
            CharacterGender myGender = _character.Gender;
            Gender = myGender.ToString();

            // Image
            string url = "http://render-api-us.worldofwarcraft.com/static-render/us";
            string thumbnailUrl = _character.Thumbnail.Replace("avatar", "profilemain");
            Image = $"{url}/{thumbnailUrl}";

            // Items
            // TO DO:
            //      Add all transmogged items
            //      Iterate through which slots are not transmogged, and add them too.

            List<TransmogItem> myTmogItems = new List<TransmogItem>();
            foreach (Item item in _items)
            {
                TransmogItem tmogItem = new TransmogItem(item);
                Console.WriteLine($"DEBUG: ID:{tmogItem.ID} Name:{tmogItem.Name} ");
                myTmogItems.Add(tmogItem);
                // Items.Add(new TransmogItem() { ID=item.Id, Name=Item.Equa } );

            }
            Items = myTmogItems;
        }

    }


}
