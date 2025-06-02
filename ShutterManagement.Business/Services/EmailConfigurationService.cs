using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class EmailConfigurationService : BaseRepository<ShutterDBContext, TblEmailConfiguration>, IEmailConfigurationService
    {
        private readonly ITenantService tenantService;
        private readonly ClaimsProperty claimsProperty;

        public EmailConfigurationService(ITenantService tenantService, ClaimsProperty claimsProperty)
        {
            this.tenantService = tenantService;
            this.claimsProperty = claimsProperty;
        }
        public async Task<IResponseVm<EmailConfigurationVm>> GetAll_EmailConfigurations()
        {

            IResponseVm<EmailConfigurationVm> responseVm = new ResponseVm<EmailConfigurationVm>();
            try
            {
                var emailConfiguration = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive == true).FirstOrDefault();
                if (emailConfiguration != null)
                {
                    responseVm.Data = new EmailConfigurationVm
                    {
                        Id = emailConfiguration.Id,
                        SmtpServer = emailConfiguration.SmtpServer??"",
                        Port = emailConfiguration.Port,
                        Username = emailConfiguration.Username??"",
                        Password = emailConfiguration.Password ?? "",
                        UseSsl = emailConfiguration.UseSsl,
                        ReplyTo = emailConfiguration.ReplyTo ?? "",
                        EmailTypeName = dbContext.TblEmailTypes.Where(x => x.Id == emailConfiguration.EmailType).Select(s => s.Name).FirstOrDefault()??"",
                        EmailType = emailConfiguration.EmailType,
                        From = emailConfiguration.From,
                        AccessToken = emailConfiguration.AccessToken,
                        TenantId = emailConfiguration.TenantId,
                        IsActive = emailConfiguration.IsActive
                    };
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;

        }

        public async Task<IResponseVm<string>> AddOrUpdateEmailConfiguration(CreateEmailConfiguration emailConfigurationVm)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var existingConfig = GetAllQuerable().Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsDelete == false && x.IsActive==true).FirstOrDefault();
                if (existingConfig != null)
                {
                    existingConfig.SmtpServer = emailConfigurationVm.SmtpServer;
                    existingConfig.Port = emailConfigurationVm.Port;
                    existingConfig.Username = emailConfigurationVm.Username;
                    existingConfig.Password = emailConfigurationVm.Password;
                    existingConfig.UseSsl = emailConfigurationVm.UseSsl;
                    existingConfig.ReplyTo = emailConfigurationVm.ReplyTo;
                    existingConfig.EmailType = emailConfigurationVm.EmailType;
                    existingConfig.From = emailConfigurationVm.From;
                    existingConfig.AccessToken = emailConfigurationVm.AccessToken;
                    existingConfig.ModifiedBy = AppConstant.LoggedInUserID;
                    existingConfig.ModifiedAt = DateTime.Now;

                    await Update(existingConfig);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.UpdateSuccessMsg;
                }
                else
                {
                    TblEmailConfiguration newEmailConfig = new TblEmailConfiguration
                    {
                        Id = Guid.NewGuid(),
                        SmtpServer = emailConfigurationVm.SmtpServer,
                        Port = emailConfigurationVm.Port,
                        Username = emailConfigurationVm.Username,
                        Password = emailConfigurationVm.Password,
                        UseSsl = emailConfigurationVm.UseSsl,
                        ReplyTo = emailConfigurationVm.ReplyTo,
                        EmailType = emailConfigurationVm.EmailType,
                        TenantId = AppConstant.LoggedInTenantId,
                        CreatedBy = AppConstant.LoggedInUserID,
                        CreatedAt = DateTime.Now,
                        IsActive= true,
                        IsDelete = false,
                    };

                    await Add(newEmailConfig);
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }


        public async Task<IResponseVm<List<GetEmailTypesVm>>> GetAll_EmailTypeConfigurations()
        {
            IResponseVm<List<GetEmailTypesVm>> responseVm = new ResponseVm<List<GetEmailTypesVm>>();
            try
            {
                var emailTypes = dbContext.TblEmailTypes.ToList();

                var result = emailTypes.Select(x => new GetEmailTypesVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList();

                responseVm.Data = result;
                responseVm.IsSuccess = true;
                responseVm.Message = AppConstant.FetchSuccessMsg;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Error: {ex.Message}";
            }

            return responseVm;
        }



    }
}
