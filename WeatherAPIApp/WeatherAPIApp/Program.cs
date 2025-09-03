
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http; 

public class Program
{
    public static bool running = true; //used for looping, only set to false for exception errors
    public static async Task FetchWeatherAsync(string LatLon) 
    {
        string[] parts = LatLon.Split(","); //splits the string into two parts: lat, lon
        string LatitudeVal = parts[0];
        string LongitudeVal = parts[1];

        //URL for free temperature tracker based on latitude and longitude
        string url = $"https://api.open-meteo.com/v1/forecast?latitude={LatitudeVal}&longitude={LongitudeVal}&current_weather=true";

        using HttpClient client = new HttpClient();

        try
        {
            var response = await client.GetStringAsync(url); //get response
            using JsonDocument doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            var currentWeather = root.GetProperty("current_weather"); //get current weather

            double temperature = currentWeather.GetProperty("temperature").GetDouble(); //get temperature as double
            string windDirection = currentWeather.GetProperty("winddirection").GetDouble().ToString(); //get wind as string

            Console.WriteLine($"Current Temperature: {temperature}°C"); //display both results
            Console.WriteLine($"Wind Direction: {windDirection}°");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex);
            Program.running = false; //dont know if this works as I havent found an exception that causes this part to run
            //return "Exception Error in Lat to Weather Conversion";
        }



    }

    public static async Task<string> FetchLatLonAsync(string cityInput)
    {

        //APIKEY expires soon so this can be changed here
        //URL is for a free API that finds latitude and longitude based on city
        string apiKey = "0b0ee68982314afe96b0dd7119298b4a";
        string url = $"https://api.opencagedata.com/geocode/v1/json?q={cityInput}&key={apiKey}";

        using HttpClient client = new HttpClient();

        try
        {
            var response = await client.GetStringAsync(url); //use await and get stringAsync
            using JsonDocument doc = JsonDocument.Parse(response);
            var results = doc.RootElement.GetProperty("results");

            if (results.GetArrayLength() == 0)
            {
                return "No Results Found"; //if none are found, no results found is shown, and this
                                           //triggers the else statement in the Main()
            }

            var geometry = results[0].GetProperty("geometry"); 

            //for the two lines below I had to get help virtually as I did not know how to get them
            //from all the data that i get from the results
            double lat = geometry.GetProperty("lat").GetDouble(); //retrieves lat
            double lon = geometry.GetProperty("lng").GetDouble(); //retrieves lon

        

            return $"{lat},{lon}"; //returns lat and lon values as one string

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex);
            Program.running = false; //set to false and hopefully this works, havent tested an exception that causes this to run
            return "Exception Error in City to Lat Lon Conversion";
        }

    }

    public static async Task ChooseAnotherCity()
    {
        await Task.Delay(1000); //1 second delay because I felt like adding more delays
        Console.WriteLine("Choose another city:");
    }

    public static async Task Main(string[] args) //Entry point
    {
        Program.running = true; //it is already set to true, but I just did it again incase
        Console.WriteLine("Welcome To The Weather App!");
        Console.WriteLine("Please enter a city:");

        while (Program.running) //checks program running value, and loops based off that
        {
            var cityInput = Console.ReadLine(); //takes input from user

            string latlon = await FetchLatLonAsync(cityInput); //uses await to fetch LatLon values of city
            if (latlon != "No Results Found") //if the return value from the function is not this string, then it is valid
            {
                await Task.WhenAll(FetchWeatherAsync(latlon), ChooseAnotherCity()); //runs both the fetch weather method and choose another city delay prompt concurrently using await
            }                                                                       //so they both show up simultaneously
            else
            {
                Console.WriteLine("Couldn't find the city. Try Again:"); //gives user opportunity to input another city
            }
        }


    }
}
