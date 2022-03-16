using System;
using static JSanitizer.Tests.Controllers.JSanitizerController;

namespace JSanitizer.Tests
{
    public class WeatherForecast
    {
        public WeatherForecast()
        {
            this.Data = new Data();
        }

        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }

        public Data Data { set; get; }
    }
}
