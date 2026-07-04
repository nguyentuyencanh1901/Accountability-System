using Example.Common.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Example.UserService.WebAdmin.ModelBinding
{
    /// <summary>Bind DateTimeOffset từ input text dd/MM/yyyy HH:mm.</summary>
    public class VietnamDateTimeOffsetModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;
            var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;

            if (string.IsNullOrWhiteSpace(value))
            {
                if (isNullable)
                    bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (VietnamTimeHelper.TryParseFormInput(value, out var parsed))
            {
                bindingContext.Result = ModelBindingResult.Success(parsed);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Định dạng ngày giờ không hợp lệ. Vui lòng nhập theo {VietnamTimeHelper.FormInputFormat}.");
            return Task.CompletedTask;
        }
    }

    public class VietnamDateTimeOffsetModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var modelType = context.Metadata.ModelType;
            if (modelType == typeof(DateTimeOffset) || modelType == typeof(DateTimeOffset?))
                return new VietnamDateTimeOffsetModelBinder();
            return null;
        }
    }
}
