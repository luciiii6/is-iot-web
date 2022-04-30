﻿using IsIoTWeb.Models;
using IsIoTWeb.Models.Irrigation;
using IsIoTWeb.Models.Valve;
using IsIoTWeb.Mqtt;
using IsIoTWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IsIoTWeb.Controllers
{
    [Authorize]
    public class IrrigationController : Controller
    {
        private IValveRepository _valveRepository;
        private IUserRepository _userRepository;
        private IMqttClient _mqttClient;
        private IConfiguration _configuration;

        public IrrigationController(IValveRepository valveRepository, IUserRepository userRepository, IMqttClient mqttClient, IConfiguration configuration)
        {
            _valveRepository = valveRepository;
            _userRepository = userRepository;
            _mqttClient = mqttClient;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Logs");
        }

        [HttpPost]
        public async Task<ActionResult> GetValveLogsByFilter([FromBody] ValveLogsFilter? filter)
        {
            await _mqttClient.Connect();
            List<ValveLog> valvesLogs = _valveRepository.GetAll().Result.ToList();

            if (filter == null)
            {
                return Json(valvesLogs);
            }
            else
            {
                if (filter.ValveId != null)
                {
                    valvesLogs = valvesLogs.Where(x => x.ValveId == filter.ValveId).ToList();
                }

                if (!string.IsNullOrEmpty(filter.OneDate))
                {
                    valvesLogs = valvesLogs.Where(x => x.Date() == filter.OneDate).ToList();
                }

                if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
                {
                    DateTime fromDatetime = DateTime.Parse(filter.FromDate);
                    DateTime toDatetime = DateTime.Parse(filter.ToDate);
                    valvesLogs = valvesLogs.Where(x => DateTime.Parse(x.Date()) >= fromDatetime && DateTime.Parse(x.Date()) < toDatetime).ToList();
                }
            }

            List<ValveLogDisplay> valvesLogsDisplay = new List<ValveLogDisplay>();
            var users = await _userRepository.GetAll();

            foreach (var valveLog in valvesLogs)
            {
                ValveLogDisplay valveLogDisplay = new ValveLogDisplay();
                valveLogDisplay.ValveId = valveLog.ValveId;
                valveLogDisplay.Timestamp = valveLog.Timestamp;
                valveLogDisplay.Action = valveLog.Action;

                if (valveLog.UserId != null)
                {
                    var user = users.First(x => x.Id.ToString() == valveLog.UserId);
                    string fullName = user == null ? "" : user.FirstName + " " + user.LastName;
                    valveLogDisplay.User = new ValveLogDisplay.UserInfo(valveLog.UserId, fullName);
                }
                else
                {
                    valveLogDisplay.User = new ValveLogDisplay.UserInfo("", "");
                }

                valvesLogsDisplay.Add(valveLogDisplay);
            }

            return Json(valvesLogsDisplay);
        }

        public IActionResult Logs()
        {
            return View();
        }

        public async Task<IActionResult> Manual()
        {
            await _mqttClient.Connect();
            RequestStatus();
            await _mqttClient.Subscribe("/valves/status/response/");
            return View();
        }

        public IActionResult Automated()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> WeatherData()
        {
            WeatherData weatherData = new WeatherData();
            var apiKey = _configuration.GetSection("AccuWeather").GetSection("ApiKey").Value;
            var location = "240499";
            var url = $"https://dataservice.accuweather.com/forecasts/v1/hourly/1hour/{location}?apikey={apiKey}&details=true&metric=true";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // TODO: Remove comments. Used hard coded response to stop consuming api in development.
                    //HttpResponseMessage response = await client.GetAsync(url);
                    //var data_str = response.Content.ReadAsStringAsync().Result;
                    var data_str = "[{\"DateTime\":\"2022-04-30T16:00:00-05:00\",\"EpochDateTime\":1651352400,\"WeatherIcon\":1,\"IconPhrase\":\"Cloudy\",\"HasPrecipitation\":false,\"IsDaylight\":true,\"Temperature\":{\"Value\":15.7,\"Unit\":\"C\",\"UnitType\":17},\"RealFeelTemperature\":{\"Value\":30.9,\"Unit\":\"C\",\"UnitType\":17,\"Phrase\":\"Very Warm\"},\"RealFeelTemperatureShade\":{\"Value\":27.3,\"Unit\":\"C\",\"UnitType\":17,\"Phrase\":\"Very Warm\"},\"WetBulbTemperature\":{\"Value\":13.9,\"Unit\":\"C\",\"UnitType\":17},\"DewPoint\":{\"Value\":1.1,\"Unit\":\"C\",\"UnitType\":17},\"Wind\":{\"Speed\":{\"Value\":11.1,\"Unit\":\"km/h\",\"UnitType\":7},\"Direction\":{\"Degrees\":92,\"Localized\":\"E\",\"English\":\"E\"}},\"WindGust\":{\"Speed\":{\"Value\":27.8,\"Unit\":\"km/h\",\"UnitType\":7}},\"RelativeHumidity\":17,\"IndoorRelativeHumidity\":17,\"Visibility\":{\"Value\":16.1,\"Unit\":\"km\",\"UnitType\":6},\"Ceiling\":{\"Value\":9144.0,\"Unit\":\"m\",\"UnitType\":5},\"UVIndex\":5,\"UVIndexText\":\"Moderate\",\"PrecipitationProbability\":0,\"ThunderstormProbability\":0,\"RainProbability\":32,\"SnowProbability\":0,\"IceProbability\":0,\"TotalLiquid\":{\"Value\":0.0,\"Unit\":\"mm\",\"UnitType\":3},\"Rain\":{\"Value\":0.0,\"Unit\":\"mm\",\"UnitType\":3},\"Snow\":{\"Value\":0.0,\"Unit\":\"cm\",\"UnitType\":4},\"Ice\":{\"Value\":0.0,\"Unit\":\"mm\",\"UnitType\":3},\"CloudCover\":3,\"Evapotranspiration\":{\"Value\":0.5,\"Unit\":\"mm\",\"UnitType\":3},\"SolarIrradiance\":{\"Value\":1305.4,\"Unit\":\"W/m²\",\"UnitType\":33},\"MobileLink\":\"http://www.accuweather.com/en/mx/pozas-de-santa-ana/240499/hourly-weather-forecast/240499?day=1&hbhhour=16&unit=c&lang=en-us\",\"Link\":\"http://www.accuweather.com/en/mx/pozas-de-santa-ana/240499/hourly-weather-forecast/240499?day=1&hbhhour=16&unit=c&lang=en-us\"}]";

                    dynamic data = JObject.Parse(JArray.Parse(data_str)[0].ToString());

                    //if (response.IsSuccessStatusCode)
                    //{
                        weatherData.Temperature = data.Temperature.Value.Value;
                        weatherData.RainProbability = data.RainProbability.Value;
                        weatherData.Condition = data.IconPhrase.Value;
                        weatherData.Date = data.DateTime.Value.ToShortDateString();
                        return Json(weatherData);
                    //}
                }
            }
            catch (Exception e) { }

            weatherData.Condition = "unknown";
            return Json(weatherData);
        }

        public IActionResult Configure()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddSchedule()
        {
            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<ActionResult> Action([FromBody] ValveActionInput valveActionInput)
        {
            await _mqttClient.Connect();

            var valveState = GetLastValvesState().Where(x => x.ValveId == valveActionInput.ValveId).FirstOrDefault();
            if (valveState == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"Valve [{valveActionInput.ValveId}] does not exist.");
            }

            ValveAction valveAction = new ValveAction(valveActionInput.ValveId,
                    valveActionInput.Action,
                    _userRepository.GetLoggedUserByUsername(User.Identity.Name).Result.Id.ToString()
                    );

            await _mqttClient.Publish($"/valves/control/", JsonConvert.SerializeObject(valveAction, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
            RequestStatus();

            if ((valveState.State == "ON" && valveActionInput.Action == "TURN_OFF") || (valveState.State == "OFF" && valveActionInput.Action == "TURN_ON"))
            {
                return StatusCode((int)HttpStatusCode.OK);
            }
            else if (valveState.State == "OFF" && valveActionInput.Action == "TURN_OFF")
            {
                return StatusCode((int)HttpStatusCode.Accepted, $"Request sent, but valve [{valveActionInput.ValveId}] was already turned off.");
            }
            else if (valveState.State == "ON" && valveActionInput.Action == "TURN_ON")
            {
                return StatusCode((int)HttpStatusCode.Accepted, $"Request sent, but valve [{valveActionInput.ValveId}] was already turned on.");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Accepted, $"Undefined.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> TurnAllOff()
        {
            await _mqttClient.Connect();
            List<ValveState> valvesStates = GetLastValvesState();
            foreach (var valve in valvesStates)
            {
                ValveAction action = new ValveAction(valve.ValveId,
                    "TURN_OFF",
                    _userRepository.GetLoggedUserByUsername(User.Identity.Name).Result.Id.ToString());

                await _mqttClient.Publish($"/valves/control/", JsonConvert.SerializeObject(action, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
            }
            RequestStatus();
            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<JsonResult> GetValvesState()
        {
            RequestStatus();
            return Json(GetLastValvesState());
        }

        private List<ValveState> GetLastValvesState()
        {
            var message = _mqttClient.GetLastPayload();
            List<ValveState> valves = new List<ValveState>();
            if (!string.IsNullOrEmpty(message))
            {
                valves = JsonConvert.DeserializeObject<List<ValveState>>(message);
            }
            return valves;
        }

        private async void RequestStatus()
        {
            await _mqttClient.Publish($"/valves/status/request/", "{}");
        }
    }
}
