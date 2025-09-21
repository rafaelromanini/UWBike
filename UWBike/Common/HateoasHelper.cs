using Microsoft.AspNetCore.Mvc;
using UWBike.Common;

namespace UWBike.Common
{
    public static class HateoasHelper
    {
        public static void AddHateoasLinks<T>(ApiResponse<T> response, string resourceName, object? resourceId, IUrlHelper urlHelper)
        {
            if (resourceId != null)
            {
                response.Links.Add(new Link(
                    urlHelper.Action("GetById", resourceName, new { id = resourceId })!,
                    "self"
                ));

                response.Links.Add(new Link(
                    urlHelper.Action("Update", resourceName, new { id = resourceId })!,
                    "update",
                    "PUT"
                ));

                response.Links.Add(new Link(
                    urlHelper.Action("Delete", resourceName, new { id = resourceId })!,
                    "delete",
                    "DELETE"
                ));
            }

            response.Links.Add(new Link(
                urlHelper.Action("GetAll", resourceName)!,
                "list"
            ));

            response.Links.Add(new Link(
                urlHelper.Action("Create", resourceName)!,
                "create",
                "POST"
            ));
        }

        public static void AddPaginationLinks<T>(PagedResult<T> pagedResult, string actionName, string controllerName, 
            IUrlHelper urlHelper, object? additionalParams = null)
        {
            // Self link
            pagedResult.Links.Add(new Link(
                urlHelper.Action(actionName, controllerName, 
                    MergeParams(new { pageNumber = pagedResult.PageNumber, pageSize = pagedResult.PageSize }, additionalParams))!,
                "self"
            ));

            // First page
            pagedResult.Links.Add(new Link(
                urlHelper.Action(actionName, controllerName, 
                    MergeParams(new { pageNumber = 1, pageSize = pagedResult.PageSize }, additionalParams))!,
                "first"
            ));

            // Last page
            pagedResult.Links.Add(new Link(
                urlHelper.Action(actionName, controllerName, 
                    MergeParams(new { pageNumber = pagedResult.TotalPages, pageSize = pagedResult.PageSize }, additionalParams))!,
                "last"
            ));

            // Previous page
            if (pagedResult.HasPrevious)
            {
                pagedResult.Links.Add(new Link(
                    urlHelper.Action(actionName, controllerName, 
                        MergeParams(new { pageNumber = pagedResult.PageNumber - 1, pageSize = pagedResult.PageSize }, additionalParams))!,
                    "prev"
                ));
            }

            // Next page
            if (pagedResult.HasNext)
            {
                pagedResult.Links.Add(new Link(
                    urlHelper.Action(actionName, controllerName, 
                        MergeParams(new { pageNumber = pagedResult.PageNumber + 1, pageSize = pagedResult.PageSize }, additionalParams))!,
                    "next"
                ));
            }
        }

        private static object MergeParams(object defaultParams, object? additionalParams)
        {
            if (additionalParams == null) return defaultParams;

            var result = new Dictionary<string, object?>();
            
            // Add default params
            foreach (var prop in defaultParams.GetType().GetProperties())
            {
                result[prop.Name] = prop.GetValue(defaultParams);
            }
            
            // Add additional params
            foreach (var prop in additionalParams.GetType().GetProperties())
            {
                var value = prop.GetValue(additionalParams);
                if (value != null)
                {
                    result[prop.Name] = value;
                }
            }
            
            return result;
        }
    }
}