using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class TemplateService : BaseRepository<ShutterDBContext, TblTemplate>, ITemplatesService
    {
        private readonly ShutterDBContext context;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenantService;
        private readonly IMstPlaceholderService placeholders;

        public TemplateService(ShutterDBContext context,ClaimsProperty claimsProperty,ITenantService tenantService,IMstPlaceholderService placeholders)
        {
            this.context = context;
            this.claimsProperty = claimsProperty;
            this.tenantService = tenantService;
            this.placeholders = placeholders;
        }
        public async Task<IResponseVm<string>> ActiveInActive(Guid Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IstemplateNameExist = GetAllQuerable().Where(t => t.TemplateId == Id &&
                     t.IsDelete == false && t.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                    if (IstemplateNameExist != null)
                    {
                        IstemplateNameExist.IsActive = !IstemplateNameExist.IsActive;
                        IstemplateNameExist.UpdatedAt = DateTime.Now;
                        IstemplateNameExist.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(IstemplateNameExist);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.UpdateSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_Temaplate(CreateTemplate model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
            var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsDelete == false);
            if(IsTenant)
                {
                    var IstemplateNameExist = await AnyAsync(t => t.TemplateName == model.TemplateName &&
                    t.IsDelete == false && t.TenantId == claimsProperty.GetTenantId());
                    if(IstemplateNameExist)
                    {
                        responseVm.Message = "TemplateName" + " " + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    TblTemplate template = new TblTemplate
                    {
                        TemplateId = Guid.NewGuid(),
                        TemplateName = model.TemplateName,
                        TemplateValue = model.TemplateValue,
                        TenantId = claimsProperty.GetTenantId(),
                        IsActive = true,
                        IsDelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = AppConstant.LoggedInUserID
                    };
                    var res = await Add(template);
                   
                    string pattern = @"{{[a-zA-Z]+}}";
                    var matches = Regex.Matches(template.TemplateValue, pattern);

                    foreach (Match match in matches)
                    {
                        var placeholder = match.Value;
                        var IsplaceholderExist = placeholders.GetAllQuerable().Where(p => p.PlaceholderValue == placeholder && p.IsDelete == false).FirstOrDefault();
                        if (IsplaceholderExist == null)
                        {
                            MstPlaceholder placeholder1 = new MstPlaceholder
                            {
                                Id = Guid.NewGuid(),
                                PlaceholderValue = placeholder,
                                IsActive = true,
                                IsDelete = false
                            };
                            await placeholders.Add(placeholder1);
                            TblTemplatePlaceholder templatePlaceholder = new TblTemplatePlaceholder
                            {
                                TemplateId = template.TemplateId,
                                PlaceholderId = placeholder1.Id,
                                IsActive = true,
                                IsDelete = false,
                                CreatedAt = DateTime.Now,
                                CreatedBy = AppConstant.LoggedInUserID
                            };
                           context.TblTemplatePlaceholders.Add(templatePlaceholder);   
                        }
                        else
                        {
                            TblTemplatePlaceholder placeholder2 = new TblTemplatePlaceholder
                                {
                                 TemplateId = template.TemplateId,  
                                 PlaceholderId = IsplaceholderExist.Id,
                                 IsActive = true,
                                 IsDelete = false,
                                 CreatedAt = DateTime.Now,
                                 CreatedBy = AppConstant.LoggedInUserID
                            };
                            context.TblTemplatePlaceholders.Add(placeholder2);
                        }
                    }
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.InsertSuccessMsg;
                }
            }
            catch(Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Delete(Guid Id)
        {

            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
                if (IsTenant)
                {
                    var IstemplateNameExist = GetAllQuerable().Where(t => t.TemplateId == Id &&
                     t.IsDelete == false && t.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                    if (IstemplateNameExist != null)
                    {
                        IstemplateNameExist.IsDelete = true;
                        IstemplateNameExist.IsActive = false;
                        IstemplateNameExist.UpdatedAt = DateTime.Now;
                        IstemplateNameExist.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(IstemplateNameExist);
                        responseVm.IsSuccess = true;
                        responseVm.Message = AppConstant.DeleteSuccessMsg;
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                   
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
       
        public async Task<IResponseVm<List<GetAllTemplate>>> GetAll_Template(Pagination filter)
        {
            IResponseVm<List<GetAllTemplate>> responseVm = new ResponseVm<List<GetAllTemplate>>();
            try
            {
                var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.IsDelete == false && x.IsActive == true);

                if (IsTenant)
                {

                    var Templatelist = (await GetAll()).Where(t => t.TenantId == claimsProperty.GetTenantId() && t.IsDelete == false).ToList();
                    filter.SearchValue = filter.SearchValue?.Trim().ToLower();
                    var FilterRecords = Templatelist
                                    .Where(x =>
                                    (filter.Status == 0 ? x.IsActive == false : (filter.Status == 1 ? x.IsActive == true : true)) &&                           
                                     (string.IsNullOrEmpty(filter.SearchValue) ? true :
                                        (x.TemplateId.ToString() == filter.SearchValue)
                                        || (x.TemplateName ?? "").ToLower().Contains(filter.SearchValue)
                                        || (x.TemplateValue ?? "").ToLower().Contains(filter.SearchValue)))
                                    .ToList();

                    var TakenRecords = FilterRecords
                        .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                        .Take(filter.RecordPerPage)
                        .ToList();

                    responseVm.Data = TakenRecords.Select(x => new GetAllTemplate
                    {
                        TemplateId = x.TemplateId.ToString(),
                        IsActive = x.IsActive,
                        TemplateValue = x.TemplateValue,
                        TemplateName = x.TemplateName

                    }).ToList();
                    responseVm.TotalRecords = FilterRecords.Count;
                    responseVm.IsSuccess = true;
                    responseVm.Message = AppConstant.FetchSuccessMsg;
                   
                }
                else
                {
                    responseVm.Message = AppConstant.AccessDenied;
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }

            return responseVm;
        }

        public async Task<IResponseVm<dynamic>> Update_Template(UpdateTemplate model)
        {
            IResponseVm<dynamic> responseVm = new ResponseVm<dynamic>();
            try
            {
             var IsTenant = await tenantService.AnyAsync(x => x.TenantId == claimsProperty.GetTenantId() && x.ParentTenantId == null && x.IsActive == true && x.IsDelete == false);
              if(IsTenant)
                {
                    var IstemplateNameExist = GetAllQuerable().Where(t => t.TemplateId != Guid.Parse(model.TemplateId) &&
                    t.TemplateName == model.TemplateName && t.IsDelete == false && t.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();
                    if(IstemplateNameExist != null)
                    {
                        responseVm.Message = "TemplateName" + AppConstant.AlreadyExists;
                        return responseVm;
                    }
                    var IstemplateExist = GetAllQuerable().Where(t => t.TemplateId == Guid.Parse(model.TemplateId) &&
                    t.IsDelete == false && t.TenantId == claimsProperty.GetTenantId()).FirstOrDefault();

                    if (IstemplateExist != null)
                    {
                        IstemplateExist.TemplateName = model.TemplateName;
                        IstemplateExist.TemplateValue = model.TemplateValue;
                        IstemplateExist.UpdatedAt = DateTime.Now;
                        IstemplateExist.ModifiedBy = AppConstant.LoggedInUserID;
                        var res = await Update(IstemplateExist);

                        string pattern = @"{{[a-zA-Z]+}}";
                        var matches = Regex.Matches(IstemplateExist.TemplateValue, pattern);

                        var IsPlaceholderTemplateExist = context.TblTemplatePlaceholders.Where(x => x.TemplateId == IstemplateExist.TemplateId && x.IsDelete == false && x.TenantId
                        == claimsProperty.GetTenantId()).ToList();
                        if (IsPlaceholderTemplateExist != null)
                        {
                            foreach (var items in IsPlaceholderTemplateExist)
                            {
                                context.TblTemplatePlaceholders.Remove(items);
                                await context.SaveChangesAsync();
                            }
                        }
                        foreach (Match match in matches)
                        {
                            var placeholder = match.Value;
                            var IsplaceholderExist = placeholders.GetAllQuerable().Where(p => p.PlaceholderValue == placeholder && p.IsDelete == false).FirstOrDefault();

                            if (IsplaceholderExist == null)
                            {
                                MstPlaceholder placeholder1 = new MstPlaceholder
                                {
                                    Id = Guid.NewGuid(),
                                    PlaceholderValue = placeholder,
                                    IsActive = true,
                                    IsDelete = false
                                };
                                await placeholders.Add(placeholder1);
                                TblTemplatePlaceholder templatePlaceholder = new TblTemplatePlaceholder
                                {
                                    TemplateId = IstemplateExist.TemplateId,
                                    PlaceholderId = placeholder1.Id,
                                    IsActive = true,
                                    IsDelete = false,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID
                                };
                                context.TblTemplatePlaceholders.Add(templatePlaceholder);
                            }
                            else
                            {
                                TblTemplatePlaceholder placeholder2 = new TblTemplatePlaceholder
                                {
                                    TemplateId = IstemplateExist.TemplateId,
                                    PlaceholderId = IsplaceholderExist.Id,
                                    IsActive = true,
                                    IsDelete = false,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = AppConstant.LoggedInUserID
                                };
                                context.TblTemplatePlaceholders.Add(placeholder2);
                            }
                            responseVm.IsSuccess = true;
                            responseVm.Message = AppConstant.UpdateSuccessMsg;
                        }
                    }
                    else
                    {
                        responseVm.Message = AppConstant.NoDataFound;
                    }
                }
            }
            catch(Exception ex)
            {
                responseVm.Message = AppConstant.ExceptionError + ex.Message;
            }
            return responseVm;
        }
    }
}
