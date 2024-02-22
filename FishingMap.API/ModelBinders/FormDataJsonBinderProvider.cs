using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FishingMap.API.ModelBinders
{
    public class FormDataJsonBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Do not use this provider for binding simple values
            if (!context.Metadata.IsComplexType) return null;

            // Do not use this provider if the binding target is not a property
            var propName = context.Metadata.PropertyName;
            if (propName == null) return null;

            var propInfo = context.Metadata.ContainerType?.GetProperty(propName);
            if (propInfo == null) return null;

            // Do not use this provider if the target property type implements IFormFile
            if (propInfo.PropertyType.IsAssignableFrom(typeof(IFormFile))) return null;

            // Do not use this provider if this property does not have the FromForm attribute
            if (!propInfo.GetCustomAttributes(typeof(FromFormAttribute), false).Any()) return null;

            // All criteria met; use the FormDataJsonBinder
            return new FormDataJsonBinder();
        }
    }
}
