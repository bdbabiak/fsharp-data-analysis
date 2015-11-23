// ========================================================
// Chapter 1: Accessing Data with Type Providers
// ========================================================

// Loads all the libraries that are included in the FsLab package
// (this way we do not need to load them one by one)
#load "packages/FsLab/FsLab.fsx"

// --------------------------------------------------------
// Getting Data from the World Bank
// --------------------------------------------------------

// Open the F# Data namespace and initialize a connection to World Bank
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle

let wb = WorldBankData.GetDataContext()

// Explore some of the indicators available from the World Bank
wb.Countries.``Czech Republic``.CapitalCity
wb.Countries.``Czech Republic``.Indicators
  .``CO2 emissions (kt)``.[2010]


// --------------------------------------------------------
// Calling the Open Weather Map REST API
// --------------------------------------------------------

let apiKey = "b0b8d612591799e312d8b9fc8343fe69"
let baseUrl = "http://api.openweathermap.org/data/2.5"
let forecastUrl = sprintf "%s/forecast/daily?units=metric&APPID=%s" baseUrl apiKey

let makeCityForecastUrl cityName = sprintf "%s&q=%s" forecastUrl cityName

type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/forecast/daily?q=Kiev,UA&units=metric&APPID=b0b8d612591799e312d8b9fc8343fe69">

// Print the weather forecast (type '.' after 'day' to
// see what other information is returned from the service)
let w = Weather.GetSample()
printfn "%s" w.City.Country
for day in w.List do
  printfn "%f" day.Temp.Max

/// Returns the maximal expected temperature for tomorrow
/// for a specified place in the world (typically a city)
let getTomorrowTemp place =
    try
        let url = makeCityForecastUrl place
        let w = Weather.Load(makeCityForecastUrl place)
        let tomorrow = Seq.head w.List
        Some(tomorrow.Temp.Max)
    with
      | :? System.Exception as e ->
        printfn "Failed to load temperature for %s: %s" place e.Message
        None

getTomorrowTemp "Prague"
getTomorrowTemp "Cambridge,UK"

// --------------------------------------------------------
// Plotting Temperatures Around the World
// --------------------------------------------------------

// Get temperatures in capital cities of all countries in the world

//type CityTemperature = {City: string; Temperature: }

let getCapitalTemp (c: WorldBankData.ServiceTypes.Country) =
    let place = c.CapitalCity + "," + c.Name
    printfn "Getting temperature in: %s" place
    let t = getTomorrowTemp place
    match t with
        | Some(x) -> Some(c.Name, x)
        | None -> None

let temperatures =
    (seq(wb.Countries))
        |> Seq.map getCapitalTemp 
        |> Seq.choose id
        |> List.ofSeq

// Plot tomorrow's temperatures on a map
Chart.Geo(temperatures)

// Make the chart nicer by specifying various chart options
// (set the colors for different temperatures - you might
// need to change this when running the code during winter!)

let colors = [| "#80E000";"#E0C000";"#E07B00";"#E02800" |]
let values = [| 0;+15;+30;+45 |]
let axis = ColorAxis(values=values, colors=colors)

temperatures
|> Chart.Geo
|> Chart.WithOptions(Options(colorAxis=axis))
|> Chart.WithLabel "Temp"