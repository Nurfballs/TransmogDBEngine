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

        public static void Main(string[] args)
        {

            //// Get a random realm
            // string realm = GetRandomRealm();
            // Console.WriteLine($"{DateTime.Now}: [+] Selected: {realm}");

            //// Select a random auction owner
            // string auctionOwner = GetRandomAuctionOwner(realm);
            // Console.WriteLine($"{DateTime.Now}: [+] Selected: {auctionOwner}");

            //// Get guild information
            // string guild = GetGuild(realm, auctionOwner);
            // Console.WriteLine($"{DateTime.Now}: [+] Guild is: {guild}" );

            //DEBUG
            string realm = "Aggramar";
            //string name = "Orwell";
            string guild = "The Enclave";

            IEnumerable<GuildMember> lvl100Members = GetLevel100GuildMembers(realm, guild);

            foreach (GuildMember member in lvl100Members)
            {
                Console.WriteLine($"{DateTime.Now}: [*] {member.Character.Name} is level {member.Character.Level} and has guild rank {member.Rank}");

                // Enumerate Transmogged Items
                List<Item> TransmogrifiedItems = GetTransmogrifiedItems(realm, member.Character.Name);
                Console.WriteLine($"{DateTime.Now}: [+] {member.Character.Name} has {TransmogrifiedItems.Count()} transmogrified items");

                if (TransmogrifiedItems.Count() > 3)
                {
                    foreach (Item item in TransmogrifiedItems)
                    {
                        Console.WriteLine($"{DateTime.Now}: [*] Transmog: {item.Name} ");

                    }
                    // Get the image url for the character
                    Console.WriteLine($"{DateTime.Now}: [+] {GetCharacterImageURL(member.Character.Realm, member.Character.Name)}");

                    // Create Transmog Object
                    WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");
                    Character character = explorer.GetCharacter(member.Character.Realm, member.Character.Name, CharacterOptions.GetEverything);
                    //Transmog tmog = new Transmog(character, TransmogrifiedItems);
                    Transmog tmog = new Transmog(character);


                    // Console.WriteLine(JsonConvert.SerializeObject(tmog));
                    // Post the transmog to the API
                    string url = "http://localhost:50392/api/REST/Add";
                    Console.WriteLine($"{DateTime.Now}: [*] Posting JSON to {url}");
                    Console.WriteLine(PostTransmog(url, tmog));

                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}: [-] {member.Character.Name} has < 3 transmogged items. Skipping ...");
                }

            }


            Console.WriteLine($"{DateTime.Now}: [+] Done!");
            Console.ReadKey();

        }

        public static string GetRandomRealm()
        {

            Console.WriteLine($"{DateTime.Now}: [*] Getting random realm...");

            // Get a random realm
            WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");
            IEnumerable<Realm> _realms = explorer.GetRealms();

            Random rnd = new Random();
            int r = rnd.Next(_realms.Count());

            List<Realm> RealmList = _realms.ToList();
            Realm randomRealm = RealmList[r];

            return randomRealm.Name;
        }

        public static string GetRandomAuctionOwner(string _realm)
        {
            Console.WriteLine($"{DateTime.Now}: [*] Getting a random auction owner from {_realm} ...");

            WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");
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
            WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");

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
                     
                    // character lookup failed
                    Console.WriteLine($"{DateTime.Now}: [!] Unable to look up character information for {_name} of {_realm}");
                    Console.WriteLine($"{DateTime.Now}: [!] {ex.Message}");

                    // Get anothe random auction owner from the same realm.
                    _name = GetRandomAuctionOwner(_realm);
                }
            }

            return _character.Guild.Name;
        }

        public static IEnumerable<GuildMember> GetLevel100GuildMembers(string _realm, string _guild)
        {
            WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");

            // Get the guild information
            Guild guild = explorer.GetGuild(_realm, _guild, GuildOptions.GetEverything);
            Console.WriteLine($"{DateTime.Now}: [*] {guild.Name} is a guild of level {guild.Level} and has {guild.Members.Count()} members.");

            // Select all level 100 characters in the guild.
            IEnumerable<GuildMember> lvl100Members = guild.Members.Where(m => m.Character.Level == 100);
            Console.WriteLine($"{DateTime.Now}: [*] {lvl100Members.Count()} of those members are level 100.");


            return lvl100Members;
        }

        public static List<Item> GetTransmogrifiedItems(string _realm, string _character)
        {
            // int slotCount = 0;
            WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");
            Character character = explorer.GetCharacter(_realm, _character, CharacterOptions.GetItems);

            List<Item> TransmogrifiedItems = new List<Item>();

            Console.WriteLine($"{DateTime.Now}: [*] Enumerating Transmogged items for {_character} ...");
            if (character.Items.Back.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Back.TooltipParams.TransmogItem)); }
            if (character.Items.Chest.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Chest.Id)); }
            if (character.Items.Feet.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Feet.Id)); }
            if (character.Items.Head.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Head.Id)); }
            if (character.Items.Hands.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Hands.Id)); }
            if (character.Items.Legs.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Legs.Id)); }
            if (character.Items.MainHand.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.MainHand.Id)); }

            // Offhand
            if (character.Items.OffHand != null)
            {
                if (character.Items.OffHand.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.OffHand.Id)); }
            }

            // Ranged
            if (character.Items.Ranged != null)
            {
                if (character.Items.Ranged.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Ranged.Id)); }

            }

            // Shirt
            if (character.Items.Shirt != null)
            {
                if (character.Items.Shirt.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Shirt.Id)); }
            }

            if (character.Items.Shoulder.TooltipParams.TransmogItem != 0) { TransmogrifiedItems.Add(explorer.GetItem(character.Items.Shoulder.Id)); }

            return TransmogrifiedItems;

        }

        public static string GetCharacterImageURL(string _realm, string _character)
        {
            string url = "http://render-api-us.worldofwarcraft.com/static-render/us";

            WowExplorer explorer = new WowExplorer(Region.US, Locale.en_US, $"{apikey}");
            Character character = explorer.GetCharacter(_realm, _character, CharacterOptions.GetItems);

            string thumbnailUrl = character.Thumbnail.Replace("avatar", "profilemain");

            url = $"{url}/{thumbnailUrl}";

            return url;
        }

        public static string PostTransmog(string _url, Transmog _transmog)
        {
            string result = "";
            string json = JsonConvert.SerializeObject(_transmog);
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                result = client.UploadString(_url, "POST", json);
            }

            return result;
        }
    }
}
