using System;
using System.Threading.Tasks;
using CSC.CSClassroom.WebApp.Providers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CSC.CSClassroom.WebApp.ModelBinders
{
	/// <summary>
	/// Uses a DateTimeModelProvider for dates.
	/// </summary>
	public class DateTimeModelBinderProvider : IModelBinderProvider
	{
		/// <summary>
		/// The time zone provider.
		/// </summary>
		private ITimeZoneProvider _timeZoneProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DateTimeModelBinderProvider(ITimeZoneProvider timeZoneProvider)
		{
			_timeZoneProvider = timeZoneProvider;
		}

		/// <summary>
		/// Uses a DateTimeModelProvider for dates.
		/// </summary>
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context.Metadata.ModelType != typeof(DateTime)
				&& context.Metadata.ModelType != typeof(DateTime?))
			{
				return null;
			}

			return new DateTimeModelBinder(_timeZoneProvider);
		}
	}

	/// <summary>
	/// Binds a DateTime object.
	/// </summary>
	public class DateTimeModelBinder : IModelBinder
	{
		/// <summary>
		/// The time zone provider.
		/// </summary>
		private readonly ITimeZoneProvider _timeZoneProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DateTimeModelBinder(ITimeZoneProvider timeZoneProvider)
		{
			_timeZoneProvider = timeZoneProvider;
		}

		/// <summary>
		/// Binds the DateTime object.
		/// </summary>
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

			if (value.FirstValue == null && bindingContext.ModelType == typeof(DateTime?))
			{
				bindingContext.Result = ModelBindingResult.Success(null);
				return Task.CompletedTask;
			}

			DateTime dateTime;
			if (!DateTime.TryParse(value.FirstValue, out dateTime))
			{
				bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid date/time.");
				bindingContext.Result = ModelBindingResult.Failed();
				return Task.CompletedTask;
			}

			bindingContext.Result = ModelBindingResult.Success(_timeZoneProvider.ToUtcTime(dateTime));
			return Task.CompletedTask;
		}
	}
}
