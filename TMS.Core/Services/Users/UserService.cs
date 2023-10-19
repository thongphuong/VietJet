using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DAL.Repositories;
using DAL.UnitOfWork;
using TMS.Core.ViewModels;

namespace TMS.Core.Services.Users
{
    using ViewModels.UserModels;
    using DAL.Entities;
    using TMS.Core.Utils;
    using App_GlobalResources;
    using TMS.Core.Services.Configs;
    using Newtonsoft.Json.Linq;

    public class UserService : BaseService, IUserService
    {
        private readonly IRepository<USER> _repoUser;
        private readonly IRepository<UserRole> _repoUserRole;
        private readonly IRepository<UserPermission> _repoUserPermission;
        private readonly IRepository<Department> _repoDepartment;
        private readonly IRepository<Trainee> _repoEmployee;
        private readonly IRepository<GroupUser> _repoGroupUser;
        private readonly IRepository<GroupUserAccess> _repoGroupUserAccess;
        private readonly IConfigService configService;
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public UserService(IUnitOfWork unitOfWork, IConfigService _configService, IRepository<USER> repoUser, IRepository<UserRole> repoUserRole, IRepository<UserPermission> repoUserPermission, IRepository<Department> repoDepartment, IRepository<Trainee> repoEmployee, IRepository<GroupUser> repoGroupUser, IRepository<GroupUserAccess> repoGroupUserAccess, IRepository<Course> repoCourse, IRepository<SYS_LogEvent> repoSYS_LogEvent) : base(unitOfWork, repoCourse, repoSYS_LogEvent)
        {
            _repoUser = repoUser;
            _repoUserRole = repoUserRole;
            _repoUserPermission = repoUserPermission;
            _repoDepartment = repoDepartment;
            _repoEmployee = repoEmployee;
            _repoGroupUser = repoGroupUser;
            _repoGroupUserAccess = repoGroupUserAccess;
            configService = _configService;
        }

        public IQueryable<USER> GetAll(Expression<Func<USER, bool>> query = null, IEnumerable<int> departments = null, bool isMaster = false)
        {
            if (departments == null) return null;
            var entities = isMaster ? _repoUser.GetAll() : _repoUser.GetAll(a => a.UserPermissions.Any(x => departments.Any(z => z == x.DepartmentId)));
            return query != null
                ? entities.Where(query)
                : entities;
        }

        public USER Get(Expression<Func<USER, bool>> query = null)
        {
            var entity = _repoUser.Get(query);
            return entity;
        }

        public USER GetByName(string username)
        {
            var user = _repoUser.GetAll().Where(c => c.USERNAME.Equals(username)).FirstOrDefault();
            return user;
        }

        public USER GetById(int? id)
        {
            if (!id.HasValue) return null;
            return _repoUser.Get(id);
        }

        public void Insert(USER entity)
        {
            _repoUser.Insert(entity);
            Uow.SaveChanges();
        }

        public void Update(USER entity)
        {
            _repoUser.Update(entity);
            Uow.SaveChanges();
        }
        private void RemoveEntity(USER entity)
        {
            if (entity.UserRoles.Any())
            {
                _repoUserRole.Delete(entity.UserRoles);
            }
            if (entity.UserPermissions.Any())
            {
                _repoUserPermission.Delete(entity.UserPermissions);
            }
            _repoUser.Delete(entity);
        }
        public USER Modify(TMS.Core.ViewModels.UserModels.UserProfile model, string pass, string token)
        {
            var culture = GetCulture();
            if (model == null) return null;
            JObject check;
            //string oldPass = "";
            var flag = false;
            var entity = _repoUser.Get(model.Id);
            var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.UserName);
            if (model.UserName.Contains(" "))
            {
                throw new Exception(codeHasSpaceMessage);
            }
            if (_repoUser.GetAll(a => a.USERNAME == model.UserName && a.ID != model.Id).Any())
            {
                throw new Exception(string.Format(Messege.DataIsExists, Resource.lblUserName, model.UserName));
            }
            if (model.InstructorId.HasValue && !_repoEmployee.GetAll(a => a.IsDeleted == false && a.Id == model.InstructorId && a.int_Role == (int)UtilConstants.ROLE.Instructor).Any())
            {
                throw new Exception(Messege.EMPLOYEE_ISNOT_INTRUCTOR);
            }

            if (entity == null)
            {

                if (model.InstructorId.HasValue)
                {
                    var getInstructor = _repoEmployee.Get(a => a.IsDeleted == false && a.Id == model.InstructorId && a.int_Role == (int)UtilConstants.ROLE.Instructor);

                    //oldPass = Common.DecryptString(getInstructor.Password);
                    //check = Account_Authenticate_Tho (model.UserName, oldPass);

                    if (!string.IsNullOrEmpty(pass))
                    {
                        entity = new USER
                        {
                            CREATED_BY = CurrentUser.USER_ID.ToString(),
                            CREATION_DATE = DateTime.Now,
                            PASSWORD = model.Password,
                        };
                        var instruc = _repoEmployee.Get(model.InstructorId);
                        instruc.Password = model.Password;
                        _repoEmployee.Update(instruc);
                    }
                    else
                    {
                        entity = new USER
                        {
                            CREATED_BY = CurrentUser.USER_ID.ToString(),
                            CREATION_DATE = DateTime.Now,
                            PASSWORD = getInstructor.Password,
                        };
                    }
                    entity.USERNAME = getInstructor.str_Staff_Id;
                }
                else
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        throw new Exception(Messege.PASSWORD_REQUIRED);
                    }
                    entity = new USER
                    {
                        CREATED_BY = CurrentUser.USER_ID.ToString(),
                        CREATION_DATE = DateTime.Now,
                        PASSWORD = model.Password,
                        USERNAME = model.UserName,
                    };
                    //check = Account_Authenticate_Tho (model.UserName, pass);
                }

                if (model.Permissions != null)
                {
                    var invalidPermission =
                    _repoDepartment.GetAll(a => model.Permissions.Contains(a.Id) && a.IsDeleted == true && a.IsActive == false);
                    if (invalidPermission.Any()) { throw new Exception("User permission is invalid"); }
                    for (var i = 0; i < model.Permissions.Count(); i++)
                    {
                        entity.UserPermissions.Add(new UserPermission() { DepartmentId = model.Permissions[i] });
                    }
                }
                entity.IsDeleted = false;
                entity.ISACTIVE = 1;
                _repoUser.Insert(entity);
                //if(model.GroupUser != null && model.GroupUser.Any())
                //{
                //    entity.GroupUserAccesses =
                //        model.GroupUser.Select(a => new GroupUserAccess() {GroupUserId = a}).ToList();
                //}              
            }
            else
            {
                if (model.InstructorId.HasValue)
                {
                    var getInstructor = _repoEmployee.Get(a => a.IsDeleted == false && a.Id == model.InstructorId && a.int_Role == (int)UtilConstants.ROLE.Instructor);

                    if (string.IsNullOrEmpty(model.Password))
                    {
                        entity.PASSWORD = getInstructor.Password;
                        entity.LAST_UPDATED_BY = CurrentUser.USER_ID.ToString();
                        entity.LAST_UPDATE_DATE = DateTime.Now;
                    }
                    else
                    {
                        entity.PASSWORD = model.Password;
                        entity.LAST_UPDATED_BY = CurrentUser.USER_ID.ToString();
                        entity.LAST_UPDATE_DATE = DateTime.Now;
                        var instruc = _repoEmployee.Get(model.InstructorId);
                        instruc.Password = model.Password;
                        _repoEmployee.Update(instruc);
                    }

                    //oldPass = Common.DecryptString(getInstructor.Password);
                    //check = Account_Authenticate_Tho (model.UserName, oldPass);
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        entity.PASSWORD = model.Password;
                    }
                    entity.LAST_UPDATED_BY = CurrentUser.USER_ID.ToString();
                    entity.LAST_UPDATE_DATE = DateTime.Now;
                    pass = string.IsNullOrEmpty(pass) ? Common.DecryptString(entity.PASSWORD) : pass;
                    //check = Account_Authenticate_Tho (model.UserName, pass);
                }
                _repoUser.Update(entity);
                flag = true;
            }
            UpdateGroupUserAccess(ref entity, model.GroupUser);

            if (entity.UserRoles != null)
            {
                UpdateUser(ref entity, model.Role);
            }
            else
            {
                if (model.Role != null && model.Role.Any())
                {
                    entity.UserRoles = new List<UserRole>();
                    foreach (var role in model.Role)
                    {
                        entity.UserRoles.Add(new UserRole() { RoleId = role });
                    }
                }
            }

            //var fullnamelength = model.FullName.Split(' ').Length;
            //var firtName = model.FullName.Replace(model.FullName.Split(' ')[fullnamelength - 1], "").Trim();
            //var lastName = model.FullName.Split(' ')[fullnamelength - 1];
            //if (firtName == "" || firtName == " " || lastName == "" || lastName == " ") throw new Exception(Messege.VALIDATION_FULLNAME);

            //var culture_ = "en";
            //var firstName = " ";
            //if (model.FullName.Contains(" "))
            //{
            //    firstName = culture_ == "vi"
            //   ? model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim()
            //   : model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            //}
            //var lastName = model.FullName.Replace(firstName, "").Trim();
            //if (!string.IsNullOrEmpty(model.FullName))
            //{
            //    firstName = culture == "vi" ? model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim() : model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            //    lastName = model.FullName.Replace(firstName, "").Trim();
            //}
            //if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            //{
            //    if (!model.Id.HasValue)
            //    {
            //        RemoveEntity(entity);
            //    }
            //    throw new Exception(Messege.VALIDATION_FULLNAME);
            //}
            //entity.USERNAME = model.UserName;
            entity.FIRSTNAME = model.FirstName;
            entity.LASTNAME = model.LastName;
            entity.EMAIL = model.Email;
            entity.ADDRESS = model.Address;
            entity.PHONENO = model.Numbers;
            entity.DepartmentId = model.Department;
            entity.IsDeleted = false;
            entity.LANGUAGEABBREVIATION = "vi-VN";
            entity.InstructorId = model.InstructorId;
            entity.Avatar = string.IsNullOrEmpty(model.ImgAvatar) ? entity.Avatar : model.ImgAvatar;
            Uow.SaveChanges();
            return entity;

        }

        private string GetCulture()
        {
            var culture = "en";
            var cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            if (cultureCookie != null)
            {
                culture = cultureCookie.Value;
            }

            return culture;
        }
        public USER ChangeProfile(ChangeUserProfile model)
        {
            var culture = GetCulture();
            if (model == null) return null;
            var entity = _repoUser.Get(model.Id);
            if (entity == null)
            {
                throw new Exception("data is not found");
            }
            var checkEmail = entity.InstructorId.HasValue ? _repoEmployee.GetAll(a => a.IsDeleted == false && a.str_Email.Equals(model.Email) && a.Id != entity.InstructorId).Any() : _repoUser.GetAll(a => a.IsDeleted == false && a.EMAIL.Equals(model.Email) && a.ID != model.Id).Any();
            if (!string.IsNullOrEmpty(model.Email) && checkEmail)
            {
                throw new Exception(string.Format(Messege.DataIsExists, Resource.lblEmail, model.Email));
            }

            //firstName == lastName
            var firstName = culture == "vi" ? model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim() : model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            //lastName == firstName
            var lastName = model.FullName.Replace(firstName, "").Trim();
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                throw new Exception(Messege.VALIDATION_FULLNAME);
            }
            entity.LAST_UPDATED_BY = CurrentUser.USER_ID.ToString();
            entity.LAST_UPDATE_DATE = DateTime.Now;
            entity.FIRSTNAME = lastName;
            entity.LASTNAME = firstName;
            entity.EMAIL = model.Email;
            entity.Avatar = string.IsNullOrEmpty(model.ImgAvatar) ? entity.Avatar : model.ImgAvatar;
            entity.ADDRESS = model.Address;
            entity.PHONENO = model.Numbers;
            _repoUser.Update(entity);
            Uow.SaveChanges();

            return entity;

        }

        private void UpdateUser(ref USER entity, IEnumerable<int> roles)
        {
            _repoUserRole.Delete(entity.UserRoles);
            entity.UserRoles.Clear();
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    entity.UserRoles.Add(new UserRole() { RoleId = role });
                }
            }
        }
        private void UpdateGroupUserAccess(ref USER entity, IEnumerable<int> groupUsers)
        {
            if (entity.GroupUserAccesses != null && entity.GroupUserAccesses.Any())
            {
                _repoGroupUserAccess.Delete(entity.GroupUserAccesses);
                entity.GroupUserAccesses.Clear();
            }
            if (groupUsers != null)
            {
                entity.GroupUserAccesses = new List<GroupUserAccess>();
                foreach (var groupUser in groupUsers)
                {
                    entity.GroupUserAccesses.Add(new GroupUserAccess()
                    {
                        GroupUserId = groupUser
                    });
                }
            }
        }
        public void GrantPermission(int userId, int permissionId)
        {
            var entity = _repoUser.Get(userId);
            if (entity == null) throw new Exception("Data is not found");
            var permission = entity.UserPermissions.FirstOrDefault(a => a.DepartmentId == permissionId);
            if (permission == null)
            {
                var entityDepartment = _repoDepartment.Get(permissionId);
                var departments =
                   _repoDepartment.GetAll(
                       a => a.IsActive == true && a.IsDeleted == false && a.Ancestor.StartsWith(entityDepartment.Ancestor)).Select(a => a.Id).ToList();
                foreach (var department in departments.Where(department => entity.UserPermissions.All(x => x.DepartmentId != department)))
                {
                    entity.UserPermissions.Add(new UserPermission()
                    {
                        UserId = entity.ID,
                        DepartmentId = department,
                    });
                }
            }
            else
            {
                var departments =
                    _repoDepartment.GetAll(
                        a => a.IsActive == true && a.IsDeleted == false && a.Ancestor.StartsWith(permission.Department.Ancestor)).Select(a => a.Id).ToList();
                var userDepartments = entity.UserPermissions.Where(a => departments.Contains((int)a.DepartmentId)).ToList();
                foreach (var userPermission in userDepartments)
                {
                    _repoUserPermission.Delete(userPermission);
                }
            }
            Uow.SaveChanges();
        }
        //TODO:DELETE
        public int Insert(TMS.Core.ViewModels.UserModels.UserProfile model)
        {
            var checkUserName = _repoUser.GetAll(a => a.USERNAME.Equals(model.UserName) && a.ID != model.Id).Any();
            if (checkUserName)
            {
                throw new Exception(string.Format(Messege.DataIsExists, Resource.lblUserName, model.UserName));
            }
            var entity = new USER();

            //firstName == lastName
            var firstName = model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
            //lastName == firstName
            var lastName = model.FullName.Replace(firstName, "").Trim();
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                throw new Exception(Messege.VALIDATION_FULLNAME);
            }

            //var fullnamelength = model.FullName.Split(' ').Length;
            //var firtName = model.FullName.Replace(model.FullName.Split(' ')[fullnamelength - 1], "").Trim();
            //var lastName = model.FullName.Split(' ')[fullnamelength - 1];

            entity.USERNAME = model.UserName.Trim();
            entity.FIRSTNAME = firstName;
            entity.LASTNAME = lastName;
            entity.ISACTIVE = 1;
            entity.ADDRESS = model.Address.Trim();
            entity.PASSWORD = model.Password.Trim();
            entity.EMAIL = model.Email.Trim();
            entity.PHONENO = model.Numbers.Trim();
            entity.DepartmentId = model.Department;
            _repoUserRole.Delete(entity.UserRoles);
            foreach (var r in model.Role)
            {
                entity.UserRoles.Add(new UserRole()
                {
                    RoleId = r,
                    UserId = entity.ID,
                });
            }
            _repoUser.Insert(entity);
            Uow.SaveChanges();
            return entity.ID;
        }

        //TODO:DELETE
        public void Update(TMS.Core.ViewModels.UserModels.UserProfile model)
        {
            var entity = _repoUser.Get(model.Id);
            if (entity == null)
            {
                throw new Exception("data is not found");
            }
            var checkUserName = _repoUser.GetAll(a => a.USERNAME.Equals(model.UserName) && a.ID != model.Id).Any();
            if (checkUserName)
            {
                throw new Exception(string.Format(Messege.DataIsExists, Resource.lblUserName, model.UserName));
            }
            //firstName == lastName
            var firstName = model.FullName.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
            //lastName == firstName
            var lastName = model.FullName.Replace(firstName, "").Trim();
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                throw new Exception(Messege.VALIDATION_FULLNAME);
            }

            //var fullnamelength = model.FullName.Split(' ').Length;
            //var firtName = model.FullName.Replace(model.FullName.Split(' ')[fullnamelength - 1], "").Trim();
            //var lastName = model.FullName.Split(' ')[fullnamelength - 1];

            entity.USERNAME = model.UserName.Trim();
            entity.FIRSTNAME = firstName;
            entity.LASTNAME = lastName;
            entity.ISACTIVE = 1;
            entity.ADDRESS = model.Address.Trim();
            entity.PASSWORD = model.Password.Trim();
            entity.EMAIL = model.Email.Trim();
            entity.PHONENO = model.Numbers.Trim();
            entity.DepartmentId = model.Department;
            if (entity.UserRoles.Any()) _repoUserRole.Delete(entity.UserRoles);
            entity.UserRoles.Clear();
            foreach (var r in model.Role)
            {
                entity.UserRoles.Add(new UserRole()
                {
                    RoleId = r,
                    UserId = entity.ID,
                });
            }
            if (entity.UserPermissions.Any())
                _repoUserPermission.Delete(entity.UserPermissions);

            _repoUser.Update(entity);
            Uow.SaveChanges();
        }
    }
}
