using HomeSearch.Model;
using Microsoft.Playwright;
using Npgsql;
using System.Data;

namespace HomeSearch.Services {
    public class ZillowScraper : IZillowScraper {
        private String sqlDataSource = "Host=localhost;Username = postgres;Password=Andrew1021!;Database=HomeFinder";

        public async Task GenerateHouses() {
            
            //Playwright setup to not be detected as bot
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            var context = await browser.NewContextAsync(new BrowserNewContextOptions { UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36" });
            var page = await context.NewPageAsync();
            await page.SetViewportSizeAsync(1600, 1600);
            
            //Navigate to page and scroll to bottom of list to load all elements
            await page.GotoAsync("https://www.zillow.com/homes/for_sale/house,multifamily,mobile,land,townhouse_type/?searchQueryState=%7B%22pagination%22%3A%7B%7D%2C%22usersSearchTerm%22%3A%22OH%22%2C%22mapBounds%22%3A%7B%22west%22%3A-95.52325640625%2C%22east%22%3A-69.81524859375%2C%22south%22%3A33.35486302517181%2C%22north%22%3A46.76734548556455%7D%2C%22mapZoom%22%3A6%2C%22customRegionId%22%3A%229738262716X1-CRw0n589kkac8u_ykjgz%22%2C%22isMapVisible%22%3Atrue%2C%22filterState%22%3A%7B%22doz%22%3A%7B%22value%22%3A%221%22%7D%2C%22price%22%3A%7B%22min%22%3A200000%2C%22max%22%3A300000%7D%2C%22con%22%3A%7B%22value%22%3Afalse%7D%2C%22apa%22%3A%7B%22value%22%3Afalse%7D%2C%22mp%22%3A%7B%22min%22%3A869%2C%22max%22%3A1305%7D%2C%22sort%22%3A%7B%22value%22%3A%22days%22%7D%2C%22apco%22%3A%7B%22value%22%3Afalse%7D%7D%2C%22isListVisible%22%3Atrue%7D");
            var body = page.Locator(".list-card-info");
            await page.HoverAsync(".photo-cards");
            Thread.Sleep(4000);
            while(await page.Locator(".list-card-info").Locator(".list-card-heading").Locator(".list-card-price").CountAsync() < await body.CountAsync()) {
                await page.Mouse.WheelAsync(0, 500);
                Thread.Sleep(1000);
            }
            
            //grab lists of DomElements and convert to lists of specific data types
            var prices = await body.Locator(".list-card-heading").Locator(".list-card-price").AllTextContentsAsync();
            var addresses = await body.Locator(".list-card-addr").AllTextContentsAsync();
            var details = await body.Locator(".list-card-details").AllTextContentsAsync();
            var element = await page.QuerySelectorAllAsync(".list-card-top");
            var idElement = await page.QuerySelectorAllAsync("article");
            List<String> listOfLinks = new List<string>();
            List<String> listOfImages = new List<string>();  
            List<String> listOfIds = new List<string>();

            foreach (var idEl in idElement) {
                var id = await idEl.GetAttributeAsync("id");
                listOfIds.Add(id);
            }

            foreach (var el in element) {
                var images = await el.QuerySelectorAsync("img");
                var image = await images.GetAttributeAsync("src");
                var links = await el.QuerySelectorAsync("a");
                var link = await links.GetAttributeAsync("href");
                listOfLinks.Add(link);
                listOfImages.Add(image);
            }

            //make connection to database and insert values from list of elements above. (Could make helper class for connection and insertion)
            try {
                
                String sql = @"insert into house(house_id, address, price, details, photo, url) values (@id, @address, @price, @details, @photo, @url);";
                DataTable dt = new DataTable();
                NpgsqlDataReader reader;
                NpgsqlConnection connection = new NpgsqlConnection(sqlDataSource);
                connection.Open();

                //if id already exist throws error and doesnt add it to list but will still add new entries
                for (int i = 0; i < prices.Count; i++) {
                    try {
                        NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                        cmd.Parameters.AddWithValue("@id", listOfIds[i]);
                        cmd.Parameters.AddWithValue("@address", addresses[i]);
                        cmd.Parameters.AddWithValue("@price", prices[i]);
                        cmd.Parameters.AddWithValue("@details", details[i]);
                        cmd.Parameters.AddWithValue("@photo", listOfImages[i]);
                        cmd.Parameters.AddWithValue("@url", listOfLinks[i]);
                        reader = cmd.ExecuteReader();
                        dt.Load(reader);
                    }
                    catch (NpgsqlException ex) {
                        

                    }
                    

                }
                

                connection.Close();
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Complete");
         

        }

        //pulls from database and converts datatable object to house objects and then to a list of them from each row in the datatable
        public List<House> GetHouseList() {
            DataTable dt = new DataTable();
            NpgsqlDataReader reader;
            NpgsqlConnection connection = new NpgsqlConnection(sqlDataSource);
            connection.Open();
            String sql = @"select * from house";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            reader = command.ExecuteReader();
            dt.Load(reader);

            reader.Close();
            connection.Close();

            //enumerable is just a generic collection you can iterate through.
            List<House> houses = dt.AsEnumerable().Select(row => new House(row.Field<String>("house_id"), row.Field<String>("address"), row.Field<String>("price"), row.Field<String>("details"), 
                row.Field<String>("photo"), row.Field<String>("url"))).ToList();

            return houses;
        }

        //gets list from database and uses the find method to check if there is a matching id. If not just returns null
        public House GetHouseById(String id) {
            DataTable dt = new DataTable();
            NpgsqlDataReader reader;
            NpgsqlConnection connection = new NpgsqlConnection(sqlDataSource);
            connection.Open();
            String sql = @"select * from house";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            reader = command.ExecuteReader();
            dt.Load(reader);

            reader.Close();
            connection.Close();

            List<House> houses = dt.AsEnumerable().Select(row => new House(row.Field<String>("house_id"), row.Field<String>("address"), row.Field<String>("price"), row.Field<String>("details"),
                row.Field<String>("photo"), row.Field<String>("url"))).ToList();
            return houses.Find(x => x.Id == id);
        }
    }
}
