using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Core.Common
{
    public class Helper
    {
        public static EmailTemplateType? GetTemplateType(EstimateOrOrderBaseVm model)
        {
            if (!string.IsNullOrEmpty(model.EstimateCode))
            {
                return model.Status switch
                {
                    nameof(Estimation.Approved) => EmailTemplateType.EstimateApproved,
                    nameof(Estimation.Rejected) => EmailTemplateType.EstimateRejected,
                    _ => null
                };
            }
            else if (!string.IsNullOrEmpty(model.OrderName))
            {
                return model.OrderStatus switch
                {
                    nameof(Estimation.Approved) => EmailTemplateType.OrderApproved,
                    nameof(Estimation.Rejected) => EmailTemplateType.OrderRejected,
                    _ => null
                };
            }

            return null;
        }

        public static string ToTitleCaseSafe(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
        }

        //public static string GetTemplateType(EstimateOrOrderBaseVm model)
        //{
        //    EmailTemplateType type = EmailTemplateType.Default;
        //    if (!string.IsNullOrEmpty(model.EstimateCode))
        //    {
        //        type = model.Status switch
        //        {
        //            nameof(Estimation.Approved) => EmailTemplateType.EstimateApproved,
        //            nameof(Estimation.Rejected) => EmailTemplateType.EstimateRejected,

        //        };
        //    }
        //    else if (!string.IsNullOrEmpty(model.OrderName))
        //    {
        //        type = model.OrderStatus switch
        //        {
        //            nameof(Estimation.Approved) => EmailTemplateType.OrderApproved,
        //            nameof(Estimation.Rejected) => EmailTemplateType.OrderRejected,

        //        };
        //    }
        //    return EnumValue.GetEnumValue(type).ToString();
        //}
    }
}
