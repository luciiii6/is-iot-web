using IsIoTWeb.Models;
using IsIoTWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsIoTWeb.Controllers
{
    [Authorize(Roles = "ADMINISTRATOR")]
    public class UsersController : Controller
    {
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private ISinkRepository _sinkRepository;

        public UsersController(IUserRepository userRepository, IRoleRepository roleRepository, ISinkRepository sinkRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _sinkRepository = sinkRepository;
        }

        public IActionResult Index() => View();

        public async Task<ViewResult> Create()
        {
            try
            {
                List<Role> roleList = (List<Role>)await _roleRepository.GetAll();
                ViewBag.Roles = roleList;
            }
            catch (Exception)
            {
                ViewBag.Errors = "An error occured when fetching user's roles data!";
            }
            return View();
        }

        public ViewResult Details() => View();

        public async Task<ViewResult> Edit()
        {
            try
            {
                List<Role> roleList = (List<Role>)await _roleRepository.GetAll();
                ViewBag.Roles = roleList;
            }
            catch (Exception)
            {
                ViewBag.Errors = "An error occured when fetching user's roles data!";
            }
            return View();
        }

        public ActionResult GetUsers()
        {
            List<UserPresentation> userPresentations = new List<UserPresentation>();
            try
            {
                List<User> users = (List<User>)_userRepository.GetAll().Result;
                foreach(var user in users)
                {
                    var roleObjectId = user.Roles.FirstOrDefault();
                    userPresentations.Add( new UserPresentation()
                    {
                        Id = Utils.Utils.DynamicObjectIdToString(user.Id.Timestamp, user.Id.Machine, user.Id.Pid, user.Id.Increment),
                        Username = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        CreatedOn = user.CreatedOn.ToString(),
                        PhoneNumber = user.PhoneNumber,
                        Role = GetRoleName(Utils.Utils.DynamicObjectIdToString(roleObjectId.Timestamp, roleObjectId.Machine, roleObjectId.Pid, roleObjectId.Increment))
                    });
                }
            }
            catch (Exception)
            {
                return Json(new Error() { ErrorMessages = { "An error occured when fetching Users' data!" } });
            }
            return Json(userPresentations);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateInput userCreateInput)
        {
            if (ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                try
                {
                    errors = await _userRepository.Create(userCreateInput);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(userCreateInput);
                }

                try
                {
                    var user = _userRepository.GetByUsername(userCreateInput.Username);
                    await _sinkRepository.AddUser(user.Id.ToString());
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(userCreateInput);
                }

                if (errors != null)
                {
                    foreach (string error in errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View(userCreateInput);
                }
            }
            else
            {
                return View(userCreateInput);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GetId([FromBody] UserDynamicId id)
        {
            return Json(new { id = Utils.Utils.DynamicObjectIdToString(id.Timestamp, id.Machine, id.Pid, id.Increment) });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            User user;
            UserUpdateInput userUpdateInput = new UserUpdateInput();
            try
            {
                user = await _userRepository.Get(id);
                userUpdateInput.FirstName = user.FirstName;
                userUpdateInput.LastName = user.LastName;
                userUpdateInput.Email = user.Email;
                userUpdateInput.PhoneNumber = user.PhoneNumber;
                userUpdateInput.Username = user.UserName;
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }

            return View(userUpdateInput);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            User user;
            UserPresentation userPresenation;
            try
            {
                user = await _userRepository.Get(id);
                var roleObjectId = user.Roles.FirstOrDefault();
                userPresenation = new UserPresentation()
                {
                    Id = Utils.Utils.DynamicObjectIdToString(user.Id.Timestamp, user.Id.Machine, user.Id.Pid, user.Id.Increment),
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CreatedOn = user.CreatedOn.ToString(),
                    PhoneNumber = user.PhoneNumber,
                    Role = GetRoleName(Utils.Utils.DynamicObjectIdToString(roleObjectId.Timestamp, roleObjectId.Machine, roleObjectId.Pid, roleObjectId.Increment))
                };
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }

            return View(userPresenation);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _userRepository.Delete(id);
            }
            catch (Exception)
            {
                /* Do nothing. Return to index. */
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserUpdateInput userUpdateInput)
        {
            if (ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                try
                {
                    errors = await _userRepository.Update(userUpdateInput);
                }
                catch (Exception)
                {
                    return RedirectToAction("Index");
                }

                if (errors == null)
                {
                    ViewBag.Message = "User Updated Successfully";
                }
                else
                {
                    foreach (string error in errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    return View(userUpdateInput);
                }
            }
            else
            {
                return View(userUpdateInput);
            }
            return RedirectToAction("Index");
        }
        
        private string GetRoleName(string id)
        {
            return _roleRepository.Get(id).Result.Name;
        }
    }
}
