namespace HomeSearch.Model {
    public class House {

        public House(string id, string address, String price, string description, string photos, string link) {
            Id = id;
            Address = address;
            Price = price;
            Description = description;
            Photos = photos;
            Link = link;
        }

        public String Id { get; set; }
        public String Address { get; set; }
        public String Price { get; set; }
        public String Description { get; set; }
        public String Photos { get; set; }
        public String Link { get; set; }
    }
}
