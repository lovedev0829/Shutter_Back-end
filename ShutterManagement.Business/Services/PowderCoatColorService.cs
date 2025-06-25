using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class PowderCoatColorService : IPowderCoatColorService
    {
        private readonly IMapper mapper;
        private readonly ClaimsProperty claimsProperty;
        private readonly ShutterDBContext _context;

        public PowderCoatColorService(IMapper mapper, ShutterDBContext context, ClaimsProperty claimsProperty)
        {
            this.mapper = mapper;
            this.claimsProperty = claimsProperty;
            this._context = context;
        }

        public async Task<IResponseVm<string>> ActiveInActive(Guid? Id)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var existingColor = await _context.MstPowderCoatColors.FindAsync(Id);
                if (existingColor != null)
                {
                    existingColor.IsActive = !existingColor.IsActive;
                    existingColor.UpdatedAt = DateTime.Now;
                    existingColor.ModifiedBy = claimsProperty.GetUserId();
                    _context.MstPowderCoatColors.Update(existingColor);
                    await _context.SaveChangesAsync();
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Status updated successfully.";
                }
                else
                {
                    responseVm.Message = "Powder coat color not found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Create_PowderCoatColor(CreatePowderCoatColor model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var newColor = new MstPowderCoatColor
                {
                    Id = Guid.NewGuid(),
                    ColorName = model.ColorName,
                    Description = model.Description,
                    Cost = model.Cost,
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.Now,
                    TenantId = claimsProperty.GetTenantId(),
                    CreatedBy = claimsProperty.GetUserId()
                };

                _context.MstPowderCoatColors.Add(newColor);
                await _context.SaveChangesAsync();
                responseVm.IsSuccess = true;
                responseVm.Message = "Powder coat color created successfully.";
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> DeletePowderCoatColor(Guid? powderCoatColorId)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var existingColor = await _context.MstPowderCoatColors.FindAsync(powderCoatColorId);
                if (existingColor != null)
                {
                    existingColor.IsDelete = true;
                    existingColor.UpdatedAt = DateTime.Now;
                    existingColor.ModifiedBy = claimsProperty.GetUserId();
                    _context.MstPowderCoatColors.Update(existingColor);
                    await _context.SaveChangesAsync();
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Powder coat color deleted successfully.";
                }
                else
                {
                    responseVm.Message = "Powder coat color not found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<PowderCoatColorResponse>>> GetAllPowderCoatColor(Pagination filter)
        {
            IResponseVm<List<PowderCoatColorResponse>> responseVm = new ResponseVm<List<PowderCoatColorResponse>>();
            try
            {
                var currentTenantId = claimsProperty.GetTenantId();
                
                // Get current tenant and parent tenant IDs for sharing colors
                var tenantIds = new List<Guid> { currentTenantId };
                var parentTenantId = claimsProperty.GetParentTenantId();
                if (!string.IsNullOrEmpty(parentTenantId) && Guid.TryParse(parentTenantId, out var parentId))
                {
                    tenantIds.Add(parentId);
                }

                var query = _context.MstPowderCoatColors
                    .Where(x => x.IsDelete == false && tenantIds.Contains((Guid)x.TenantId))
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(filter.SearchValue))
                {
                    query = query.Where(x => 
                        x.ColorName.ToLower().Contains(filter.SearchValue.ToLower()) ||
                        x.Description.ToLower().Contains(filter.SearchValue.ToLower()));
                }

                // Apply column filters
                if (filter.ColumnFilters != null && filter.ColumnFilters.Any())
                {
                    foreach (var columnFilter in filter.ColumnFilters)
                    {
                        if (!string.IsNullOrEmpty(columnFilter.Value))
                        {
                            switch (columnFilter.Field.ToLower())
                            {
                                case "colorname":
                                    query = query.Where(x => x.ColorName.ToLower().Contains(columnFilter.Value.ToLower()));
                                    break;

                                case "description":
                                    query = query.Where(x => x.Description.ToLower().Contains(columnFilter.Value.ToLower()));
                                    break;
                                case "cost":
                                    if (decimal.TryParse(columnFilter.Value, out var costValue))
                                    {
                                        query = query.Where(x => x.Cost == costValue);
                                    }
                                    break;
                            }
                        }
                    }
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(filter.SortColumn))
                {
                    switch (filter.SortColumn.ToLower())
                    {
                        case "colorname":
                            query = filter.SortDirection?.ToLower() == "desc" 
                                ? query.OrderByDescending(x => x.ColorName)
                                : query.OrderBy(x => x.ColorName);
                            break;

                        case "description":
                            query = filter.SortDirection?.ToLower() == "desc" 
                                ? query.OrderByDescending(x => x.Description)
                                : query.OrderBy(x => x.Description);
                            break;
                        case "cost":
                            query = filter.SortDirection?.ToLower() == "desc" 
                                ? query.OrderByDescending(x => x.Cost)
                                : query.OrderBy(x => x.Cost);
                            break;
                        case "isactive":
                            query = filter.SortDirection?.ToLower() == "desc" 
                                ? query.OrderByDescending(x => x.IsActive)
                                : query.OrderBy(x => x.IsActive);
                            break;
                        default:
                            query = query.OrderByDescending(x => x.CreatedAt);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.CreatedAt);
                }

                var totalCount = await query.CountAsync();

                // Apply pagination
                var pageNo = filter.PageNo > 0 ? filter.PageNo : 1;
                var recordPerPage = filter.RecordPerPage > 0 ? filter.RecordPerPage : 10;
                var skip = (pageNo - 1) * recordPerPage;

                var colors = await query
                    .Skip(skip)
                    .Take(recordPerPage)
                    .Select(x => new PowderCoatColorResponse
                    {
                        Id = x.Id,
                        ColorName = x.ColorName,
                        Description = x.Description,
                        Cost = x.Cost,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToListAsync();

                responseVm.IsSuccess = true;
                responseVm.Data = colors;
                responseVm.TotalRecords = totalCount;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<List<PowderCoatColorResponse>>> GetPowderCoatColorsForDropdown()
        {
            IResponseVm<List<PowderCoatColorResponse>> responseVm = new ResponseVm<List<PowderCoatColorResponse>>();
            try
            {
                var currentTenantId = claimsProperty.GetTenantId();
                
                // Get current tenant and parent tenant IDs for sharing colors
                var tenantIds = new List<Guid> { currentTenantId };
                var parentTenantId = claimsProperty.GetParentTenantId();
                if (!string.IsNullOrEmpty(parentTenantId) && Guid.TryParse(parentTenantId, out var parentId))
                {
                    tenantIds.Add(parentId);
                }

                var colors = await _context.MstPowderCoatColors
                    .Where(x => x.IsDelete == false && x.IsActive == true && tenantIds.Contains((Guid)x.TenantId))
                    .OrderBy(x => x.ColorName)
                    .Select(x => new PowderCoatColorResponse
                    {
                        Id = x.Id,
                        ColorName = x.ColorName,
                        Description = x.Description,
                        Cost = x.Cost
                    })
                    .ToListAsync();

                responseVm.IsSuccess = true;
                responseVm.Data = colors;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }

        public async Task<IResponseVm<string>> Update_PowderCoatColor(UpdatePowderCoatColor model)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var existingColor = await _context.MstPowderCoatColors.FindAsync(model.Id);
                if (existingColor != null)
                {
                    existingColor.ColorName = model.ColorName;
                    existingColor.Description = model.Description;
                    existingColor.Cost = model.Cost;
                    existingColor.UpdatedAt = DateTime.Now;
                    existingColor.ModifiedBy = claimsProperty.GetUserId();
                    
                    _context.MstPowderCoatColors.Update(existingColor);
                    await _context.SaveChangesAsync();
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Powder coat color updated successfully.";
                }
                else
                {
                    responseVm.Message = "Powder coat color not found.";
                }
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }


    }
} 