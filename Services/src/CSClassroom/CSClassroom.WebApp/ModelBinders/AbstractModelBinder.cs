using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CSC.CSClassroom.WebApp.ModelBinders
{
	/// <summary>
	/// Binds a question model.
	/// </summary>
	public class AbstractModelBinder : IModelBinder
	{
		/// <summary>
		/// The base type.
		/// </summary>
		private Type _baseType;

		/// <summary>
		/// The model metadata provider.
		/// </summary>
		private IDictionary<Type, IModelBinder> _derivedModelBinders;

		/// <summary>
		/// The model metadata provider.
		/// </summary>
		private IModelMetadataProvider _modelMetadataProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AbstractModelBinder(
			Type baseType,
			IDictionary<Type, IModelBinder> derivedModelBinders, 
			IModelMetadataProvider modelMetadataProvider)
		{
			_baseType = baseType;
			_derivedModelBinders = derivedModelBinders;
			_modelMetadataProvider = modelMetadataProvider;
		}

		/// <summary>
		/// Binds the model.
		/// </summary>
		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var typeFieldNamePrefix = !string.IsNullOrEmpty(bindingContext.ModelName)
				? $"{bindingContext.ModelName}."
				: "";

			var typeName = (string)bindingContext.ValueProvider
				.GetValue($"{typeFieldNamePrefix}{_baseType.Name}Type")
				.ConvertTo(typeof(string));

			if (typeName == null)
				return;
			
			var derivedModelBinderKvp = _derivedModelBinders.First(kvp => kvp.Key.Name == typeName);
			var derivedModelType = derivedModelBinderKvp.Key;
			var derivedModelBinder = derivedModelBinderKvp.Value;

			ModelBindingResult result;
			using (bindingContext.EnterNestedScope(
				_modelMetadataProvider.GetMetadataForType(derivedModelType),
				bindingContext.FieldName,
				bindingContext.ModelName,
				model: null
				))
			{
				await derivedModelBinder.BindModelAsync(bindingContext);
				result = bindingContext.Result;
			}

			bindingContext.Result = result;
		}
	}

	/// <summary>
	/// A model provider for the QuestionModelBinder.
	/// </summary>
	public class AbstractModelBinderProvider : IModelBinderProvider
	{
		/// <summary>
		/// The base types that will be supported.
		/// </summary>
		private ICollection<Type> _baseTypes;

		/// <summary>
		/// The cached subclass types for the base types.
		/// </summary>
		private IDictionary<Type, List<Type>> _subTypes;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AbstractModelBinderProvider(IEnumerable<Type> baseTypes)
		{
			_baseTypes = new HashSet<Type>(baseTypes);

			_subTypes = _baseTypes.ToDictionary
			(
				baseType => baseType,
				baseType => baseType.GetTypeInfo().Assembly
					.GetTypes()
					.Where(type => baseType.IsAssignableFrom(type))
					.Where(type => !type.GetTypeInfo().IsAbstract)
					.ToList()
			);
		}

		/// <summary>
		/// Returns the model binder for a question.
		/// </summary>
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			var modelType = context.Metadata.ModelType;

			if (!_baseTypes.Contains(modelType))
				return null;				

			var derivedModelBinders = _subTypes[modelType].ToDictionary
			(
				t => t,
				t => context.CreateBinder
				(
					context.MetadataProvider.GetMetadataForType(t)
				)
			);

			return new AbstractModelBinder(modelType, derivedModelBinders, context.MetadataProvider);
		}
	}
}
