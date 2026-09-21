using System;
using System.Collections.Generic;

namespace WDesk.Widgets.Weather;

public class ForecastDay
{
    public DateTime Date { get; set; }
    public double TempMax { get; set; }
    public double TempMin { get; set; }
    public int WeatherCode { get; set; }
    public string Condition { get; set; } = "";
}

public class WeatherData
{
    public string City { get; set; } = "Tehran";
    public double Temperature { get; set; }
    public double TempMax { get; set; }
    public double TempMin { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int WeatherCode { get; set; }
    public string Condition { get; set; } = "Unknown";
    public string ConditionEmoji { get; set; } = "❓";
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<double> HourlyTemps { get; set; } = new();
    public List<ForecastDay> Forecast { get; set; } = new();

    public double ToF(double celsius) => celsius * 9.0 / 5.0 + 32;
    public double Display(double celsius, bool useFahrenheit) => useFahrenheit ? ToF(celsius) : celsius;
    public string Unit(bool useFahrenheit) => useFahrenheit ? "°F" : "°C";
}