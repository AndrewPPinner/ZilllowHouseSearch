using HomeSearch.Model;

namespace HomeSearch.Services {
    public interface IZillowScraper {

        public async Task GenerateHouses() { }

        public List<House> GetHouseList();

        public House GetHouseById(String id);
    }
}
