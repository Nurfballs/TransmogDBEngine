using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WowDotNetAPI;
using WowDotNetAPI.Models;

namespace TransmogDBEngine
{
    class TransmogSet
    {

        public string Realm { get; set; }
        public string Name { get; set; }
        // public string Spec { get; set; }
        public string Class { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public string Image { get; set; }
        public List<TransmogItem> Items { get; set; }
        public DateTime Updated { get; set; }

        public TransmogSet(Character _character, List<TransmogItem> _items)
        //public TransmogSet(Character _character)
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
            //Image = GetCharacterImageURL(_character.Realm, _character.Name);
            string url = "http://render-api-us.worldofwarcraft.com/static-render/us";
            string thumbnailUrl = _character.Thumbnail.Replace("avatar", "profilemain");
            Image = $"{url}/{thumbnailUrl}";

            // Items
            Items = _items;

            // Updated
            Updated = DateTime.Now;
        }

 



    }


}
