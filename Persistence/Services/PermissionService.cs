using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class PermissionService(
        IUserRolePermissionRepository userRolePermissionRepository,
        ICurrentUserService currentUserService) : IPermissionService
    {
        private readonly IUserRolePermissionRepository _repository = userRolePermissionRepository;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        public async Task<bool> CheckPermission(string entityName, OperationType operation, User? user = null)
        {
            user ??= await _currentUserService.GetCurrentUserAsync()
                        ?? throw new UnauthorizedAccessException("User not found");

            var permission = GetPermission(entityName, operation);

            return await _repository.AnyAsync(x => x.UserRoleId == user.RoleId && x.Permission == permission);
        }

        public Permission GetPermission(string entityName, OperationType operation)
        {
            var permission = entityName.ToLower() switch
            {
                "family" => operation switch
                {
                    OperationType.GET => Permission.GET_FAMILY,
                    OperationType.CREATE => Permission.CREATE_FAMILY,
                    OperationType.UPDATE => Permission.UPDATE_FAMILY,
                    OperationType.DELETE => Permission.DELETE_FAMILY,
                    _ => throw new NotImplementedException($"Operation '{operation}' is not implemented for entity '{entityName}'.")
                },
                "member" => operation switch
                {
                    OperationType.GET => Permission.GET_MEMBER,
                    OperationType.CREATE => Permission.CREATE_MEMBER,
                    OperationType.UPDATE => Permission.UPDATE_MEMBER,
                    OperationType.DELETE => Permission.DELETE_MEMBER,
                    _ => throw new NotImplementedException($"Operation '{operation}' is not implemented for entity '{entityName}'.")
                },
                "user" => operation switch
                {
                    OperationType.GET => Permission.GET_USER,
                    OperationType.CREATE => Permission.CREATE_USER,
                    OperationType.UPDATE => Permission.UPDATE_USER,
                    OperationType.DELETE => Permission.DELETE_USER,
                    _ => throw new NotImplementedException($"Operation '{operation}' is not implemented for entity '{entityName}'.")
                },
                "userrole" => operation switch
                {
                    OperationType.GET => Permission.GET_ROLE,
                    OperationType.CREATE => Permission.CREATE_ROLE,
                    OperationType.UPDATE => Permission.UPDATE_ROLE,
                    OperationType.DELETE => Permission.DELETE_ROLE,
                    _ => throw new NotImplementedException($"Operation '{operation}' is not implemented for entity '{entityName}'.")
                },
                "uploadedfile" => operation switch
                {
                    OperationType.GET => Permission.GET_FILE,
                    OperationType.CREATE => Permission.CREATE_FILE,
                    OperationType.UPDATE => Permission.UPDATE_FILE,
                    OperationType.DELETE => Permission.DELETE_FILE,
                    _ => throw new NotImplementedException($"Operation '{operation}' is not implemented for entity '{entityName}'.")
                },
                "userrolepermission" => operation switch
                {
                    OperationType.GET => Permission.GET_ROLE_PERMISSION,
                    OperationType.CREATE => Permission.CREATE_ROLE_PERMISSION,
                    OperationType.UPDATE => Permission.UPDATE_ROLE_PERMISSION,
                    OperationType.DELETE => Permission.DELETE_ROLE_PERMISSION,
                    _ => throw new NotImplementedException($"Operation '{operation}' is not implemented for entity '{entityName}'.")
                },
                _ => throw new NotImplementedException($"Permission check for entity '{entityName}' is not implemented.")
            };

            return permission;
        }
    }
}
