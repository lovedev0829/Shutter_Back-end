using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.GHL.Services;

namespace ShutterManagement.Business.Services
{
    public class GhlCustomFieldsService : BaseRepository<ShutterDBContext, TblGhlCustomField>, IGhlCustomFieldsService
    {
        private readonly GHLService_v2 gHLService_V2;

        public GhlCustomFieldsService(GHLService_v2 gHLService_V2)
        {
            this.gHLService_V2 = gHLService_V2;
        }

        public async Task<IResponseVm<string>> SaveGhlCustomFieldsAsync(Guid? tenantId = null)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();

            try
            {
                var getFieldsResponse = await gHLService_V2.GetGhlCustomFieldsAsync();

                if (!getFieldsResponse.IsSuccess || getFieldsResponse.Data == null || !getFieldsResponse.Data.Any())
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Failed to fetch or no custom fields found.";
                    return responseVm;
                }

                var customFields = getFieldsResponse.Data;
                var currentTenantId = (tenantId != null && tenantId != Guid.Empty) ? tenantId.Value : AppConstant.LoggedInTenantId;

                foreach (var field in customFields)
                {
                    var existingField = GetAllQuerable().Where(x => x.TenantId == currentTenantId && x.FieldName == field.name).FirstOrDefault();

                    if (existingField != null)
                    {
                        existingField.GhlFieldId = field.id;
                        existingField.Model = field.model;
                        existingField.FieldKey = field.fieldKey;
                        existingField.Placeholder = field.placeholder;
                        existingField.DataType = field.dataType;
                        existingField.Position = field.position;
                        existingField.DocumentType = field.documentType;
                        existingField.ParentId = field.parentId;
                        existingField.LocationId = field.locationId;
                        existingField.DateAdded = field.dateAdded;
                        existingField.IsStandard = field.standard;
                        existingField.ModifiedAt = DateTime.Now;
                        existingField.IsDelete = false;
                        existingField.IsActive = true;

                        await Update(existingField);
                    }
                    else
                    {
                        var newField = new TblGhlCustomField
                        {
                            Id = Guid.NewGuid(),
                            GhlFieldId = field.id,
                            FieldName = field.name,
                            Model = field.model,
                            FieldKey = field.fieldKey,
                            Placeholder = field.placeholder,
                            DataType = field.dataType,
                            Position = field.position,
                            DocumentType = field.documentType,
                            ParentId = field.parentId,
                            LocationId = field.locationId,
                            DateAdded = field.dateAdded,
                            IsStandard = field.standard,
                            TenantId = currentTenantId,
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            IsDelete = false
                        };

                        await Add(newField);
                    }
                }
                responseVm.IsSuccess = true;
                responseVm.Message = "Custom fields saved successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Exception: {ex.Message}";
            }

            return responseVm;
        }


    }
}
