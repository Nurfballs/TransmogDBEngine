using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowDotNetAPI;
using WowDotNetAPI.Models;
using System.Web;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace TransmogDBEngine
{
    class Program
    {
        public static string apikey = "2zews2ar44bs2pk3yny7whjr7hx9m5r9";
        public static WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");

        public static void Main(string[] args)
        {

            while (true)
            {
                //// Get a random realm
                Realm realm = GetRandomRealm();
                Console.WriteLine($"{DateTime.Now}: [+] Selected: {realm.Name}");

                // Select a random auction owner
                string auctionOwner = GetRandomAuctionOwner(realm.Name);
                Console.WriteLine($"{DateTime.Now}: [+] Selected: {auctionOwner}");

                // Get guild information
                string guild = GetGuild(realm.Name, auctionOwner);
                Console.WriteLine($"{DateTime.Now}: [+] Guild is: {guild}");

                //DEBUG
                //IEnumerable<Realm> _realms = explorer.GetRealms();
                //Realm realm = _realms.Where(i => i.Name == "Caelestrasz").FirstOrDefault();
                //string name = "Katora";
                //string guild = "Affinity";

                IEnumerable<GuildMember> lvl100Members = new List<GuildMember>();

                try
                {
                    // IEnumerable<GuildMember> lvl100Members = GetLevel100GuildMembers(realm, guild);
                    lvl100Members = GetLevel100GuildMembers(realm, guild);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"{DateTime.Now}: [!] Unable to look up guild information for {guild}");
                    Console.WriteLine($"{DateTime.Now}: [!] {ex.Message}");
                    //throw;
                    continue;
                }
            


            foreach (GuildMember member in lvl100Members)
            {
                     Console.WriteLine($"{DateTime.Now}: [*] {member.Character.Name} is level {member.Character.Level} and has guild rank {member.Rank}");

                    // See if the character exists already
                    HttpStatusCode result = LookupCharacter(member.Character.Realm, member.Character.Name).StatusCode;
                    switch (result)
                    {
                        case HttpStatusCode.Found:
                            Console.WriteLine($"{DateTime.Now}: [!] Character already exists. Skiping ...");
                            continue;
                        case HttpStatusCode.NotFound:
                            //Console.WriteLine($"{DateTime.Now}: [+] Character not found.");
                            break;
                        default:
                            Console.WriteLine($"{DateTime.Now}: [!] An error has occurred checking this character in the database.");
                            Console.WriteLine($"{DateTime.Now}: {result.ToString()}");
                            continue;
                    }

                    // Get the character info for assessment
                    Character character = explorer.GetCharacter(member.Character.Realm, member.Character.Name, CharacterOptions.GetEverything);

                    // Validate the image url
                    string imgUrl = ValidateCharacterImageURL(realm, character);
                    if (imgUrl == null)
                    {
                        // failed to lookup the image url
                        continue;
                    }

                    // Enumerate Transmogged Items
                    List<TransmogItem> TransmogrifiedItems = new List<TransmogItem>();
                    try
                    {
                       TransmogrifiedItems = GetTransmogrifiedItems(realm, member.Character.Name);
                    }
                        catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now}: [!] Unable to lookup character info for {member.Character.Name}");
                        Console.WriteLine($"{DateTime.Now}: [!] {ex.Message}");
                        continue;
                    }
                        
                
                Console.WriteLine($"{DateTime.Now}: [+] {member.Character.Name} has {TransmogrifiedItems.Where(i => i.Transmogrified == true).Count()} transmogrified items");



                // if (TransmogrifiedItems.Count() > 3)
                if (TransmogrifiedItems.Where(i=>i.Transmogrified == true).Count() > 3)
                {

                    // Create Transmog Object
                    // Character character = explorer.GetCharacter(member.Character.Realm, member.Character.Name, CharacterOptions.GetEverything);

                    TransmogSet Appearance = new TransmogSet(character, TransmogrifiedItems);
                    // TransmogSet Appearance = new TransmogSet(character);

                    // Post the transmog to the API
                    string url = "http://localhost:50392/api/REST/Add";
                    Console.WriteLine($"{DateTime.Now}: [*] Posting JSON to {url}");
                    Console.WriteLine(PostTransmog(url, Appearance));
                    
                    // Console.ReadKey(); 
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}: [-] {member.Character.Name} has < 3 transmogged items. Skipping ...");
                }

            }

                Console.WriteLine("");
            // Console.ReadKey(); // DEBUG
            }
        }

        public static Realm GetRandomRealm()
        {

            Console.WriteLine($"{DateTime.Now}: [*] Getting random realm...");

            // Get a random realm
            IEnumerable<Realm> _realms = explorer.GetRealms();

            Random rnd = new Random();
            int r = rnd.Next(_realms.Count());

            List<Realm> RealmList = _realms.ToList();
            Realm randomRealm = RealmList[r];

            return randomRealm;
        }

        public static string GetRandomAuctionOwner(string _realm)
        {
            Console.WriteLine($"{DateTime.Now}: [*] Getting a random auction owner from {_realm} ...");

            Auctions _auctions = explorer.GetAuctions(_realm);
            List<Auction> AuctionList = _auctions.CurrentAuctions.ToList();


            Random rnd = new Random();
            int r = rnd.Next(AuctionList.Count());
            Auction randomAuction = AuctionList[r];

            string auctionOwner = randomAuction.Owner;
            return auctionOwner;
        }

        public static string GetGuild(string _realm, string _name)
        {
            Character _character = new Character();

            // DEBUG
            // _name = "Orwell";
            // _realm = "Aggramar";

            while (_character.Guild == null)
            {
                Console.WriteLine($"{DateTime.Now}: [*] Getting guild information for {_name} of {_realm}");

                try
                {
                    // look up the character info
                    _character = explorer.GetCharacter(_realm, _name, CharacterOptions.GetStats | CharacterOptions.GetGuild);
                    

                  if (_character.Guild == null) {
                        // Auction owner is not a member of a guild
                        Console.WriteLine($"{DateTime.Now}: [!] {_name} of {_realm} is not a member of a guild.");
                        
                        // Get anothe random auction owner from the same realm.
                        _name = GetRandomAuctionOwner(_realm);
                    }

                }
                catch (Exception ex)
                {
                     
                    Console.WriteLine($"{DateTime.Now}: [!] Unable to look up character information for {_name} of {_realm}");
                    Console.WriteLine($"{DateTime.Now}: [!] {ex.Message}");

                    // Get anothe random auction owner from the same realm.
                    _name = GetRandomAuctionOwner(_realm);
                }
            }

            return _character.Guild.Name;
        }

        public static IEnumerable<GuildMember> GetLevel100GuildMembers(Realm _realm, string _guild)
        {

            // Get the guild information
            Guild guild = explorer.GetGuild(explorer.Region, _realm.Name, _guild, GuildOptions.GetEverything);
            Console.WriteLine($"{DateTime.Now}: [*] {guild.Name} is a guild of level {guild.Level} and has {guild.Members.Count()} members.");

            // Select all level 100 characters in the guild.
            IEnumerable<GuildMember> lvl100Members = guild.Members.Where(m => m.Character.Level == 100);
            Console.WriteLine($"{DateTime.Now}: [*] {lvl100Members.Count()} of those members are level 100.");


            return lvl100Members;
        }

        public static List<TransmogItem> GetTransmogrifiedItems(Realm _realm, string _character)
        {
            Character character = explorer.GetCharacter(explorer.Region, _realm.Name, _character, CharacterOptions.GetItems);
            //Character character = explorer.GetCharacter(_realm, _character, CharacterOptions.GetItems);

            // List<Item> TransmogrifiedItems = new List<Item>();
            List<TransmogItem> TransmogrifiedItems = new List<TransmogItem>();

            Console.WriteLine($"{DateTime.Now}: [*] Enumerating items for {_character} ...");

            // Back
            if (character.Items.Back.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Back.TooltipParams.TransmogItem), true, SlotType.BACK));
            }  else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Back.Id), false, SlotType.BACK));
            }

            // Chest
            if (character.Items.Chest.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Chest.TooltipParams.TransmogItem), true, SlotType.CHEST));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Chest.Id), false, SlotType.CHEST));
            }

            // Feet
            if (character.Items.Feet.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Feet.TooltipParams.TransmogItem), true, SlotType.FEET));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Feet.Id), false, SlotType.FEET));
            }


            // Head
            if (character.Items.Head.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Head.TooltipParams.TransmogItem), true, SlotType.HEAD));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Head.Id), false, SlotType.HEAD));
            }

            // Hands
            if (character.Items.Hands.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Hands.TooltipParams.TransmogItem), true, SlotType.HANDS));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Hands.Id), false, SlotType.HANDS));
            }

            // Legs
            if (character.Items.Legs.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Legs.TooltipParams.TransmogItem), true, SlotType.LEGS));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Legs.Id), false, SlotType.LEGS));
            }
            // MainHand
            if (character.Items.MainHand.TooltipParams.TransmogItem != 0) {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.MainHand.TooltipParams.TransmogItem), true, SlotType.MAINHAND));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.MainHand.Id), false, SlotType.MAINHAND));
            }

            // Offhand
            if (character.Items.OffHand != null)
            {
                 if (character.Items.OffHand.TooltipParams.TransmogItem != 0)
                    {
                    TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.OffHand.TooltipParams.TransmogItem), true, SlotType.OFFHAND));
                } else
                {
                    TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.OffHand.Id), false, SlotType.OFFHAND));
                }
            }

            // Ranged
            if (character.Items.Ranged != null)
            { 
                if (character.Items.Ranged.TooltipParams.TransmogItem != 0)
                {
                    TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Ranged.TooltipParams.TransmogItem), true, SlotType.RANGED));
                } else
                {
                    TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Ranged.Id), false, SlotType.RANGED));
                }
            }

            // SHIRT
            if (character.Items.Shirt != null)
            {
                if (character.Items.Shirt.TooltipParams.TransmogItem != 0)
                {
                    TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Shirt.TooltipParams.TransmogItem), true, SlotType.SHIRT));
                }
                else {
                    TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Shirt.Id), false, SlotType.SHIRT));
                }                
             }


            // SHOULDER
            if (character.Items.Shoulder.TooltipParams.TransmogItem != 0) 
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Shoulder.TooltipParams.TransmogItem), true, SlotType.SHOULDER));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Shoulder.Id), false, SlotType.SHOULDER));
            }

            // WAIST
            if (character.Items.Waist.TooltipParams.TransmogItem != 0)
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Waist.TooltipParams.TransmogItem), true, SlotType.WAIST));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Waist.Id), false, SlotType.WAIST));
            }

            // WRIST
            if (character.Items.Wrist.TooltipParams.TransmogItem != 0)
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Wrist.TooltipParams.TransmogItem), true, SlotType.WRIST));
            }
            else
            {
                TransmogrifiedItems.Add(new TransmogItem(explorer.GetItem(character.Items.Wrist.Id), false, SlotType.WRIST));
            }



            return TransmogrifiedItems;

        }

        public static string ValidateCharacterImageURL(Realm _realm, Character _character)
        {
            string url = "http://render-api-us.worldofwarcraft.com/static-render/us";
            //WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");
            //Character character = explorer.GetCharacter(_realm.Name, _character.Name, CharacterOptions.GetItems);
            string thumbnailUrl = _character.Thumbnail.Replace("avatar", "profilemain");
            string fullUrl = $"{url}/{thumbnailUrl}";

            try
            {
                // creat the webrequest
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(fullUrl);

                //send request and wait for response
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"{DateTime.Now}: [+] Profile image has been validated.");
                }
            }
            catch (WebException e)
            {
                Console.WriteLine($"{DateTime.Now}: [!] Unable to lookup profile image for {_character.Name}");
                Console.WriteLine($"{DateTime.Now}: [!] {e.Message}");
                fullUrl = null;
                //throw;
            }
            return fullUrl;
        }

        public static string PostTransmog(string _url, TransmogSet _transmog)
        {
            string result = "";
            string json = JsonConvert.SerializeObject(_transmog);
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                try
                {
                    result = client.UploadString(_url, "POST", json);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"{DateTime.Now}: [!] An error has occurred posting data to the web API.");
                    Console.WriteLine($"{DateTime.Now}: [!] {ex.Message}");
                }
                
            }

            return result;
        }

        public static HttpWebResponse LookupCharacter(string _realm, string _character)
        {

            Console.WriteLine($"{DateTime.Now}: [*] Checking if {_character} of {_realm} already exists in the database");

            // create the webrequest
            string url = $"http://localhost:50392/api/Lookup?realm={_realm}&character={_character}";
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            //send request and wait for response
            HttpWebResponse response;
            try
            {
                response = myHttpWebRequest.GetResponse() as HttpWebResponse;
                //HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            }
            catch (WebException ex)
            {

                response = ex.Response as HttpWebResponse;
            }

            return response;
            
        }
    }
}
