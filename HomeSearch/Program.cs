using HomeSearch.Services;

var builder = WebApplication.CreateBuilder(args);
IZillowScraper Zillow = new ZillowScraper();

var startTimeSpan = TimeSpan.Zero;
var periodTimeSpan = TimeSpan.FromMinutes(10);

var timer = new System.Threading.Timer((e) => {
    Task.Run(async () => {
        await Zillow.GenerateHouses();
    });
}, null, startTimeSpan, periodTimeSpan);





// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(c => {
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) {
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
