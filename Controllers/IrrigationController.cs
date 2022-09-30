using IsIoTWeb.Models;
using IsIoTWeb.Models.Irrigation;
using IsIoTWeb.Models.Schedule;
using IsIoTWeb.Models.Valve;
using IsIoTWeb.Mqtt;
using IsIoTWeb.Repository;
using IsIoTWeb.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
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
    public class IrrigationController : Controller
    {
        private IValveRepository _valveRepository;
        private IUserRepository _userRepository;
        private IIrrigationRepository _irrigationRepository;
        private IScheduleRepository _scheduleRepository;
        private IMqttClient _mqttClient;
        private IConfiguration _configuration;

        public IrrigationController(IValveRepository valveRepository, 
            IUserRepository userRepository,
            IIrrigationRepository irrigationRepository, 
            IScheduleRepository scheduleRepository,
            IMqttClient mqttClient,
            IConfiguration configuration)
        {
            _valveRepository = valveRepository;
            _userRepository = userRepository;
            _irrigationRepository = irrigationRepository;
            _scheduleRepository = scheduleRepository;
            _mqttClient = mqttClient;
            _configuration = configuration;
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult Index()
        {
            return RedirectToAction("Logs");
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public async Task<ActionResult> GetValveLogsByFilter([FromBody] ValveLogsFilter? filter)
        {
            if (filter == null)
            {
                return Json(new Error() { ErrorMessages = { "Filter is null!" } });
            }

            try
            {
                await _mqttClient.Connect();
            }
            catch(Exception)
            {
                return Json(new Error() { ErrorMessages = { "MQTT Broker is not available!" } });
            }

            List<ValveLog> valvesLogs;
            try
            {
                valvesLogs = _valveRepository.GetAllByFilter(filter).Result.ToList();
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching Valves' data!" } });
            }

            List<User> users;
            try
            {
                users = (List<User>)await _userRepository.GetAll();
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching Users' data!" } });
            }

            List<ValveLogDisplay> valvesLogsDisplay = new List<ValveLogDisplay>();
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

        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult Logs()
        {
            return View();
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        public async Task<IActionResult> Manual()
        {
            try
            {
                await _mqttClient.Connect();
            }
            catch (Exception)
            {
                return View(new Error() { ErrorMessages = { "MQTT Broker is not available!" } });
            }

            try
            {
                RequestStatus();
                await _mqttClient.Subscribe($"/{StaticVariables.SinkId}/valves/status/response/");
            }
            catch (Exception)
            {
                return View(new Error() { ErrorMessages = { "An error occured when fetching Valves' status!" } });
            }

            return View();
        }

        [Authorize(Roles = "VIEWER,ADMINISTRATOR")]
        public IActionResult Automated()
        {
            return View();
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult Scheduled()
        {
            return View();
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public ActionResult GetSchedules()
        {
            try
            {
                return Json(_scheduleRepository.GetAll().Result);
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching schedules' data!" } });
            }
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSchedule(string id)
        {
            try
            {
                var objectId = new ObjectId(id);
                await _scheduleRepository.Delete(objectId);
                
            }
            catch (Exception)
            {
                /* Do nothing. Return OK. */
            }
            return StatusCode((int)HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<JsonResult> WeatherData()
        {
            WeatherData weatherData = new WeatherData();
            var apiKey = _configuration.GetSection("AccuWeather").GetSection("ApiKey").Value;
            var location = "1161143";
            var url = $"https://dataservice.accuweather.com/forecasts/v1/hourly/1hour/{location}?apikey={apiKey}&details=true&metric=true";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // TODO: Remove comments. Used hard coded response to stop consuming api in development.
                    HttpResponseMessage response = await client.GetAsync(url);
                    var data_str = response.Content.ReadAsStringAsync().Result;
                    //var data_str = "[{\"DateTime\":\"2022-05-12T16:00:00-05:00\",\"EpochDateTime\":1651352400,\"WeatherIcon\":1,\"IconPhrase\":\"Cloudy\",\"HasPrecipitation\":false,\"IsDaylight\":true,\"Temperature\":{\"Value\":15.7,\"Unit\":\"C\",\"UnitType\":17},\"RealFeelTemperature\":{\"Value\":30.9,\"Unit\":\"C\",\"UnitType\":17,\"Phrase\":\"Very Warm\"},\"RealFeelTemperatureShade\":{\"Value\":27.3,\"Unit\":\"C\",\"UnitType\":17,\"Phrase\":\"Very Warm\"},\"WetBulbTemperature\":{\"Value\":13.9,\"Unit\":\"C\",\"UnitType\":17},\"DewPoint\":{\"Value\":1.1,\"Unit\":\"C\",\"UnitType\":17},\"Wind\":{\"Speed\":{\"Value\":11.1,\"Unit\":\"km/h\",\"UnitType\":7},\"Direction\":{\"Degrees\":92,\"Localized\":\"E\",\"English\":\"E\"}},\"WindGust\":{\"Speed\":{\"Value\":27.8,\"Unit\":\"km/h\",\"UnitType\":7}},\"RelativeHumidity\":17,\"IndoorRelativeHumidity\":17,\"Visibility\":{\"Value\":16.1,\"Unit\":\"km\",\"UnitType\":6},\"Ceiling\":{\"Value\":9144.0,\"Unit\":\"m\",\"UnitType\":5},\"UVIndex\":5,\"UVIndexText\":\"Moderate\",\"PrecipitationProbability\":0,\"ThunderstormProbability\":0,\"RainProbability\":32,\"SnowProbability\":0,\"IceProbability\":0,\"TotalLiquid\":{\"Value\":0.0,\"Unit\":\"mm\",\"UnitType\":3},\"Rain\":{\"Value\":0.0,\"Unit\":\"mm\",\"UnitType\":3},\"Snow\":{\"Value\":0.0,\"Unit\":\"cm\",\"UnitType\":4},\"Ice\":{\"Value\":0.0,\"Unit\":\"mm\",\"UnitType\":3},\"CloudCover\":3,\"Evapotranspiration\":{\"Value\":0.5,\"Unit\":\"mm\",\"UnitType\":3},\"SolarIrradiance\":{\"Value\":1305.4,\"Unit\":\"W/m²\",\"UnitType\":33},\"MobileLink\":\"http://www.accuweather.com/en/mx/pozas-de-santa-ana/240499/hourly-weather-forecast/240499?day=1&hbhhour=16&unit=c&lang=en-us\",\"Link\":\"http://www.accuweather.com/en/mx/pozas-de-santa-ana/240499/hourly-weather-forecast/240499?day=1&hbhhour=16&unit=c&lang=en-us\"}]";

                    dynamic data = JObject.Parse(JArray.Parse(data_str)[0].ToString());

                    if (response.IsSuccessStatusCode)
                    {
                        weatherData.Temperature = data.Temperature.Value.Value;
                        weatherData.RainProbability = data.RainProbability.Value;
                        weatherData.Condition = data.IconPhrase.Value;
                        weatherData.Date = data.DateTime.Value.ToShortDateString();
                        return Json(weatherData);
                    }
                }
            }
            catch (Exception) { }

            weatherData.Condition = "unknown";
            return Json(weatherData);
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult Configure()
        {
            return View();
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public ActionResult AddSchedule([FromBody] ScheduleInput scheduleInput)
        {
            if (scheduleInput == null)
            {
                return Json(new Error() { ErrorMessages = { "The sent Schedule is null!" } });
            }

            List<Schedule> schedules = new List<Schedule>();
            var fromDate = DateTime.Parse(scheduleInput.FromDate);
            var toDate = DateTime.Parse(scheduleInput.ToDate);

            for (DateTime date = fromDate; date <= toDate; date = date.AddDays(1.0))
            {
                foreach (ScheduleEntry entry in scheduleInput.Entries)
                {
                    Double hours = double.Parse(entry.Time.Substring(0, 2));
                    Double minutes = double.Parse(entry.Time.Substring(3, 2));
                    double timestamp = new DateTimeOffset(date.AddHours(hours).AddMinutes(minutes)).ToUnixTimeSeconds();

                    schedules.Add(new Schedule()
                    {
                        Timestamp = timestamp,
                        Duration = entry.Duration
                    });
                }
            }
            
            foreach(var schedule in schedules)
            {
                try
                {
                    _scheduleRepository.Create(schedule);
                }
                catch (Exception)
                {
                    return Json(new Error() { ErrorMessages = { "An error occured when tried to add the schedule!" } });
                }
            }
            
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public async Task<ActionResult> Action([FromBody] ValveActionInput valveActionInput)
        {
            if (valveActionInput == null)
            {
                return Json(new Error() { ErrorMessages = { "Valve Action is null!" } });
            }

            try
            {
                await _mqttClient.Connect();
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "MQTT Broker is not available!" } });
            }

            var valveState = GetLastValvesState().Where(x => x.ValveId == valveActionInput.ValveId).FirstOrDefault();
            if (valveState == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"Valve [{valveActionInput.ValveId}] does not exist.");
            }

            ValveAction valveAction;
            try
            {
                valveAction = new ValveAction(valveActionInput.ValveId,
                        valveActionInput.Action,
                        _userRepository.GetLoggedUserByUsername(User.Identity.Name).Result.Id.ToString()
                        );
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "Failed to fetch logged user. Please reauthenticate!" } });
            }

            try
            {
                await _mqttClient.Publish($"/{StaticVariables.SinkId}/valves/control/", JsonConvert.SerializeObject(valveAction, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"Sending valve action request failed!");
            }

            try
            {
                RequestStatus();
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching Valves' status!" } });
            }

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

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public async Task<ActionResult> TurnAllOff()
        {
            try
            {
                await _mqttClient.Connect();
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "MQTT Broker is not available!" } });
            }

            List<ValveState> valvesStates = GetLastValvesState();
            foreach (var valve in valvesStates)
            {
                ValveAction valveAction;
                try
                {
                    valveAction = new ValveAction(valve.ValveId,
                        "TURN_OFF",
                        _userRepository.GetLoggedUserByUsername(User.Identity.Name).Result.Id.ToString());
                }
                catch (Exception)
                {
                    return Json(new Error() { ErrorMessages = { "Failed to fetch logged user. Please reauthenticate!" } });
                }

                try
                {
                    await _mqttClient.Publish($"/{StaticVariables.SinkId}/valves/control/", JsonConvert.SerializeObject(valveAction, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }));
                }
                catch (Exception)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, $"Sending valve action request failed!");
                }
            }
            RequestStatus();
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public JsonResult GetValvesState()
        {
            try
            {
                RequestStatus();
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching Valves' status!" } });
            }

            return Json(GetLastValvesState());
        }

        [Authorize(Roles = "VIEWER,ADMINISTRATOR")]
        [HttpPost]
        public ActionResult GetIrrigationLogsByFilter([FromBody] IrrigationLogsFilter? filter)
        {
            if (filter == null)
            {
                return Json(new Error() { ErrorMessages = { "Filter is null!" } });
            }

            try
            {
                return Json(_irrigationRepository.GetAllByFilter(filter).Result.ToList());
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching irrigation logs!" } });
            }
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
            try
            {
                await _mqttClient.Publish($"/{StaticVariables.SinkId}/valves/status/request/", "{}");
            }
            catch (Exception)
            {
                /* Do nothing. */
            }
        }
    }
}
