using HomeSearch.Model;
using HomeSearch.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeSearch.Controllers {
    [Route("data")]
    [ApiController]
    public class SearchController : ControllerBase {

        private IZillowScraper zillow = new ZillowScraper();

        public SearchController() {

        }

        [HttpGet]
        public List<House> GetAllData() {
            return zillow.GetHouseList();
        }

        [HttpGet("/{id}")]
        public House GetHouseByID(String id) {
            return zillow.GetHouseById(id);
        }
    }
}
